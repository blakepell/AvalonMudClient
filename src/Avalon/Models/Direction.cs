using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// A named speedwalk path between two rooms.
/// </summary>
[Display(Name = "Directions")]
public partial class Direction : ObservableObject, IEditableModel
{
    private static readonly Regex StepPattern = new(@"\(([^)]*)\)|(\d*)(ne|nw|se|sw|[nsewud])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Gets or sets a stable identifier.
    /// </summary>
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the friendly name used by #go.
    /// </summary>
    [property: Display(Name = "Name")]
    [property: Description("The friendly name of the direction.")]
    [property: DataListColumn(Name = "Name", Width = 140)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Name")]
    public partial string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the speedwalk, such as 2n 3w u (open door).
    /// </summary>
    [property: Display(Name = "Speedwalk Directions")]
    [property: Description("The speedwalk directions.  These include 2n 3w 4u and commands like (open door).")]
    [property: DataListColumn(Name = "Speedwalk", Width = 220, Trim = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Speedwalk")]
    public partial string Speedwalk { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the room the path starts in.
    /// </summary>
    [property: Display(Name = "Starting Room Name")]
    [property: Description("The name of the starting room.  The client uses this to string different sets of directions together.")]
    [property: DataListColumn(Name = "Starting Room Name", Width = 140, Trim = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("StartingRoom")]
    public partial string StartingRoom { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the room the path ends in.
    /// </summary>
    [property: Display(Name = "Ending Room Name")]
    [property: Description("The name of the ending room.")]
    [property: DataListColumn(Name = "Ending Room Name", Width = 140, Trim = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("EndingRoom")]
    public partial string EndingRoom { get; set; } = "";

    /// <summary>
    /// Gets or sets the package the direction belongs to.
    /// </summary>
    [property: Display(Name = "Package")]
    [property: DataListColumn(Name = "Package", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Package")]
    public partial string Package { get; set; } = "";

    /// <summary>
    /// Expands the speedwalk into individual commands, for example "2n (open door) e" becomes n, n, open door, e.
    /// </summary>
    public static IEnumerable<string> Expand(string speedwalk)
    {
        foreach (Match match in StepPattern.Matches(speedwalk ?? ""))
        {
            if (match.Groups[1].Success)
            {
                yield return match.Groups[1].Value;
                continue;
            }

            int count = int.TryParse(match.Groups[2].Value, out int n) && n > 0 ? n : 1;
            for (int i = 0; i < count; i++)
                yield return match.Groups[3].Value.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Returns the path walked backwards, so "2n e" becomes "w 2s".  Commands in parentheses are kept as is.
    /// </summary>
    public static string Reverse(string speedwalk)
    {
        var steps = Expand(speedwalk).Reverse().Select(ReverseStep).ToList();
        return Compress(steps);
    }

    /// <summary>
    /// Compresses individual steps into a speedwalk, for example n, n, e becomes "2n e".
    /// </summary>
    public static string Compress(IEnumerable<string> steps)
    {
        var sb = new StringBuilder();
        string? last = null;
        int count = 0;

        void Flush()
        {
            if (last is null)
            {
                return;
            }

            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            if (IsDirection(last))
            {
                sb.Append(count > 1 ? $"{count}{last}" : last);
            }
            else
            {
                sb.Append(string.Join(' ', Enumerable.Repeat($"({last})", count)));
            }
        }

        foreach (var step in steps)
        {
            if (step == last)
            {
                count++;
                continue;
            }
            Flush();
            last = step;
            count = 1;
        }
        Flush();
        return sb.ToString();
    }

    /// <summary>
    /// Returns whether the step is one of the ten compass directions.
    /// </summary>
    public static bool IsDirection(string step) => step is "n" or "s" or "e" or "w" or "u" or "d" or "ne" or "nw" or "se" or "sw";

    private static string ReverseStep(string step) => step switch
    {
        "n" => "s", "s" => "n", "e" => "w", "w" => "e", "u" => "d", "d" => "u",
        "ne" => "sw", "sw" => "ne", "nw" => "se", "se" => "nw",
        _ => step
    };

    /// <inheritdoc/>
    public override string ToString() => $"{Name} {Speedwalk} {StartingRoom} {EndingRoom} {Package}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Speedwalk);

    /// <inheritdoc/>
    public IEditableModel CloneNew()
    {
        var clone = ModelCloner.Clone(this);
        clone.Id = Guid.NewGuid().ToString();
        return clone;
    }
}
