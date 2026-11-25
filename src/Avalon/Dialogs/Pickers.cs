using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Avalon.Models;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Dialogs;

/// <summary>
/// Asks which game a command should work on when no game terminal is open.
/// </summary>
public sealed class GamePicker : DialogBase
{
    private readonly ListBox _list;

    private GamePicker(IEnumerable<Game> games)
    {
        Title = "Select a Game";
        Width = 380;
        Height = 360;
        _list = new ListBox
        {
            ItemsSource = games.OrderBy(g => g.Name).ToList(),
            DisplayMemberPath = nameof(Game.Name),
            Margin = new Thickness(12, 12, 12, 0)
        };
        _list.MouseDoubleClick += (_, _) =>
        {
            if (_list.SelectedItem is not null)
            {
                DialogResult = true;
            }
        };
        SetBody(_list);
        Loaded += (_, _) =>
        {
            _list.SelectedIndex = 0;
            _list.Focus();
        };
    }

    /// <summary>
    /// Shows the picker and returns the chosen game, or null when cancelled.
    /// </summary>
    public static Game? Pick(IEnumerable<Game> games)
    {
        var picker = new GamePicker(games);
        return picker.ShowDialog() == true ? picker._list.SelectedItem as Game : null;
    }

    /// <inheritdoc/>
    protected override bool OnOk() => _list.SelectedItem is not null;
}

/// <summary>
/// Lists the saved directions, those that start in the current room first, and walks the chosen one.
/// </summary>
/// <remarks>
/// Recreates Continuum's DirectionsSelectWindow, including two hop paths that pass through another direction's
/// ending room.
/// </remarks>
public sealed class DirectionsSelectWindow : DialogBase
{
    private readonly ListView _list;
    private readonly SearchBox _search;
    private readonly List<Direction> _all;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionsSelectWindow"/> class.
    /// </summary>
    public DirectionsSelectWindow(Game game, string room)
    {
        Title = string.IsNullOrWhiteSpace(room) ? "Directions" : $"Directions from {room}";
        Width = 640;
        Height = 460;

        _all = string.IsNullOrWhiteSpace(room)
            ? game.Directions.OrderBy(d => d.Name).ToList()
            : game.Directions.Where(d => string.Equals(d.StartingRoom, room, StringComparison.OrdinalIgnoreCase))
                .Concat(game.Directions.Where(d => game.Directions.Any(first =>
                    string.Equals(first.StartingRoom, room, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(first.EndingRoom)
                    && string.Equals(first.EndingRoom, d.StartingRoom, StringComparison.OrdinalIgnoreCase))))
                .Concat(game.Directions.Where(d => string.IsNullOrWhiteSpace(d.StartingRoom)))
                .Distinct().ToList();

        var grid = new GridView();
        grid.Columns.Add(new GridViewColumn { Header = "Name", Width = 180, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(Direction.Name)) });
        grid.Columns.Add(new GridViewColumn { Header = "Speedwalk", Width = 260, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(Direction.Speedwalk)) });
        grid.Columns.Add(new GridViewColumn { Header = "Starts In", Width = 140, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(Direction.StartingRoom)) });
        _list = new ListView { View = grid, ItemsSource = _all };
        _list.MouseDoubleClick += (_, _) =>
        {
            if (_list.SelectedItem is not null)
            {
                DialogResult = true;
            }
        };
        _list.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && _list.SelectedItem is not null)
            {
                DialogResult = true;
            }
        };

        _search = new SearchBox { Watermark = "Find a destination", Height = 28, Margin = new Thickness(0, 0, 0, 6) };
        _search.TextChanged += (_, _) =>
        {
            var query = _search.Text.Trim();
            _list.ItemsSource = _all.Where(d => d.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            _list.SelectedIndex = 0;
        };
        _search.SearchExecuted += (_, _) =>
        {
            if (Selected is not null)
            {
                DialogResult = true;
            }
        };

        var panel = new DockPanel { Margin = new Thickness(12, 12, 12, 0) };
        DockPanel.SetDock(_search, Dock.Top);
        panel.Children.Add(_search);
        panel.Children.Add(_list);
        SetBody(panel, "Go");
        Loaded += (_, _) =>
        {
            _list.SelectedIndex = 0;
            _search.Focus();
        };
    }

    /// <summary>
    /// Gets the chosen direction.
    /// </summary>
    public Direction? Selected => _list.SelectedItem as Direction;

    /// <inheritdoc/>
    protected override bool OnOk() => Selected is not null;
}
