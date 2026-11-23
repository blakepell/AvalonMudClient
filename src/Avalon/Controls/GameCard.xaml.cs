using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Avalon.Common;
using Avalon.Models;

namespace Avalon.Controls;

/// <summary>
/// Displays a game supplied through its data context, with optional artwork and a copyable address.
/// </summary>
public partial class GameCard : UserControl
{
    /// <summary>
    /// Identifies the bubbling request to connect to the card's game.
    /// </summary>
    public static readonly RoutedEvent ConnectRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(ConnectRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GameCard));

    /// <summary>
    /// Identifies the bubbling request to edit the card's game.
    /// </summary>
    public static readonly RoutedEvent EditRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(EditRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GameCard));

    /// <summary>
    /// Identifies the bubbling request to delete the card's game.
    /// </summary>
    public static readonly RoutedEvent DeleteRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(DeleteRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GameCard));

    /// <summary>
    /// Occurs when the Connect button is clicked.
    /// </summary>
    public event RoutedEventHandler ConnectRequested
    {
        add => AddHandler(ConnectRequestedEvent, value);
        remove => RemoveHandler(ConnectRequestedEvent, value);
    }

    /// <summary>
    /// Occurs when the primary Options button or Edit menu item is clicked.
    /// </summary>
    public event RoutedEventHandler EditRequested
    {
        add => AddHandler(EditRequestedEvent, value);
        remove => RemoveHandler(EditRequestedEvent, value);
    }

    /// <summary>
    /// Occurs when the Delete menu item is clicked.
    /// </summary>
    public event RoutedEventHandler DeleteRequested
    {
        add => AddHandler(DeleteRequestedEvent, value);
        remove => RemoveHandler(DeleteRequestedEvent, value);
    }

    private void Edit_OnClick(object sender, RoutedEventArgs e)
        => RaiseEvent(new RoutedEventArgs(EditRequestedEvent, this));

    private void Connect_OnClick(object sender, RoutedEventArgs e)
        => RaiseEvent(new RoutedEventArgs(ConnectRequestedEvent, this));

    private void Delete_OnClick(object sender, RoutedEventArgs e)
        => RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent, this));

    private Game? _game;

    /// <summary>
    /// Initializes a new instance of the GameCard class.
    /// </summary>
    public GameCard()
    {
        InitializeComponent();
        Loaded += (_, _) => ObserveGame();
        Unloaded += (_, _) => StopObservingGame();
        DataContextChanged += (_, _) => { if (IsLoaded)
            {
                ObserveGame();
            }
        };
    }

    private void ObserveGame()
    {
        StopObservingGame();
        _game = DataContext as Game;
        if (_game is not null)
        {
            _game.PropertyChanged += Game_OnPropertyChanged;
        }

        UpdateArtwork();
    }

    private void StopObservingGame()
    {
        if (_game is not null)
        {
            _game.PropertyChanged -= Game_OnPropertyChanged;
        }

        _game = null;
    }

    private void Game_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Game.ImagePath))
        {
            UpdateArtwork();
        }
    }

    private void UpdateArtwork() => Artwork.Source = GameArtwork.Load(_game?.ImagePath);

    private void CopyAddress_OnClick(object sender, RoutedEventArgs e)
    {
        if (_game is null)
        {
            return;
        }

        try
        {
            Clipboard.SetText(_game.Address);
        }
        catch (COMException)
        {
            Mosaic.UI.Wpf.Controls.MessageBox.Show("The clipboard is busy. Please try again.", "Copy address");
        }
    }
}
