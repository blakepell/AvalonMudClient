using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Avalon.Models;

public partial class Game
{
    /// <summary>
    /// Gets runtime state for this game. This state is recreated when the profile is loaded.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public ProfileViewModel ViewModel { get; } = new();

    /// <summary>
    /// Gets or sets the game address using its Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public string? HostName { get => IpAddress; set => IpAddress = value; }

    /// <summary>
    /// Gets or sets whether GMCP is enabled using its Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public bool GmcpEnable { get => GmcpEnabled; set => GmcpEnabled = value; }

    /// <summary>
    /// Gets or sets the profile's aliases using the Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public ObservableCollection<Alias> AliasList { get => Aliases; set => Aliases = value; }

    /// <summary>
    /// Gets or sets the profile's triggers using the Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public ObservableCollection<Trigger> TriggerList { get => Triggers; set => Triggers = value; }

    /// <summary>
    /// Gets or sets the profile's directions using the Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public ObservableCollection<Direction> DirectionList { get => Directions; set => Directions = value; }

    /// <summary>
    /// Gets or sets the profile's macros using the Continuum property name.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public ObservableCollection<Macro> MacroList { get => Macros; set => Macros = value; }
}
