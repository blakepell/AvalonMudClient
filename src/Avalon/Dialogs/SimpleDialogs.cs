using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Mosaic.UI.Wpf.Controls;
using Mosaic.UI.Wpf.Themes;

namespace Avalon.Dialogs;

/// <summary>
/// Shared layout for the small dialogs: content over an OK / Cancel footer.
/// </summary>
public abstract class DialogBase : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogBase"/> class.
    /// </summary>
    protected DialogBase()
    {
        Style = (Style)Application.Current.FindResource("DialogWindowStyle");
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        if (Application.Current.MainWindow is { IsLoaded: true } owner && !ReferenceEquals(owner, this))
        {
            Owner = owner;
        }
    }

    /// <summary>
    /// Builds the window content from <paramref name="body"/> and an OK / Cancel footer.
    /// </summary>
    protected void SetBody(UIElement body, string okText = "OK", UIElement? footerLeft = null)
    {
        var ok = new Button { Content = okText, IsDefault = !AcceptsReturnInBody, Style = (Style)FindResource("DialogButtonStyle") };
        ok.Click += (_, _) =>
        {
            if (OnOk())
            {
                DialogResult = true;
            }
        };
        var cancel = new Button { Content = "Cancel", IsCancel = true, Style = (Style)FindResource("DialogButtonStyle") };

        var footer = new DockPanel { Margin = new Thickness(12), LastChildFill = false };
        if (footerLeft is not null)
        {
            DockPanel.SetDock(footerLeft, Dock.Left);
            footer.Children.Add(footerLeft);
        }
        DockPanel.SetDock(cancel, Dock.Right);
        DockPanel.SetDock(ok, Dock.Right);
        footer.Children.Add(cancel);
        footer.Children.Add(ok);

        var root = new DockPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        root.Children.Add(body);
        Content = root;
    }

    /// <summary>
    /// Gets whether Enter belongs to the body (a multi-line editor) rather than the OK button.
    /// </summary>
    protected virtual bool AcceptsReturnInBody => false;

    /// <summary>
    /// Called when OK is pressed; return false to keep the dialog open.
    /// </summary>
    protected virtual bool OnOk() => true;
}

/// <summary>
/// A multi-line plain text editor, used for command text the way Continuum's StringEditor is.
/// </summary>
public sealed class StringEditor : DialogBase
{
    private readonly TextEditor _editor;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringEditor"/> class.
    /// </summary>
    public StringEditor(string text)
    {
        Title = "Edit Text";
        Width = 700;
        Height = 450;
        _editor = new TextEditor
        {
            Text = text,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            ShowLineNumbers = true,
            WordWrap = true,
            Padding = new Thickness(4),
            Margin = new Thickness(12, 12, 12, 0),
            BorderThickness = new Thickness(1)
        };
        _editor.SetResourceReference(BackgroundProperty, MosaicTheme.ControlTextBackgroundBrush);
        _editor.SetResourceReference(ForegroundProperty, MosaicTheme.WindowForegroundBrush);
        _editor.SetResourceReference(BorderBrushProperty, MosaicTheme.ControlBorderBrush);
        _editor.TextArea.TextView.LinkTextForegroundBrush = Brushes.SteelBlue;
        SetBody(_editor);
        Loaded += (_, _) => _editor.TextArea.Focus();
    }

    /// <summary>
    /// Gets the edited text.
    /// </summary>
    public string Text => _editor.Text;

    /// <inheritdoc/>
    protected override bool AcceptsReturnInBody => true;
}

/// <summary>
/// Prompts for a single line of text.
/// </summary>
public sealed class InputBox : DialogBase
{
    private readonly TextBox _input;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputBox"/> class.
    /// </summary>
    public InputBox(string title, string prompt, string value = "")
    {
        Title = title;
        Width = 420;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _input = new TextBox { Text = value, Height = 28, VerticalContentAlignment = VerticalAlignment.Center };
        var panel = new StackPanel { Margin = new Thickness(12, 12, 12, 0) };
        panel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(_input);
        SetBody(panel);
        Loaded += (_, _) =>
        {
            _input.Focus();
            _input.SelectAll();
        };
    }

