using System.IO;
using System.Windows;
using System.Windows.Controls;
using Avalon.Common;
using Avalon.Models;
using Microsoft.Win32;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Collects and validates a new or existing game before closing its Mosaic modal dialog.
/// </summary>
public partial class NewGameForm : UserControl
{
    private readonly Game? _original;

    /// <summary>
    /// Gets the completed game after the dialog is accepted.
    /// </summary>
    public Game? CreatedGame { get; private set; }

    /// <summary>
    /// Initializes a new instance of the NewGameForm class.
    /// </summary>
    public NewGameForm() : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the NewGameForm class with a detached edit of a game.
    /// </summary>
    /// <param name="game">The game to edit, or null to create a game.</param>
    public NewGameForm(Game? game)
    {
        InitializeComponent();
        _original = game;
        AutoConnect.IsChecked = game?.AutoConnect ?? true;
        if (game is not null)
        {
            GameName.Text = game.Name ?? "";
            Host.Text = game.IpAddress ?? "";
            Port.Text = game.Port.ToString();
            Description.Text = game.Description ?? "";
            Genre.Text = game.Genre ?? "";
            PlayStyle.Text = game.PlayStyle ?? "";
            AccentColor.SelectedColor = game.AccentBrush.Color;
            ImagePath.Text = game.ImagePath ?? "";
            SaveButton.Content = "Save";
        }
        Loaded += (_, _) => GameName.Focus();
    }

    private void Browse_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog
        {
            Title = "Choose game artwork — recommended 1200 × 600",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            CheckFileExists = true
        };
        if (picker.ShowDialog(Window.GetWindow(this)) == true)
        {
            ImagePath.Text = picker.FileName;
        }
    }

    private void RemoveArtwork_OnClick(object sender, RoutedEventArgs e) => ImagePath.Clear();

    private void Cancel_OnClick(object sender, RoutedEventArgs e) => ModalDialog.FindHost(this)?.Close(false);

    private void Add_OnClick(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";
        if (string.IsNullOrWhiteSpace(GameName.Text))
        {
            ShowError("Enter a game name.", GameName);
            return;
        }

        var host = Host.Text.Trim();
        if (Uri.CheckHostName(host.Trim('[', ']')) == UriHostNameType.Unknown)
        {
            ShowError("Enter a hostname or IP address without a protocol or port.", Host);
            return;
        }

        if (!int.TryParse(Port.Text, out var port) || port is < 1 or > 65535)
        {
            ShowError("Enter a port between 1 and 65535.", Port);
            return;
        }

        var color = AccentColor.SelectedColor.ToString();

        var game = new Game
        {
            Id = _original?.Id ?? Guid.NewGuid(), Plays = _original?.Plays ?? 0,
            LastPlayed = _original?.LastPlayed ?? DateTime.MinValue,
            Name = GameName.Text.Trim(), IpAddress = host, Port = port,
            AutoConnect = AutoConnect.IsChecked == true,
            Description = Description.Text.Trim(), Genre = Genre.Text.Trim(),
            PlayStyle = PlayStyle.Text.Trim(), AccentColor = color
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(ImagePath.Text))
            {
                game.ImagePath = ImagePath.Text == _original?.ImagePath
                    ? _original.ImagePath
                    : GameArtwork.Import(ImagePath.Text, Guid.NewGuid());
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            ShowError($"Unable to import artwork: {ex.Message}", ImagePath);
            return;
        }

        CreatedGame = game;
        ModalDialog.FindHost(this)?.Close(true);
    }

    private void ShowError(string message, UIElement field)
    {
        ErrorText.Text = message;
        field.Focus();
    }
}

