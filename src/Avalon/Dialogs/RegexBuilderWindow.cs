using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using Mosaic.UI.Wpf.Themes;

namespace Avalon.Dialogs;

/// <summary>
/// The Regular Expression Builder, ported from Continuum: a pattern tested live against five sample lines.
/// </summary>
/// <remarks>
/// Every edit re-runs the pattern (with a two second timeout) and highlights the match in each sample in green.
/// The Pattern Options menu replaces the selected part of the pattern with common building blocks, and Save hands the
/// pattern back to the trigger being edited.
/// </remarks>
public sealed class RegexBuilderWindow : Window
{
    private static readonly SolidColorBrush MatchBrush = Frozen(Color.FromRgb(0x2E, 0x6A, 0x2D));
    private static readonly SolidColorBrush ErrorBrush = Frozen(Color.FromRgb(0x8B, 0x1E, 0x1E));
    private static IHighlightingDefinition? _regexHighlighting;

    private readonly TextEditor _pattern;
    private readonly List<(TextEditor Editor, MatchColorizer Colorizer)> _tests = [];
    private readonly Border _statusBar;
    private readonly TextBlock _status;
    private readonly Button _save;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexBuilderWindow"/> class.
    /// </summary>
    /// <param name="pattern">The pattern to start with.</param>
    /// <param name="sample">Optional text for the first test line, such as a line selected in the terminal.</param>
    public RegexBuilderWindow(string pattern = "", string sample = "")
    {
        Style = (Style)Application.Current.FindResource("DialogWindowStyle");
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Title = "Regular Expression Builder";
        Width = 820;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        ShowInTaskbar = true;
        if (Application.Current.MainWindow is { IsLoaded: true } owner)
        {
            Owner = owner;
        }

        _pattern = CreateEditor();
        _pattern.SyntaxHighlighting = LoadHighlighting();
        _pattern.Text = pattern;

        var body = new StackPanel { Margin = new Thickness(12, 8, 12, 0) };
        body.Children.Add(Row("Regex Pattern:", _pattern));
        for (int i = 1; i <= 5; i++)
        {
            var editor = CreateEditor();
            var colorizer = new MatchColorizer();
            editor.TextArea.TextView.LineTransformers.Add(colorizer);
            _tests.Add((editor, colorizer));
            body.Children.Add(Row($"Test {i}:", editor));
        }
        _tests[0].Editor.Text = sample;

        var copy = new Button { Content = "Copy Pattern", Style = (Style)FindResource("DialogButtonStyle"), Margin = new Thickness(0) };
        copy.Click += (_, _) => Clipboard.SetDataObject(Pattern, true);
        var close = new Button { Content = "Close", IsCancel = true, Style = (Style)FindResource("DialogButtonStyle") };
        close.Click += (_, _) => Close();
        _save = new Button { Content = "Save", Style = (Style)FindResource("DialogButtonStyle"), Visibility = Visibility.Collapsed };
        _save.Click += (_, _) => DialogResult = true;

        var footer = new DockPanel { Margin = new Thickness(12, 10, 12, 10), LastChildFill = false };
        DockPanel.SetDock(copy, Dock.Left);
        DockPanel.SetDock(close, Dock.Right);
        DockPanel.SetDock(_save, Dock.Right);
        footer.Children.Add(copy);
        footer.Children.Add(close);
        footer.Children.Add(_save);

        _status = new TextBlock { Margin = new Thickness(10, 4, 10, 4), Foreground = Brushes.White, TextTrimming = TextTrimming.CharacterEllipsis };
        _statusBar = new Border { Child = _status, MinHeight = 24 };
        _statusBar.SetResourceReference(Border.BackgroundProperty, MosaicTheme.AccentBrush);

        var root = new DockPanel();
        var menu = BuildMenu();
        DockPanel.SetDock(menu, Dock.Top);
        DockPanel.SetDock(_statusBar, Dock.Bottom);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(menu);
        root.Children.Add(_statusBar);
        root.Children.Add(footer);
        root.Children.Add(body);
        Content = root;

        _pattern.TextChanged += (_, _) => RunTests();
        foreach (var (editor, _) in _tests)
        {
            editor.TextChanged += (_, _) => RunTests();
        }

        Loaded += (_, _) =>
        {
            _pattern.TextArea.Focus();
            _pattern.CaretOffset = _pattern.Text.Length;
            RunTests();
        };
    }

