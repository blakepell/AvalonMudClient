using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Avalon.Terminal;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;

namespace Avalon.Controls;

public partial class MudTerminal
{
    private static readonly SolidColorBrush SearchMarkerBrush = CreateFrozenBrush(0x33, 0x99, 0xFF);
    private SearchPanel _backBufferSearch = null!;

    private void InitializeBackBuffer()
    {
        BackBuffer.Document = Output.Document;
        BackBuffer.TextArea.TextView.LineTransformers.Add(_transcript);
        _backBufferSearch = SearchPanel.Install(BackBuffer);
        _backBufferSearch.MarkerBrush = SearchMarkerBrush;
    }

    private bool IsBackBufferVisible => BackBuffer.Visibility == Visibility.Visible;

    /// <summary>
    /// Shared preview key down logic between the game terminal and its back buffer.  If they have
    /// focus this implements page up and page down so the back buffer shows all the paging, once it
    /// gets to the bottom it will disappear.  The escape key sends the focus back to the input box
    /// hiding the back buffer.
    /// </summary>
    private void Terminal_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.PageUp:
                e.Handled = true;
                BackBufferPageUp(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                break;
            case Key.PageDown:
                e.Handled = true;
                BackBufferPageDown();
                break;
            case Key.Escape:
                CloseBackBuffer();
                break;
            case Key.Enter:
                FocusInput();
                break;
        }
    }

    /// <summary>
    /// Common KeyDown event for the terminals.  Since they are read only, anytime someone clicks into
    /// one of them and types the keys go straight to the input box.
    /// </summary>
    private void Terminal_OnKeyDown(object sender, KeyEventArgs e)
    {
        // Ignore if either of the control keys are down, this allows Ctrl+C to copy text that's
        // selected out of the given terminal (and Ctrl+F to open the search panel).
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            return;
        }

        // If the back buffer and its search panel are visible allow text to go through.
        if (IsBackBufferVisible && _backBufferSearch.IsVisible)
        {
            return;
        }

        if (!IsConnected)
        {
            return;
        }

        if (IsPrivateInput)
        {
            FocusPrivateInput();
            return;
        }

        // Set the focus of the keyboard to the input text box.
        Input.CaretOffset = Input.Document.TextLength;
        Input.TextArea.Focus();
    }

    /// <summary>
    /// The middle mouse button closes the back buffer.
    /// </summary>
    private void Terminal_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            CloseBackBuffer();
        }
    }

    /// <summary>
    /// Shows the back buffer scrolled to the bottom (or the top when <paramref name="toTop"/> is set),
    /// or pages it up if it's already showing.
    /// </summary>
    private void BackBufferPageUp(bool toTop)
    {
        if (!IsBackBufferVisible)
        {
            ShowBackBuffer(toTop);
            return;
        }

        BackBuffer.PageUp();
    }

    /// <summary>
    /// Pages the back buffer down, collapsing it once its last line is visible since the main
    /// terminal shows everything at the end.
    /// </summary>
    private void BackBufferPageDown()
    {
        if (IsBackBufferVisible)
        {
            BackBuffer.PageDown();
        }

        if (IsLastLineVisible(BackBuffer))
        {
            BackBuffer.Visibility = Visibility.Collapsed;
            _backBufferSearch.Close();
            FocusInput();
        }
    }

    private void ShowBackBuffer(bool toTop)
    {
        BackBuffer.Visibility = Visibility.Visible;
        BackBuffer.UpdateLayout();

        if (toTop)
        {
            BackBuffer.ScrollToHome();
        }
        else
        {
            BackBuffer.ScrollToEnd();
        }

        // Since the height of the main terminal changed scroll it to the bottom.
        FollowLiveOutput();
    }

    /// <summary>
    /// Collapses the back buffer so it reclaims the space for the main terminal, closes its search
    /// panel, scrolls the main terminal to the bottom, resets the input history position and clears
    /// the input box.
    /// </summary>
    private void CloseBackBuffer()
    {
        BackBuffer.Visibility = Visibility.Collapsed;
        _backBufferSearch.Close();
        FollowLiveOutput();

        InputHistoryPosition = -1;
        Input.Text = "";
        ClearPrivateInput();
        FocusInput();
    }

    private void CloseBackBuffer_OnClick(object sender, RoutedEventArgs e) => CloseBackBuffer();

    /// <summary>
    /// Scrolling up in the main terminal opens the back buffer, after that the wheel pages the back
    /// buffer until its end is reached and it closes.
    /// </summary>
    private void Output_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Allow scrolling down in the main terminal when the back buffer is collapsed.
        if (e.Delta < 0 && !IsBackBufferVisible)
        {
            return;
        }

        if (!IsBackBufferVisible)
        {
            ShowBackBuffer(toTop: false);
            e.Handled = true;
        }
        else if (e.Delta > 0)
        {
            BackBuffer.PageUp();
            e.Handled = true;
        }
        else
        {
            BackBufferPageDown();
        }
    }

    /// <summary>
    /// The back buffer scrolls normally, scrolling down once its last line is visible closes it.
    /// </summary>
    private void BackBuffer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsBackBufferVisible && e.Delta < 0 && IsLastLineVisible(BackBuffer))
        {
            BackBuffer.Visibility = Visibility.Collapsed;
            _backBufferSearch.Close();
            FocusInput();
        }
    }

    private static bool IsLastLineVisible(TextEditor terminal) =>
        terminal.TextArea.TextView.GetVisualLine(terminal.LineCount) is not null;

    private void TerminalContextMenuCopy_OnClick(object sender, RoutedEventArgs e)
    {
        if (GetContextMenuTerminal(sender) is { } terminal && terminal.SelectionLength > 0)
        {
            // SetDataObject is used instead of SetText because SetText crashes more often than not.
            Clipboard.SetDataObject(terminal.SelectedText, true);
        }
    }

    private void TerminalContextMenuClear_OnClick(object sender, RoutedEventArgs e)
    {
        // The game terminal and the back buffer share a document so this clears both.
        if (GetContextMenuTerminal(sender) is not null)
        {
            _transcript.Apply(new TerminalOutput(TerminalOutputKind.Clear, string.Empty, []));
        }
    }

    private void TerminalContextMenuRegex_OnClick(object sender, RoutedEventArgs e)
    {
        var text = GetContextMenuTerminal(sender)?.SelectedText ?? "";
        ViewModels.AppCommands.ShowRegexTester(text.ReplaceLineEndings(" ").Trim());
    }

    private static TextEditor? GetContextMenuTerminal(object sender)
    {
        if ((sender as MenuItem)?.Parent is not ContextMenu menu)
        {
            return null;
        }

        return menu.PlacementTarget as TextEditor ?? (menu.Parent as Popup)?.PlacementTarget as TextEditor;
    }

    private void FollowLiveOutput()
    {
        if (_scroll is not { Status: DispatcherOperationStatus.Pending })
        {
            _scroll = Dispatcher.InvokeAsync(Output.ScrollToEnd, DispatcherPriority.Loaded);
        }
    }

    private void RedrawTranscript()
    {
        Output.TextArea.TextView.Redraw();
        BackBuffer.TextArea.TextView.Redraw();
    }
}
