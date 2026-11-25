using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Avalon.Models;
using Avalon.ViewModels;
using Mosaic.UI.Wpf.Themes;

namespace Avalon.Controls;

/// <summary>
/// Base for the tool windows that follow the active terminal's game.
/// </summary>
public abstract class ActiveGamePanel : UserControl
{
    private AppViewModel? _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveGamePanel"/> class.
    /// </summary>
    protected ActiveGamePanel()
    {
        DataContextChanged += (_, _) => Attach();
    }

    /// <summary>
    /// Gets the game whose terminal is showing.
    /// </summary>
    protected Game? Game => _viewModel?.ActiveTerminal?.Game;

    /// <summary>
    /// Gets the terminal that is showing.
    /// </summary>
    protected MudTerminal? Terminal => _viewModel?.ActiveTerminal;

    /// <summary>
    /// Rebuilds the panel for the current game.
    /// </summary>
    protected abstract void OnGameChanged(Game? game);

    private void Attach()
    {
        if (_viewModel is not null)
        {
            PropertyChangedEventManager.RemoveHandler(_viewModel, OnViewModelChanged, nameof(AppViewModel.ActiveTerminal));
        }

        _viewModel = DataContext as AppViewModel;
        if (_viewModel is not null)
        {
            PropertyChangedEventManager.AddHandler(_viewModel, OnViewModelChanged, nameof(AppViewModel.ActiveTerminal));
        }

        OnGameChanged(Game);
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e) => OnGameChanged(Game);
}

/// <summary>
/// The Variables window (Continuum's VariableRepeater): visible variables as label / value rows in their colors.
/// </summary>
public sealed class VariableRepeater : ActiveGamePanel
{
    private readonly ItemsControl _items = new();
    private CollectionViewSource? _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableRepeater"/> class.
    /// </summary>
    public VariableRepeater()
    {
        _items.ItemTemplate = CreateTemplate();
        Content = new ScrollViewer { Content = _items, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    /// <inheritdoc/>
    protected override void OnGameChanged(Game? game)
    {
        if (game is null)
        {
            _items.ItemsSource = null;
            return;
        }

        _source = new CollectionViewSource { Source = game.Variables, IsLiveFilteringRequested = true, IsLiveSortingRequested = true };
        _source.LiveFilteringProperties.Add(nameof(Variable.IsVisible));
        _source.LiveSortingProperties.Add(nameof(Variable.DisplayOrder));
        _source.SortDescriptions.Add(new SortDescription(nameof(Variable.DisplayOrder), ListSortDirection.Ascending));
        _source.Filter += (_, e) => e.Accepted = e.Item is Variable { IsVisible: true };
        _items.ItemsSource = _source.View;
    }

    private static DataTemplate CreateTemplate()
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.PaddingProperty, new Thickness(6, 3, 6, 3));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1));
        border.SetResourceReference(Border.BorderBrushProperty, MosaicTheme.ControlBorderBrush);
        border.SetBinding(Border.BackgroundProperty, new Binding(nameof(Variable.BackgroundColor)) { TargetNullValue = Brushes.Transparent });

        var grid = new FrameworkElementFactory(typeof(DockPanel));
        border.AppendChild(grid);

        var triangle = new FrameworkElementFactory(typeof(Path));
        triangle.SetValue(Path.DataProperty, Geometry.Parse("M0,0 L6,4 L0,8 Z"));
        triangle.SetValue(WidthProperty, 6.0);
        triangle.SetValue(HeightProperty, 8.0);
        triangle.SetValue(MarginProperty, new Thickness(0, 0, 6, 0));
        triangle.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        triangle.SetResourceReference(Shape.FillProperty, MosaicTheme.AccentBrush);
        grid.AppendChild(triangle);

        var label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        label.SetValue(MinWidthProperty, 90.0);
        label.SetValue(MarginProperty, new Thickness(0, 0, 10, 0));
        label.SetBinding(TextBlock.TextProperty, new Binding(nameof(Variable.DisplayLabel)));
        grid.AppendChild(label);

        var value = new FrameworkElementFactory(typeof(TextBlock));
        value.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        value.SetBinding(TextBlock.TextProperty, new Binding(nameof(Variable.Value)));
        value.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(Variable.ForegroundColor))
        {
            Converter = new FallbackBrushConverter(),
            ConverterParameter = "foreground"
        });
        grid.AppendChild(value);

        return new DataTemplate { VisualTree = border };
    }

    /// <summary>
    /// Uses the theme's foreground when a variable has no color of its own.
    /// </summary>
    private sealed class FallbackBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value as Brush ?? (Application.Current.TryFindResource(MosaicTheme.WindowForegroundBrush) as Brush) ?? Brushes.White;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}

