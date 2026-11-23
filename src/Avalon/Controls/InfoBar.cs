using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Avalon.Models;

namespace Avalon.Controls;

/// <summary>
/// A severity colored message bar with a title, message, optional action link and close button.
/// </summary>
/// <remarks>
/// Recreates the ActiPro InfoBar Continuum uses in the ribbon footer and over terminals.
/// </remarks>
public sealed class InfoBar : ContentControl
{
    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoBar),
        new PropertyMetadata("", (d, _) => ((InfoBar)d).Update()));

    /// <summary>Identifies the <see cref="Message"/> dependency property.</summary>
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(InfoBar),
        new PropertyMetadata("", (d, _) => ((InfoBar)d).Update()));

    /// <summary>Identifies the <see cref="Severity"/> dependency property.</summary>
    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(nameof(Severity), typeof(InfoBarSeverity), typeof(InfoBar),
        new PropertyMetadata(InfoBarSeverity.Information, (d, _) => ((InfoBar)d).Update()));

    /// <summary>Identifies the <see cref="IsOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(InfoBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((InfoBar)d).Update()));

    /// <summary>Identifies the <see cref="ActionText"/> dependency property.</summary>
    public static readonly DependencyProperty ActionTextProperty = DependencyProperty.Register(nameof(ActionText), typeof(string), typeof(InfoBar),
        new PropertyMetadata(null, (d, _) => ((InfoBar)d).Update()));

    private readonly Border _border = new() { CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 6, 6) };
    private readonly TextBlock _icon = new() { FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 16, Margin = new Thickness(0, 1, 10, 0), VerticalAlignment = VerticalAlignment.Top };
    private readonly TextBlock _text = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button _close = new()
    {
        Content = new TextBlock { FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 10, Text = "" },
        Width = 24,
        Height = 24,
        Padding = new Thickness(0),
        Background = Brushes.Transparent,
        BorderThickness = new Thickness(0),
        VerticalAlignment = VerticalAlignment.Top,
        ToolTip = "Close"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InfoBar"/> class.
    /// </summary>
    public InfoBar()
    {
        _close.Click += (_, _) =>
        {
            SetCurrentValue(IsOpenProperty, false);
            Closed?.Invoke(this, EventArgs.Empty);
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(_text, 1);
        Grid.SetColumn(_close, 2);
        grid.Children.Add(_icon);
        grid.Children.Add(_text);
        grid.Children.Add(_close);
        _border.Child = grid;
        Content = _border;
        Update();
    }

    /// <summary>
    /// Occurs when the close button is clicked.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Occurs when the action link is clicked.
    /// </summary>
    public event EventHandler? ActionClicked;

    /// <summary>Gets or sets the bold title.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the message.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets the severity, which picks the colors and icon.</summary>
    public InfoBarSeverity Severity
    {
        get => (InfoBarSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Gets or sets whether the bar is shown.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the text of the action link, or null for none.</summary>
    public string? ActionText
    {
        get => (string?)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    private void Update()
    {
        Visibility = IsOpen && (!string.IsNullOrEmpty(Message) || !string.IsNullOrEmpty(Title)) ? Visibility.Visible : Visibility.Collapsed;

        var (background, accent, glyph) = Severity switch
        {
            InfoBarSeverity.Success => (Color.FromRgb(0x1E, 0x3A, 0x24), Color.FromRgb(0x6C, 0xCB, 0x5F), ""),
            InfoBarSeverity.Warning => (Color.FromRgb(0x43, 0x35, 0x19), Color.FromRgb(0xFC, 0xE1, 0x00), ""),
            InfoBarSeverity.Error => (Color.FromRgb(0x44, 0x27, 0x26), Color.FromRgb(0xFF, 0x99, 0xA4), ""),
            _ => (Color.FromRgb(0x27, 0x2B, 0x33), Color.FromRgb(0x60, 0xCD, 0xFF), "")
        };
        _border.Background = new SolidColorBrush(background);
        _border.BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, accent.R, accent.G, accent.B));
        _border.BorderThickness = new Thickness(1);
        _icon.Text = glyph;
        _icon.Foreground = new SolidColorBrush(accent);
        _text.Foreground = Brushes.White;
        _close.Foreground = Brushes.White;

        _text.Inlines.Clear();
        if (!string.IsNullOrEmpty(Title))
        {
            _text.Inlines.Add(new Run(Title + "  ") { FontWeight = FontWeights.SemiBold });
        }

        _text.Inlines.Add(new Run(Message));
        if (!string.IsNullOrEmpty(ActionText))
        {
            var link = new Hyperlink(new Run(ActionText)) { Foreground = new SolidColorBrush(accent) };
            link.Click += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);
            _text.Inlines.Add(new Run("  "));
            _text.Inlines.Add(link);
        }
    }
}