    /// <summary>
    /// Gets the entered text.
    /// </summary>
    public string Value => _input.Text;

    /// <summary>
    /// Shows an input box and returns the entered text, or null when cancelled.
    /// </summary>
    public static string? Show(string title, string prompt, string value = "")
    {
        var box = new InputBox(title, prompt, value);
        return box.ShowDialog() == true ? box.Value : null;
    }
}

/// <summary>
/// A modal JavaScript editor with completion against the active game's scripting environment.
/// </summary>
public sealed class ScriptEditorWindow : DialogBase
{
    private readonly ScriptEditorControl _editor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptEditorWindow"/> class.
    /// </summary>
    /// <param name="code">The initial JavaScript source.</param>
    /// <param name="saveProperty">The callback that updates the property when saving the script.</param>
    public ScriptEditorWindow(string code, Action<string> saveProperty)
    {
        Title = "Script Editor";
        Width = 900;
        Height = 600;
        _editor = new ScriptEditorControl
        {
            Text = code,
            Environment = Scripting.ScriptEnvironmentFactory.ForActiveGame(),
            Margin = new Thickness(0),
            SaveTextAsync = (text, _) =>
            {
                saveProperty(text);
                return Task.CompletedTask;
            }
        };
        SetBody(_editor, "OK");
    }

    /// <summary>
    /// Gets the edited code.
    /// </summary>
    public string Text => _editor.Text;

    /// <inheritdoc/>
    protected override bool AcceptsReturnInBody => true;
}

/// <summary>
/// Captures a key combination such as Ctrl+Alt+NumPad8 together with the command it runs.
/// </summary>
public sealed class MacroEditor : DialogBase
{
    private readonly TextBox _keys;
    private readonly TextBox _command;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroEditor"/> class.
    /// </summary>
    public MacroEditor(Models.Macro macro)
    {
        Macro = macro;
        Title = "Macro Editor";
        Width = 460;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        _keys = new TextBox { Text = macro.KeyCombination, Height = 28, IsReadOnlyCaretVisible = false, VerticalContentAlignment = VerticalAlignment.Center };
        _keys.PreviewKeyDown += Keys_OnPreviewKeyDown;
        _command = new TextBox { Text = macro.Command, Height = 28, VerticalContentAlignment = VerticalAlignment.Center };

        var panel = new StackPanel { Margin = new Thickness(12, 12, 12, 0) };
        panel.Children.Add(new TextBlock { Text = "Press the key combination:", Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(_keys);
        panel.Children.Add(new TextBlock { Text = "Command:", Margin = new Thickness(0, 10, 0, 4) });
        panel.Children.Add(_command);
        SetBody(panel);
        Loaded += (_, _) => _keys.Focus();
    }

    /// <summary>
    /// Gets the macro being edited.
    /// </summary>
    public Models.Macro Macro { get; }

    /// <inheritdoc/>
    protected override bool OnOk()
    {
        if (string.IsNullOrWhiteSpace(_keys.Text))
        {
            return false;
        }

        Macro.KeyCombination = _keys.Text;
        Macro.Command = _command.Text;
        return true;
    }

    private void Keys_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        e.Handled = true;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return;
        }

        if (key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None)
        {
            _command.Focus();
            return;
        }

        var modifiers = Keyboard.Modifiers;
        _keys.Text = string.Concat(
            modifiers.HasFlag(ModifierKeys.Control) ? "Ctrl+" : "",
            modifiers.HasFlag(ModifierKeys.Alt) ? "Alt+" : "",
            modifiers.HasFlag(ModifierKeys.Shift) ? "Shift+" : "",
            modifiers.HasFlag(ModifierKeys.Windows) ? "Win+" : "",
            key.ToString());
    }
}
