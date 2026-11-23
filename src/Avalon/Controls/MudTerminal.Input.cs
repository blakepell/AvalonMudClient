using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

public partial class MudTerminal
{
    private static readonly HashSet<string> Directions = ["n", "s", "e", "w", "nw", "sw", "ne", "se", "u", "d"];
    private static readonly TimeSpan MultilineSendInterval = TimeSpan.FromMilliseconds(500);
    private static readonly SolidColorBrush CompletionBackground = CreateFrozenBrush(0x23, 0x23, 0x25);
    private readonly List<string> _inputHistory = [];
    private CompletionWindow? _completionWindow;
    private ModalDialog? _multilineDialog;

    /// <summary>
    /// Gets or sets the position in the input history the Up and Down keys walk from. -1 means the
    /// user is not walking the history.
    /// </summary>
    private int InputHistoryPosition { get; set; } = -1;

    /// <summary>
    /// The PreviewKeyDown event for the input text box used to setup special behavior from that box.
    /// </summary>
    private async void Input_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Capture this in case _completionWindow changes during the course of this function.
        var autoComplete = _completionWindow;
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;

                // If the completion window is visible take the selected item as the command.
                if (autoComplete is { IsVisible: true, CompletionList.SelectedItem.Text: { } selected })
                {
                    Input.Text = selected;
                    autoComplete.Close();
                }

                // Make sure the newline didn't make it into the text input, then select all in the
                // box so it can be cleared quickly.
                string command = Input.Text.ReplaceLineEndings(string.Empty);
                Input.Text = command;
                Input.SelectAll();
                AddInputHistory(command);

                _completionWindow?.Close();

                // If the user wants the input box to clear after a command, make it so.
                if (_settings.InputBoxClearAfterCommand)
                {
                    Input.Text = "";
                }

                // Set the history count to the end
                InputHistoryPosition = -1;

                await SendAsync(session => Interpreter.SendAsync(command, session.Cancellation.Token));
                break;

            case Key.Up:
                // Let the auto-completion window have the key when it's visible.
                if (autoComplete is { IsVisible: true })
                {
                    return;
                }

                // Go to the previous item in the history.
                Input.Text = InputHistoryNext();
                Input.Select(Input.Text.Length, 0);
                break;

            case Key.Down:
                // Let the auto-completion window have the key when it's visible.
                if (autoComplete is { IsVisible: true })
                {
                    return;
                }

                // Go to the next item in the history.
                Input.Text = InputHistoryPrevious();
                Input.Select(Input.Text.Length, 0);
                break;

            case Key.PageUp:
                // Let the auto-completion window have the key when it's visible.
                if (autoComplete is { IsVisible: true })
                {
                    return;
                }

                e.Handled = true;
                BackBufferPageUp(shift);
                break;

            case Key.PageDown:
                // Let the auto-completion window have the key when it's visible.
                if (autoComplete is { IsVisible: true })
                {
                    return;
                }

                e.Handled = true;
                BackBufferPageDown();
                break;

            case Key.Oem5:
            case Key.OemBackslash:
                Input.SelectAll();
                e.Handled = true;
                break;

            case Key.Escape:
                if (autoComplete is { IsVisible: true })
                {
                    autoComplete.Close();
                    return;
                }

                // Collapse the back buffer so it hides it and reclaims the space for the main
                // terminal, then reset the input history position and clear the input box.
                CloseBackBuffer();
                break;

            case Key.Tab:
                // If they're holding down Shift+Tab or they have the advanced auto complete disabled
                // then use the old auto-complete behavior.
                if (shift || !Game.AutoCompleteEnabled)
                {
                    ShiftTab(e);
                    return;
                }

                e.Handled = true;

                if (_inputHistory.Count == 0 && Game.Aliases.Count == 0 && Game.AutoCompleteList.Count == 0)
                {
                    return;
                }

                if (autoComplete is { IsVisible: true })
                {
                    // Let the auto complete's behavior handle the tab key if it's visible.
                    e.Handled = false;
                }
                else
                {
                    autoComplete?.Close();
                    ShowAutoCompletes(_inputHistory
                        .Concat(Game.Aliases.Select(alias => alias.AliasExpression))
                        .Concat(Game.AutoCompleteList));
                }

                break;

            case Key.Back:
                // If the auto-complete is visible and there's nothing in the command line then a
                // backspace will close it.
                if (autoComplete is { IsVisible: true } && string.IsNullOrWhiteSpace(Input.Text))
                {
                    autoComplete.Close();
                }

