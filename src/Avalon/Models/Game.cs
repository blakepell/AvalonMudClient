using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using Avalon.Common;
using System.Windows.Media;
using Avalon.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Cache;

namespace Avalon.Models;

/// <summary>
/// Describes a saved game server, its card presentation, and its automation.
/// </summary>
[Display(Name = "Profiles")]
public partial class Game : ObservableObject, IEditableModel
{
    private const string DefaultTabColor = "#004C81";
    private static readonly SolidColorBrush DefaultTabColorBrush = ColorPaletteCache.GetBrush(ColorPaletteCache.GetColor(DefaultTabColor));

    /// <summary>
    /// Gets or sets the stable identifier for this game.
    /// </summary>
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial Guid? Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [property: Display(Name = "Name")]
    [property: Category("Game")]
    [property: DataListColumn(Name = "Name", Width = 160)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Name")]
    public partial string? Name { get; set; }

    /// <summary>
    /// Gets or sets the hostname or IP address.
    /// </summary>
    [property: Display(Name = "Host Name")]
    [property: Category("Game")]
    [property: DataListColumn(Name = "Host Name", Width = 160)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("IpAddress")]
    [NotifyPropertyChangedFor(nameof(Address))]
    public partial string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the server port.
    /// </summary>
    [property: Display(Name = "Port")]
    [property: Category("Game")]
    [property: DataListColumn(Name = "Port", Width = 60)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Port")]
    [NotifyPropertyChangedFor(nameof(Address))]
    public partial int Port { get; set; } = 4000;

    /// <summary>
    /// Gets or sets a value that indicates whether opening or restoring the game tab starts a connection.
    /// </summary>
    [property: Display(Name = "Auto Connect")]
    [property: DisplayName("Auto Connect")]
    [property: Category("Game")]
    [property: Description("Connect automatically when the game tab opens or is restored.")]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("AutoConnect")]
    public partial bool AutoConnect { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether to accept GMCP from the server.
    /// </summary>
    [property: Display(Name = "GMCP Enabled")]
    [property: Category("Game")]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("GmcpEnabled")]
    public partial bool GmcpEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the commands to run as soon as a connection opens.
    /// </summary>
    [property: Display(Name = "On Connect Command")]
    [property: Category("Events")]
    [property: Description("Commands sent as soon as the connection opens.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Text)]
    [ObservableProperty, JsonPropertyName("OnConnectCommand")]
    public partial string? OnConnectCommand { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether aliases can run.
    /// </summary>
    [property: Display(Name = "Aliases Enabled")]
    [property: Category("Options")]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("AliasesEnabled")]
    public partial bool AliasesEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether output triggers can run.
    /// </summary>
    [property: Display(Name = "Triggers Enabled")]
    [property: Category("Options")]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("TriggersEnabled")]
    public partial bool TriggersEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the saved aliases.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Aliases")]
    public partial ObservableCollection<Alias> Aliases { get; set; } = new();

    /// <summary>
    /// Gets or sets a value that indicates whether Tab opens the command completion list.
    /// </summary>
    [property: Category("Auto Complete")]
    [ObservableProperty, JsonPropertyName("AutoCompleteEnabled")]
    public partial bool AutoCompleteEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets additional entries offered alongside aliases and command history.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("AutoCompleteList")]
    public partial List<string> AutoCompleteList { get; set; } = new();

    /// <summary>
    /// Gets or sets the preferred width of the command completion list.
    /// </summary>
    [property: Category("Auto Complete")]
    [ObservableProperty, JsonPropertyName("AutoCompleteWidth")]
    public partial double AutoCompleteWidth { get; set; } = 300;


    /// <summary>
    /// Gets or sets the saved output triggers.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Triggers")]
    public partial ObservableCollection<Trigger> Triggers { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved keyboard macros.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Macros")]
    public partial ObservableCollection<Macro> Macros { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved script sources.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Scripts")]
    public partial ObservableCollection<SourceCode> Scripts { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved game variables.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Variables")]
    public partial ObservableCollection<Variable> Variables { get; set; } = new();

    /// <summary>
    /// Gets or sets the game description.
    /// </summary>
    [property: Display(Name = "Description")]
    [property: Category("Game")]
    [property: DataListColumn(Name = "Description", Width = 220, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Text)]
    [ObservableProperty, JsonPropertyName("Description")]
    public partial string? Description { get; set; }

    /// <summary>
    /// Gets or sets the number of times this game has been played.
    /// </summary>
    [property: DataListColumn(Name = "Plays", Width = 50)]
    [property: Category("Statistics")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonPropertyName("Plays")]
    public partial int Plays { get; set; }

    /// <summary>
    /// Gets or sets the UTC time this game was last played, or DateTime.MinValue if never played.
    /// </summary>
    [property: DataListColumn(Name = "Last Played", Width = 130)]
    [property: Category("Statistics")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonPropertyName("LastPlayed")]
    public partial DateTime LastPlayed { get; set; }

    /// <summary>
    /// Records a play by updating the last played time and play count.
    /// </summary>
    public void RecordPlay()
    {
        LastPlayed = DateTime.UtcNow;
        Plays++;
    }

    /// <summary>
    /// Gets or sets the genre displayed on the card.
    /// </summary>
    [property: Category("Card")]
    [ObservableProperty, JsonPropertyName("Genre")]
    public partial string? Genre { get; set; }

    /// <summary>
    /// Gets or sets the play style, such as roleplay or hack and slash.
    /// </summary>
    [property: Category("Card")]
    [ObservableProperty, JsonPropertyName("PlayStyle")]
    public partial string? PlayStyle { get; set; }

    /// <summary>
    /// Gets or sets the artwork path relative to the application data folder.
    /// </summary>
    /// <remarks>
    /// Store a file reference rather than image bytes or a WPF ImageSource. Use 1200 by 600 artwork.
    /// </remarks>
    [property: Category("Card")]
    [ObservableProperty, JsonPropertyName("ImagePath")]
    public partial string? ImagePath { get; set; }

    /// <summary>
    /// Gets or sets the serializable accent color in hexadecimal form.
    /// </summary>
    [property: Category("Card")]
    [ObservableProperty, JsonPropertyName("AccentColor")]
    [NotifyPropertyChangedFor(nameof(AccentBrush))]
    public partial string AccentColor { get; set; } = "#4F46E5";

    /// <summary>
    /// Gets or sets the card brush, persisted through <see cref="AccentColor"/>.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public SolidColorBrush AccentBrush
    {
        get => ToBrush(AccentColor, Brushes.SlateBlue);
        set => AccentColor = value.Color.ToString();
    }

    private static SolidColorBrush ToBrush(string? color, SolidColorBrush fallback)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return fallback;
        }

        try
        {
            return ColorPaletteCache.GetBrush((Color)ColorConverter.ConvertFromString(color));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or NotSupportedException)
        {
            return fallback;
        }
    }

    /// <summary>
    /// Gets or sets the saved highlight triggers.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("HighlightTriggers")]
    public partial ObservableCollection<HighlightTrigger> HighlightTriggers { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved speedwalk directions.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Directions")]
    public partial ObservableCollection<Direction> Directions { get; set; } = new();

    /// <summary>
    /// Gets or sets the packages defined or installed for this game.
    /// </summary>
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Packages")]
    public partial ObservableCollection<PackageMetadata> Packages { get; set; } = new();

    /// <summary>
    /// Gets or sets a value that indicates whether line transformer triggers can run.
    /// </summary>
    [property: Display(Name = "Replacement Triggers Enabled")]
    [property: Category("Options")]
    [property: Description("If triggers that rewrite the line (line transformers) run.")]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("ReplacementTriggersEnabled")]
    public partial bool ReplacementTriggersEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the saved color of the game's document tab.
    /// </summary>
    [property: Display(Name = "Tab Color")]
    [property: DisplayName("Tab Color")]
    [property: Category("UI")]
    [property: Description("The color of this game's terminal tab, such as #004C81.")]
    [ObservableProperty, JsonPropertyName("TabColor")]
    [NotifyPropertyChangedFor(nameof(TabColor))]
    public partial string? TabColorHex { get; set; } = DefaultTabColor;

    /// <summary>
    /// Gets or sets the brush used for the game's document tab.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public SolidColorBrush TabColor
    {
        get => ToBrush(TabColorHex, DefaultTabColorBrush);
        set => TabColorHex = value.Color.ToString();
    }

    /// <summary>
    /// Gets or sets the width of the buttons in the Alias Button Actions window.
    /// </summary>
    [property: Display(Name = "Alias Button Width")]
    [property: Category("UI")]
    [ObservableProperty, JsonPropertyName("AliasButtonWidth")]
    public partial double AliasButtonWidth { get; set; } = 100;

    /// <summary>
    /// Gets or sets JavaScript that runs when the game's terminal opens.
    /// </summary>
    [property: Display(Name = "On Profile Loaded Event")]
    [property: DisplayName("On Profile Loaded Event")]
    [property: Category("Events")]
    [property: Description("JavaScript that runs when the game's terminal is opened.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("OnProfileLoadedEvent")]
    public partial string OnProfileLoadedEvent { get; set; } = "";

    /// <summary>
    /// Gets or sets JavaScript that runs when the game's terminal closes.
    /// </summary>
    [property: Display(Name = "On Profile Closed Event")]
    [property: DisplayName("On Profile Closed Event")]
    [property: Category("Events")]
    [property: Description("JavaScript that runs when the game's terminal is closed.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("OnProfileClosedEvent")]
    public partial string OnProfileClosedEvent { get; set; } = "";

    /// <summary>
    /// Gets or sets JavaScript that runs every tick.
    /// </summary>
    [property: Display(Name = "On Tick Event")]
    [property: DisplayName("On Tick Event")]
    [property: Category("Events")]
    [property: Description("JavaScript that runs every tick while connected.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("OnTickEvent")]
    public partial string OnTickEvent { get; set; } = "";

    /// <summary>
    /// Gets or sets the length of a tick in seconds, or 0 to disable the tick timer.
    /// </summary>
    [property: Display(Name = "Tick Duration (Seconds)")]
    [property: Category("Events")]
    [property: Description("How often the On Tick Event runs while connected.  0 turns the tick timer off.")]
    [ObservableProperty, JsonPropertyName("TickDurationInSeconds")]
    public partial int TickDurationInSeconds { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} {IpAddress} {Description}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(IpAddress) && Port is > 0 and < 65536;

    /// <inheritdoc/>
    public IEditableModel CloneNew()
    {
        var clone = ModelCloner.Clone(this);
        clone.Id = Guid.NewGuid();
        clone.Name = $"{Name} (Copy)";
        return clone;
    }
    /// <summary>
    /// Gets the address formatted for display, including IPv6 brackets when necessary.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public string Address => IpAddress?.Contains(':') == true && !IpAddress.StartsWith('[')
        ? $"[{IpAddress}]:{Port}" : $"{IpAddress}:{Port}";
}
