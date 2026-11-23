using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Avalon.Models;
using Avalon.Terminal;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;

namespace Avalon.Controls;

/// <summary>
/// A named, read only terminal shown as a tool window that scripts and hash commands can write to.
/// </summary>
/// <remarks>
/// A recreation of Continuum's TerminalDialog.  Text sent here is pulled out of the main game window,
/// such as tells, channels or the output of a script, so it doesn't scroll away.
/// </remarks>
public sealed class TerminalWindow : UserControl
{
    private readonly TextEditor _editor;
    private readonly TerminalTranscript _transcript;
    private readonly SearchPanel _search;
    private readonly AppSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalWindow"/> class.
    /// </summary>
    /// <param name="name">The name scripts use to address the terminal.</param>
    /// <param name="settings">The settings that supply the font and palette.</param>
    public TerminalWindow(string name, AppSettings settings)
    {
        TerminalName = name;
        _settings = settings;
        _editor = new TextEditor
        {
            IsReadOnly = true,
            Padding = new Thickness(10, 0, 20, 0),
            BorderThickness = new Thickness(0),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            ContextMenu = CreateContextMenu()
        };
        _editor.TextArea.TextView.Options.EnableEmailHyperlinks = false;
        _editor.TextArea.TextView.Options.ShowBoxForControlCharacters = false;
        Bind(FontFamilyProperty, nameof(AppSettings.TerminalFont), new FontFamilyConverter());
        Bind(FontSizeProperty, nameof(AppSettings.TerminalFontSize));
        Bind(TextEditor.WordWrapProperty, nameof(AppSettings.WordWrapTerminals));
        Bind(BackgroundProperty, "ColorTheme.Background");
        Bind(ForegroundProperty, "ColorTheme.Default");

        _transcript = new TerminalTranscript(_editor) { Palette = settings.ColorTheme };
        _search = SearchPanel.Install(_editor);
        PropertyChangedEventManager.AddHandler(settings, Settings_OnPropertyChanged, nameof(AppSettings.ColorTheme));
        Content = _editor;

        IsKeyboardFocusWithinChanged += (_, _) =>
        {
            if (IsKeyboardFocusWithin)
            {
                NotificationCount = 0;
            }
        };
    }

    /// <summary>
    /// Gets the name scripts use to address the terminal.
    /// </summary>
    public string TerminalName { get; }

    /// <summary>
    /// Runs <paramref name="action"/> against the named terminal on the UI thread, creating the terminal when
    /// <paramref name="create"/> is true and it doesn't exist yet.
    /// </summary>
    /// <returns>False when there's no such terminal and it wasn't created.</returns>
    public static bool With(string name, Action<TerminalWindow> action, bool create = true)
    {
        name = NormalizeName(name);
        if (name.Length == 0)
        {
            return false;
        }

        var dispatcher = Application.Current.Dispatcher;
        if (!dispatcher.CheckAccess())
        {
            return dispatcher.Invoke(() => With(name, action, create));
        }

        if (Application.Current.MainWindow is not MainWindow main)
        {
            return false;
        }

        var terminal = main.FindTerminalWindow(name);
        if (terminal is null)
        {
            if (!create)
            {
                return false;
            }

            terminal = main.ShowTerminalWindow(name, activate: false);
        }

        action(terminal);
        return true;
    }

    /// <summary>
    /// Turns a typed name into a terminal name: trimmed, without spaces.
    /// </summary>
    public static string NormalizeName(string? name) => (name ?? "").Trim().Replace(" ", "");

    /// <summary>
    /// Gets the number of lines written while the terminal wasn't focused.
    /// </summary>
    public int NotificationCount
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotificationCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Occurs when <see cref="NotificationCount"/> changes, so the host can update its title.
    /// </summary>
    public event EventHandler? NotificationCountChanged;

    /// <summary>
    /// Appends text, which may contain ANSI codes, and scrolls to the end when the view is already there.
    /// </summary>
    public void AppendAnsi(string raw)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.InvokeAsync(() => AppendAnsi(raw));
            return;
        }

        var scroll = _editor.TextArea.TextView;
        bool atEnd = scroll.VerticalOffset + scroll.ActualHeight >= scroll.DocumentHeight - 2;
        var (text, runs, _) = AnsiText.Parse(raw.Replace("\r\n", "\n"), default);
        _transcript.Apply(new TerminalOutput(TerminalOutputKind.Server, text, runs));
        if (atEnd)
        {
            _editor.ScrollToEnd();
        }

        if (!IsKeyboardFocusWithin)
        {
            NotificationCount++;
        }
    }

    /// <summary>
    /// Appends a line, optionally prefixed with a timestamp.
    /// </summary>
    public void AppendLine(string raw, bool includeTimestamp = false)
        => AppendAnsi(includeTimestamp ? $"[{DateTime.Now:HH:mm:ss}]: {raw}\n" : raw + "\n");

    /// <summary>
    /// Clears the terminal.
    /// </summary>
    public void Clear()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.InvokeAsync(Clear);
            return;
        }

        _transcript.Apply(new TerminalOutput(TerminalOutputKind.Clear, "", []));
        NotificationCount = 0;
    }

    /// <summary>
    /// Replaces the terminal's text.
    /// </summary>
    public void SetText(string raw)
    {
        Clear();
        AppendAnsi(raw);
    }

    /// <summary>
    /// Gets the terminal's plain text.
    /// </summary>
    public string Text => _editor.Text;

    internal string GetText(bool includeAnsi, int lines) => _transcript.GetText(includeAnsi, lines);

    internal void ReplaceText(string searchText, string replacementText) =>
        _transcript.ReplaceText(searchText, replacementText, lastOnly: false);

    /// <summary>
    /// Scrolls to the last line.
    /// </summary>
    public void ScrollToEnd()
    {
        if (Dispatcher.CheckAccess())
        {
            _editor.ScrollToEnd();
        }
        else
        {
            Dispatcher.InvokeAsync(_editor.ScrollToEnd);
        }
    }

    private void Bind(DependencyProperty property, string path, IValueConverter? converter = null)
        => _editor.SetBinding(property, new Binding(path) { Source = _settings, Converter = converter });

    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu();
        var copy = new MenuItem { Header = "Copy", Icon = Glyph("") };
        copy.Click += (_, _) =>
        {
            if (_editor.SelectionLength > 0)
            {
                Clipboard.SetDataObject(_editor.SelectedText, true);
            }
        };
        var find = new MenuItem { Header = "Find", Icon = Glyph(""), InputGestureText = "Ctrl+F" };
        find.Click += (_, _) => _search.Open();
        var clear = new MenuItem { Header = "Clear Terminal", Icon = Glyph("") };
        clear.Click += (_, _) => Clear();
        menu.Items.Add(copy);
        menu.Items.Add(find);
        menu.Items.Add(new Separator());
        menu.Items.Add(clear);
        return menu;
    }

    private static TextBlock Glyph(string glyph) => new() { FontFamily = new FontFamily("Segoe MDL2 Assets"), Text = glyph };

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _transcript.Palette = _settings.ColorTheme;
        _editor.TextArea.TextView.Redraw();
    }

    private sealed class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => new FontFamily(value as string ?? "Consolas");

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => Binding.DoNothing;
    }
}