                break;
        }
    }

    /// <summary>
    /// Handles Shift+Tab in the command box, completing from the input history and then aliases.
    /// </summary>
    private void ShiftTab(KeyEventArgs e)
    {
        e.Handled = true;
        string prefix = Input.Text;

        // First Pass: Input History.  If there is no input get the last entered command, otherwise
        // search the input history for the command (searching the latest entries backwards).
        string? command = string.IsNullOrWhiteSpace(prefix)
            ? _inputHistory.LastOrDefault()
            : _inputHistory.FindLast(item => item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        // Second Pass: Aliases
        if (string.IsNullOrWhiteSpace(command))
        {
            command = Game.Aliases.Select(alias => alias.AliasExpression)
                .FirstOrDefault(item => item?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (!string.IsNullOrWhiteSpace(command))
        {
            Input.Text = command;
            Input.Select(Input.Text.Length, 0);
        }
    }

    /// <summary>
    /// Shows a completion window above the command box with the entries that start with the
    /// entire text of the command box.
    /// </summary>
    private void ShowAutoCompletes(IEnumerable<string> items)
    {
        var textArea = Input.TextArea;
        var document = Input.Document;

        // The entire text is the prefix for command-style completion.
        string entireText = document.Text;

        var candidates = items
            .Where(item => !string.IsNullOrWhiteSpace(item) &&
                (entireText.Length == 0 || item.StartsWith(entireText, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (candidates.Length == 0)
        {
            return;
        }

        var window = new CompletionWindow(textArea)
        {
            // Replace the entire text content, start from the beginning and end at the end.
            StartOffset = 0,
            EndOffset = document.TextLength,
            Background = CompletionBackground,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            Width = double.IsFinite(Game.AutoCompleteWidth) && Game.AutoCompleteWidth > 0 ? Game.AutoCompleteWidth : 300
        };

        foreach (string candidate in candidates)
        {
            window.CompletionList.CompletionData.Add(new CommandCompletion(candidate));
        }

        // Preselect the current prefix if there is one.
        if (entireText.Length > 0)
        {
            window.CompletionList.SelectItem(entireText);
        }

        _completionWindow = window;
        textArea.TextEntering += AcceptCompletionBeforePunctuation;
        window.Closed += (_, _) =>
        {
            textArea.TextEntering -= AcceptCompletionBeforePunctuation;
            if (ReferenceEquals(_completionWindow, window))
            {
                _completionWindow = null;
            }
        };

        window.Show();

        // Position upward, this will always be the case because the command box is at the bottom of
        // the terminal.  Re-anchor when the list is filtered and the window changes height.
        PositionAboveCommandBox(window);
        window.SizeChanged += (_, _) => PositionAboveCommandBox(window);
    }

    /// <summary>
    /// Places the completion window directly above the command box, accounting for DPI scaling.
    /// </summary>
    private void PositionAboveCommandBox(Window window)
    {
        if (PresentationSource.FromVisual(Input)?.CompositionTarget is not { } target)
        {
            return;
        }

        Point topLeft = target.TransformFromDevice.Transform(Input.PointToScreen(new Point(0, 0)));
        window.Left = topLeft.X;
        window.Top = topLeft.Y - window.ActualHeight;
    }

    /// <summary>
    /// If the user typed a non-word character, complete now (the common UX).
    /// </summary>
    private void AcceptCompletionBeforePunctuation(object sender, TextCompositionEventArgs e)
    {
        if (e.Text is { Length: > 0 } text && !IsWordChar(text[0]))
        {
            _completionWindow?.CompletionList.RequestInsertion(e);
        }
    }

    private static bool IsWordChar(char ch) => char.IsLetterOrDigit(ch) || ch is '_' or '-' or ' ';

    /// <summary>
    /// Adds a command into the history buffer.  Blank commands, a repeat of the last command and
    /// directions (which are spammed) are not added.
    /// </summary>
    private void AddInputHistory(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        if (_inputHistory.Count > 0 && _inputHistory[^1] == command)
        {
            return;
        }

        if (command.Length > 2 || !Directions.Contains(command))
        {
            _inputHistory.Add(command);
        }

        InputHistoryPosition = -1;
    }

    /// <summary>
    /// Returns the next (older) item in the history.
    /// </summary>
    private string InputHistoryNext()
    {
        if (InputHistoryPosition < _inputHistory.Count - 1)
        {
            InputHistoryPosition++;
        }

        if (_inputHistory.Count == 0 || InputHistoryPosition < 0)
        {
            return "";
        }

        return _inputHistory[_inputHistory.Count - InputHistoryPosition - 1];
    }

    /// <summary>
    /// Returns the previous (newer) item in the history.
    /// </summary>
    private string InputHistoryPrevious()
    {
        if (InputHistoryPosition > 0)
        {
            InputHistoryPosition--;
        }

        if (_inputHistory.Count == 0 || InputHistoryPosition < 0)
        {
            return "";
        }

        return _inputHistory[_inputHistory.Count - InputHistoryPosition - 1];
    }

    private void MultilineButton_OnClick(object sender, RoutedEventArgs e) => ShowMultilineDialog();

    private bool IsPrivateInput => PrivateInputPanel.Visibility == Visibility.Visible;

    private void SetPrivateInput(bool enabled)
    {
        if (enabled == IsPrivateInput)
        {
            return;
        }

        bool restoreFocus = CommandInput.IsKeyboardFocusWithin && IsConnected;
        if (enabled)
        {
            _completionWindow?.Close();
            _multilineDialog?.Close(false);
            PrivateInputPanel.Visibility = PeekButton.Visibility = Visibility.Visible;
            if (restoreFocus)
            {
                FocusPrivateInput();
            }

            Input.Visibility = MultilineButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            Input.Visibility = MultilineButton.Visibility = Visibility.Visible;
            if (restoreFocus)
            {
                Input.TextArea.Focus();
            }

            PrivateInputPanel.Visibility = PeekButton.Visibility = Visibility.Collapsed;
            ClearPrivateInput();
            PeekButton.IsChecked = false;
            PrivateInput.Visibility = Visibility.Visible;
            RevealedInput.Visibility = Visibility.Collapsed;
        }
    }

    private void FocusPrivateInput()
    {
        if (PeekButton.IsChecked == true)
        {
            RevealedInput.Focus();
        }
        else
        {
            PrivateInput.Focus();
        }
    }

    private void PeekButton_OnToggled(object sender, RoutedEventArgs e)
    {
        if (!IsPrivateInput)
        {
            return;
        }

        if (PeekButton.IsChecked == true)
        {
            RevealedInput.Text = PrivateInput.Password;
            RevealedInput.Visibility = Visibility.Visible;
            FocusPrivateInput();
            RevealedInput.CaretIndex = RevealedInput.Text.Length;
            PrivateInput.Visibility = Visibility.Collapsed;
            PrivateInput.Clear();
        }
        else
        {
            PrivateInput.Password = RevealedInput.Text;
            PrivateInput.Visibility = Visibility.Visible;
            FocusPrivateInput();
            EditingCommands.MoveToDocumentEnd.Execute(null, PrivateInput);
            RevealedInput.Visibility = Visibility.Collapsed;
            RevealedInput.Clear();
        }
    }

    private void ClearPrivateInput()
    {
        PrivateInput.Clear();
        RevealedInput.Clear();
    }

    private void RevealedInput_OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
    }

    private async void PrivateInput_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        e.Handled = true;
        string input = PeekButton.IsChecked == true ? RevealedInput.Text : PrivateInput.Password;
        ClearPrivateInput();
        await SendAsync(session => Interpreter.SendRawAsync(input, session.Cancellation.Token));
    }

    /// <summary>
    /// Shows the Send Multiline Text dialog and sends each line to the game.
    /// </summary>
    public async void ShowMultilineDialog()
    {
        if (!IsConnected || _session is not { } session || IsPrivateInput ||
            Window.GetWindow(this) is not { Content: UIElement host } window)
        {
            return;
        }

        var editor = new TextBox
        {
            AcceptsReturn = true,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        var send = new Button { Content = "Send", IsEnabled = false, MinWidth = 75, Margin = new Thickness(8, 0, 0, 0) };
        var cancel = new Button { Content = "Cancel", MinWidth = 75 };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
            Children = { cancel, send }
        };
        DockPanel.SetDock(buttons, Dock.Bottom);
        KeyboardNavigation.SetTabIndex(editor, 0);
        KeyboardNavigation.SetTabIndex(buttons, 1);
        AutomationProperties.SetName(editor, "Multiline game text");
        var dialog = new ModalDialog
        {
            Style = (Style)FindResource("MultilineDialogStyle"),
            Title = "Send Multiline Text",
            Width = 700,
            Height = 380,
            MaxWidth = Math.Max(240, window.ActualWidth - 60),
            MaxHeight = Math.Max(200, window.ActualHeight - 60),
            Content = new DockPanel { Children = { buttons, editor } }
        };
        editor.TextChanged += (_, _) => send.IsEnabled = !string.IsNullOrWhiteSpace(editor.Text);
        editor.Loaded += (_, _) => editor.Focus();
        cancel.Click += (_, _) => dialog.Close(false);
        send.Click += (_, _) => dialog.Close(true);

        MultilineButton.IsEnabled = false;
        _multilineDialog = dialog;
        try
        {
            bool? result = await dialog.ShowAsync(host);
            FocusInput();
            if (result == true && ReferenceEquals(_session, session))
            {
                await SendAsync(active => SendLinesAsync(active, editor.Text));
            }
        }
        finally
        {
            _multilineDialog = null;
            MultilineButton.IsEnabled = true;
        }
    }

    private async Task SendLinesAsync(Session session, string text)
    {
        var token = session.Cancellation.Token;
        var lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                await Task.Delay(MultilineSendInterval, token);
            }

            if (Interpreter.ServerEchoEnabled)
            {
                return;
            }

            await Interpreter.SendAsync(lines[index], token);
        }
    }

    private void MudTerminal_OnUnloaded(object sender, RoutedEventArgs e) => _completionWindow?.Close();

    private static SolidColorBrush CreateFrozenBrush(byte red, byte green, byte blue)
    {
        var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
        brush.Freeze();
        return brush;
    }

    private sealed class CommandCompletion(string command) : ICompletionData
    {
        public ImageSource? Image => null;
        public string Text => command;
        public object Content => command;
        public object? Description => null;
        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var document = textArea.Document;
            document.Replace(0, document.TextLength, command);
            textArea.Caret.Offset = command.Length;
            textArea.ClearSelection();
        }
    }
}
