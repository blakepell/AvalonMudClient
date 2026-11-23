using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Scripting.API;

namespace Avalon.Models;

/// <summary>
/// Per-game character, movement and affect state exposed to scripts as <c>vm</c>.
/// </summary>
/// <remarks>Scripts populate the game-specific statistics from triggers or GMCP messages.</remarks>
public sealed partial class ProfileViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the active character name.
    /// </summary>
    [ObservableProperty]
    public partial string? Character { get; set; }

    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int Move { get; set; }
    public int MaxMove { get; set; }
    public int Str { get; set; }
    public int MaxStr { get; set; }
    public int Int { get; set; }
    public int MaxInt { get; set; }
    public int Wis { get; set; }
    public int MaxWis { get; set; }
    public int Dex { get; set; }
    public int MaxDex { get; set; }
    public int Con { get; set; }
    public int MaxCon { get; set; }
    public int Tha { get; set; }
    public int MaxTha { get; set; }
    public int WiziLevel { get; set; }
    public int IncogLevel { get; set; }
    public int ScriptsActive { get; set; }
    public bool IsAfk { get; set; }
    public bool IsQuiet { get; set; }
    public bool IsFighting { get; set; }
    public bool IsRiding { get; set; }
    public bool IsRecording { get; set; }
    public SolidColorBrush HpBrush { get; set; } = Brushes.White;
    public SolidColorBrush ManaBrush { get; set; } = Brushes.White;
    public SolidColorBrush MoveBrush { get; set; } = Brushes.White;
    public SolidColorBrush TickBrush { get; set; } = Brushes.White;
    public int Level { get; set; } = 1;
    public int Tick { get; set; } = 40;
    public string? Stance { get; set; } = "Unknown";
    public string RoomName { get; set; } = "Limbo";
    public string Exits { get; set; } = "None";
    public string StartRecordingRoom { get; set; } = "";

    /// <summary>
    /// Gets the game's movement state.
    /// </summary>
    public Movement Movement { get; set; } = new();

    /// <summary>
    /// Gets the active effects for the game.
    /// </summary>
    public AffectCollection Affects { get; set; } = new();
}

/// <summary>
/// Tracks which exits are currently available.
/// </summary>
public sealed class Movement
{
    public bool North { get; set; } = true;
    public bool Northeast { get; set; } = true;
    public bool East { get; set; } = true;
    public bool Southeast { get; set; } = true;
    public bool South { get; set; } = true;
    public bool Southwest { get; set; } = true;
    public bool West { get; set; } = true;
    public bool Northwest { get; set; } = true;
    public bool Up { get; set; } = true;
    public bool Down { get; set; } = true;

    /// <summary>
    /// Updates the exits from an array of abbreviations or direction names.
    /// </summary>
    public void SetMovement(JsArray? directions)
    {
        if (directions is { length: > 0 })
        {
            Parse(directions);
        }
    }

    /// <summary>
    /// Updates the exits from a JavaScript array.
    /// </summary>
    public void Parse(JsArray? directions)
    {
        if (directions is not null)
        {
            var exits = new List<string>();
            foreach (var value in directions.values())
            {
                if (value is not null)
                {
                    exits.Add(value.ToString() ?? "");
                }
            }

            Parse(exits);
        }
    }

    /// <summary>
    /// Updates the exits from a sequence of direction names.
    /// </summary>
    public void Parse(IEnumerable<string> directions)
    {
        var exits = directions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        North = Has("n", "North");
        Northeast = Has("ne", "Northeast");
        East = Has("e", "East");
        Southeast = Has("se", "Southeast");
        South = Has("s", "South");
        Southwest = Has("sw", "Southwest");
        West = Has("w", "West");
        Northwest = Has("nw", "Northwest");
        Up = Has("u", "Up");
        Down = Has("d", "Down");

        bool Has(string shortName, string fullName) => exits.Contains(shortName) || exits.Contains(fullName);
    }
}

/// <summary>
/// An observable collection of effects that scripts can update by name.
/// </summary>
public sealed class AffectCollection : ObservableCollection<Affect>
{
    /// <summary>
    /// Adds an effect or refreshes the duration of the existing effect with the same name.
    /// </summary>
    public void AddOrUpdateAffect(Affect affect)
    {
        var existing = this.FirstOrDefault(item => string.Equals(item.Name, affect.Name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            Add(affect);
        }
        else
        {
            existing.MaxDuration = Math.Max(existing.MaxDuration, affect.Duration);
            existing.Duration = affect.Duration;
        }

        SortAffects();
    }

    /// <summary>
    /// Removes all effects with the given name.
    /// </summary>
    public void RemoveAffect(string name)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            if (string.Equals(this[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Sorts effects by descending duration.
    /// </summary>
    public void SortAffects()
    {
        var sorted = this.OrderByDescending(affect => affect.Duration).ToArray();
        for (int i = 0; i < sorted.Length; i++)
        {
            int current = IndexOf(sorted[i]);
            if (current != i)
            {
                Move(current, i);
            }
        }
    }
}