    /// <summary>
    /// Gets or sets the pattern.
    /// </summary>
    public string Pattern
    {
        get => _pattern.Text.TrimEnd();
        set => _pattern.Text = value;
    }

    /// <summary>
    /// Gets or sets whether the Save button (which returns the pattern to the caller) is shown.
    /// </summary>
    public bool IsSaveVisible
    {
        get => _save.Visibility == Visibility.Visible;
        set
        {
            _save.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            _save.IsDefault = value;
        }
    }

    /// <summary>
    /// Runs the pattern against each test line.
    /// </summary>
    public void RunTests()
    {
        try
        {
            string pattern = Pattern;
            int matched = 0;
            string? groups = null;
            foreach (var (editor, colorizer) in _tests)
            {
                colorizer.Matches.Clear();
                string input = editor.Text.TrimEnd();
                if (input.Length > 0 && pattern.Length > 0)
                {
                    var match = Regex.Match(input, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));
                    if (match.Success)
                    {
                        matched++;
                        colorizer.Matches.Add((match.Index, match.Length));
                        groups ??= match.Groups.Count > 1
                            ? string.Join("  ", match.Groups.Cast<Group>().Skip(1).Select((g, i) => $"%{(int.TryParse(g.Name, out _) ? (i + 1).ToString() : g.Name)}={g.Value}"))
                            : null;
                    }
                }
                editor.TextArea.TextView.Redraw();
            }

            _statusBar.SetResourceReference(Border.BackgroundProperty, MosaicTheme.AccentBrush);
            _status.Text = pattern.Length == 0 ? "Enter a pattern." : $"{matched} of {_tests.Count(t => t.Editor.Text.Trim().Length > 0)} test(s) matched.  {groups}";
        }
        catch (Exception ex) when (ex is ArgumentException or RegexMatchTimeoutException)
        {
            foreach (var (editor, colorizer) in _tests)
            {
                colorizer.Matches.Clear();
                editor.TextArea.TextView.Redraw();
            }
            _statusBar.Background = ErrorBrush;
            _status.Text = ex.Message;
        }
    }

    private Menu BuildMenu()
    {
        var options = new MenuItem { Header = "_Pattern Options" };
        options.Items.Add(Item("_Escape Pattern", () => Pattern = Regex.Escape(Pattern)));
        options.Items.Add(Item("_Unescape Pattern", () =>
        {
            try
            {
                Pattern = Regex.Unescape(Pattern);
            }
            catch (ArgumentException ex)
            {
                _status.Text = ex.Message;
            }
        }));
        options.Items.Add(new Separator());
        options.Items.Add(Item("Match Selection to _Number", () => ReplaceSelection(@"\d+")));
        options.Items.Add(Item("Match Selection to Number Group", () => ReplaceSelection(@"(\d+)")));
        options.Items.Add(Item("Match Selection to Number Group (Named)", () => ReplaceNamed(@"\d+")));
        options.Items.Add(new Separator());
        options.Items.Add(Item("Match Selection to _Word", () => ReplaceSelection(@"\w+")));
        options.Items.Add(Item("Match Selection to Word Group", () => ReplaceSelection(@"(\w+)")));
        options.Items.Add(Item("Match Selection to Word Group (Named)", () => ReplaceNamed(@"\w+")));
        options.Items.Add(new Separator());
        options.Items.Add(Item("Match Selection to Wil_dcard", () => ReplaceSelection(".*")));
        options.Items.Add(Item("Match Selection to Wildcard Group", () => ReplaceSelection("(.*)")));
        options.Items.Add(Item("Match Selection to Wildcard Group (Named)", () => ReplaceNamed(".*")));

        var menu = new Menu { Padding = new Thickness(6, 2, 6, 2) };
        menu.Items.Add(options);
        return menu;

        static MenuItem Item(string header, Action action)
        {
            var item = new MenuItem { Header = header };
            item.Click += (_, _) => action();
            return item;
        }
    }

    private void ReplaceSelection(string replacement)
    {
        if (_pattern.SelectionLength == 0)
        {
            _status.Text = "Select the part of the pattern to replace first.";
            return;
        }
        int start = _pattern.SelectionStart;
        _pattern.Document.Replace(start, _pattern.SelectionLength, replacement);
        _pattern.Select(start, replacement.Length);
        _pattern.TextArea.Focus();
    }

    private void ReplaceNamed(string expression)
    {
        if (_pattern.SelectionLength == 0)
        {
            _status.Text = "Select the part of the pattern to replace first.";
            return;
        }
        if (InputBox.Show("Named Group", "Enter the name of the group:") is { Length: > 0 } name)
        {
            ReplaceSelection($"(?<{Regex.Replace(name, @"\W", "")}>{expression})");
        }
    }

    private static DockPanel Row(string label, TextEditor editor)
    {
        var text = new TextBlock { Text = label, Width = 110, VerticalAlignment = VerticalAlignment.Center };
        var row = new DockPanel { Margin = new Thickness(0, 4, 0, 4) };
        DockPanel.SetDock(text, Dock.Left);
        row.Children.Add(text);
        row.Children.Add(new Border
        {
            Child = editor,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4, 3, 4, 3)
        }.WithResourceBorder());
        return row;
    }

    private static TextEditor CreateEditor()
    {
        var editor = new TextEditor
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            WordWrap = false,
            Height = 22
        };
        editor.SetResourceReference(BackgroundProperty, MosaicTheme.ControlTextBackgroundBrush);
        editor.SetResourceReference(ForegroundProperty, MosaicTheme.WindowForegroundBrush);
        editor.TextArea.TextView.Options.EnableHyperlinks = false;
        editor.TextArea.TextView.Options.EnableEmailHyperlinks = false;
        // Single line: Enter is left for the dialog's default button.
        editor.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = false;
            }
        };
        editor.TextArea.PreviewTextInput += (_, e) =>
        {
            if (e.Text is "\r" or "\n")
            {
                e.Handled = true;
            }
        };
        editor.Document.Changed += (_, _) =>
        {
            if (editor.Text.Contains('\n') || editor.Text.Contains('\r'))
            {
                editor.Text = editor.Text.ReplaceLineEndings(" ");
            }
        };
        return editor;
    }

    private static IHighlightingDefinition? LoadHighlighting()
    {
        if (_regexHighlighting is not null)
        {
            return _regexHighlighting;
        }

        using var stream = typeof(RegexBuilderWindow).Assembly.GetManifestResourceStream("Avalon.Assets.Regex.xshd");
        if (stream is null)
        {
            return null;
        }

        using var reader = XmlReader.Create(stream);
        return _regexHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    private static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Paints the matched part of a test line.
    /// </summary>
    private sealed class MatchColorizer : DocumentColorizingTransformer
    {
        public List<(int Start, int Length)> Matches { get; } = [];

        protected override void ColorizeLine(DocumentLine line)
        {
            foreach (var (start, length) in Matches)
            {
                int from = Math.Max(start, line.Offset);
                int to = Math.Min(start + length, line.EndOffset);
                if (from < to)
                {
                    ChangeLinePart(from, to, element =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(MatchBrush);
                        element.TextRunProperties.SetForegroundBrush(Brushes.White);
                    });
                }
            }
        }
    }
}

/// <summary>
/// Small helpers for building themed UI in code.
/// </summary>
internal static class ThemeHelpers
{
    /// <summary>
    /// Gives a border the theme's control border brush.
    /// </summary>
    public static Border WithResourceBorder(this Border border)
    {
        border.SetResourceReference(Border.BorderBrushProperty, MosaicTheme.ControlBorderBrush);
        border.SetResourceReference(Border.BackgroundProperty, MosaicTheme.ControlTextBackgroundBrush);
        return border;
    }
}