/// <summary>
/// The Alias Button Actions window: a button for every alias marked Button Visible.
/// </summary>
public sealed class AliasButtonsRepeater : ActiveGamePanel
{
    private readonly WrapPanel _panel = new() { Margin = new Thickness(4) };
    private Game? _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasButtonsRepeater"/> class.
    /// </summary>
    public AliasButtonsRepeater()
    {
        Content = new ScrollViewer { Content = _panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    /// <inheritdoc/>
    protected override void OnGameChanged(Game? game)
    {
        if (_game is not null)
        {
            _game.Aliases.CollectionChanged -= Aliases_OnCollectionChanged;
            foreach (var alias in _game.Aliases)
            {
                alias.PropertyChanged -= Alias_OnPropertyChanged;
            }
        }

        _game = game;
        if (_game is not null)
        {
            _game.Aliases.CollectionChanged += Aliases_OnCollectionChanged;
            foreach (var alias in _game.Aliases)
            {
                alias.PropertyChanged += Alias_OnPropertyChanged;
            }
        }
        Build();
    }

    private void Aliases_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (Alias alias in e.OldItems ?? Array.Empty<Alias>())
        {
            alias.PropertyChanged -= Alias_OnPropertyChanged;
        }

        foreach (Alias alias in e.NewItems ?? Array.Empty<Alias>())
        {
            alias.PropertyChanged += Alias_OnPropertyChanged;
        }

        Build();
    }

    private void Alias_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Alias.ButtonVisible) or nameof(Alias.ButtonLabel) or nameof(Alias.SortOrder) or nameof(Alias.AliasExpression))
        {
            Build();
        }
    }

    private void Build()
    {
        _panel.Children.Clear();
        if (_game is null)
        {
            return;
        }

        foreach (var alias in _game.Aliases.Where(a => a.ButtonVisible).OrderBy(a => a.SortOrder).ThenBy(a => a.AliasExpression))
        {
            var button = new Button
            {
                Content = string.IsNullOrWhiteSpace(alias.ButtonLabel) ? alias.AliasExpression : alias.ButtonLabel,
                Width = _game.AliasButtonWidth > 0 ? _game.AliasButtonWidth : double.NaN,
                Height = 30,
                Margin = new Thickness(3),
                ToolTip = alias.Command
            };
            button.Click += async (_, _) =>
            {
                if (Terminal is { } terminal)
                {
                    await terminal.SendCommandAsync(alias.AliasExpression);
                }
            };
            _panel.Children.Add(button);
        }

        if (_panel.Children.Count == 0)
        {
            _panel.Children.Add(new TextBlock { Text = "Set Button Visible on an alias to add a button here.", Margin = new Thickness(6), TextWrapping = TextWrapping.Wrap, Opacity = 0.7 });
        }
    }
}

/// <summary>
/// A ten-direction compass with a shortcut to the Directions dialog.
/// </summary>
public sealed class MovementView : ActiveGamePanel
{
    private static readonly (string Label, string Command, int Row, int Column)[] Buttons =
    [
        ("NW", "nw", 0, 0), ("N", "n", 0, 1), ("NE", "ne", 0, 2), ("U", "u", 0, 4),
        ("W", "w", 1, 0), ("E", "e", 1, 2),
        ("SW", "sw", 2, 0), ("S", "s", 2, 1), ("SE", "se", 2, 2), ("D", "d", 2, 4)
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="MovementView"/> class.
    /// </summary>
    public MovementView()
    {
        var grid = new UniformGrid { Rows = 3, Columns = 5, Margin = new Thickness(8), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        var cells = new UIElement[15];
        foreach (var (label, command, row, column) in Buttons)
        {
            var button = CreateButton(label, command);
            button.Click += async (_, _) =>
            {
                if (Terminal is { } terminal)
                {
                    await terminal.SendCommandAsync(command);
                }
            };
            cells[row * 5 + column] = button;
        }

        var map = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Assets/map-48.png")),
            Width = 22,
            Height = 22
        };
        var directions = CreateButton(map, "Directions");
        directions.Command = AppCommands.ShowDirectionsCommand;
        cells[6] = directions;

        foreach (var cell in cells)
        {
            grid.Children.Add(cell ?? new Border());
        }

        Content = grid;
    }

    private static Button CreateButton(object content, string label)
    {
        var button = new Button { Content = content, Width = 40, Height = 40, Padding = new Thickness(0), Margin = new Thickness(2), ToolTip = label };
        System.Windows.Automation.AutomationProperties.SetName(button, label);
        return button;
    }

    /// <inheritdoc/>
    protected override void OnGameChanged(Game? game)
    {
        IsEnabled = game is not null;
    }
}
