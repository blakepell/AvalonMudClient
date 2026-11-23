using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Avalon.Models;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Presents and filters the games in the AppSettings supplied as its data context.
/// </summary>
public partial class GamesPanel : UserControl
{
    /// <summary>
    /// Initializes a new instance of the GamesPanel class.
    /// </summary>
    public GamesPanel() => InitializeComponent();

    private void GamesView_OnFilter(object sender, FilterEventArgs e)
    {
        var query = GameSearch?.Text.Trim() ?? "";
        e.Accepted = e.Item is Game game && (query.Length == 0 ||
            new[] { game.Name, game.Address, game.Description, game.Genre, game.PlayStyle }
                .Any(value => value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true));
    }

    private void GameSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        => ((CollectionViewSource)Resources["GamesView"]).View?.Refresh();

    /// <summary>
    /// Opens the New Game dialog, as the New Game button does.
    /// </summary>
    public Task NewGameAsync() => ShowGameDialogAsync();

    private async void NewGame_OnClick(object sender, RoutedEventArgs e)
        => await ShowGameDialogAsync();

    private async void GameCard_OnEditRequested(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is GameCard { DataContext: Game game })
        {
            await ShowGameDialogAsync(game);
        }
    }

    private void GameCard_OnDeleteRequested(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (DataContext is AppSettings settings && sender is GameCard { DataContext: Game game })
        {
            settings.Games.Remove(game);
            App.SaveAppSettings();
        }
    }

    private async Task ShowGameDialogAsync(Game? original = null)
    {
        if (DataContext is not AppSettings settings || Window.GetWindow(this)?.Content is not UIElement host)
        {
            return;
        }

        var form = new NewGameForm(original);
        var window = Window.GetWindow(this);
        var dialog = new ModalDialog
        {
            Title = original is null ? "New Game" : "Edit Game",
            Description = original is null ? "Add a server to your game library." : "Update this game in your library.",
            Width = 560,
            MaxWidth = Math.Max(240, window.ActualWidth - 60),
            Height = 720,
            MaxHeight = Math.Max(240, window.ActualHeight - 60),
            Content = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = form
            }
        };
        if (await dialog.ShowAsync(host) == true && form.CreatedGame is { } game)
        {
            if (original is null)
            {
                settings.Games.Add(game);
                GameSearch.Clear();
            }
            else
            {
                using (((CollectionViewSource)Resources["GamesView"]).View.DeferRefresh())
                {
                    original.Name = game.Name;
                    original.IpAddress = game.IpAddress;
                    original.Port = game.Port;
                    original.AutoConnect = game.AutoConnect;
                    original.Description = game.Description;
                    original.Genre = game.Genre;
                    original.PlayStyle = game.PlayStyle;
                    original.AccentColor = game.AccentColor;
                    original.ImagePath = game.ImagePath;
                }
            }
            App.SaveAppSettings();
        }
    }
}
