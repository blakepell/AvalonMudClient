using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Cache;

namespace Avalon.Models;

/// <summary>
/// Represents a game effect that scripts track for a character.
/// </summary>
public sealed partial class Affect : ObservableObject
{
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial string Modifies { get; set; } = "none";
    [ObservableProperty] public partial int Modifier { get; set; }
    [ObservableProperty] public partial int Duration { get; set; } = -1;
    [ObservableProperty] public partial int MaxDuration { get; set; } = 15;
    [ObservableProperty] public partial bool Permanent { get; set; }
    [ObservableProperty] public partial bool UnknownDuration { get; set; }
    [ObservableProperty] public partial bool IsSong { get; set; }
    [ObservableProperty] public partial SolidColorBrush Background { get; set; } = ColorPaletteCache.GetBrush("#246840");
    [ObservableProperty] public partial SolidColorBrush Foreground { get; set; } = ColorPaletteCache.GetBrush("#246840");

    partial void OnDurationChanged(int value) => UpdateBackgroundColor();

    /// <summary>
    /// Updates the display colors based on the remaining duration.
    /// </summary>
    public void UpdateBackgroundColor()
    {
        var (background, foreground) = Duration switch
        {
            >= 4 => ("#246840", "#3bab6a"),
            >= 2 => ("#A58A00", "#BBA73F"),
            >= 0 => ("#A32622", "#e2342e"),
            _ => ("#3bab6a", "#3bab6a")
        };
        Background = ColorPaletteCache.GetBrush(background);
        Foreground = ColorPaletteCache.GetBrush(foreground);
    }

    /// <summary>
    /// Formats the effect name and duration for display.
    /// </summary>
    public string Display()
    {
        var duration = Duration switch { -1 => "Perm", -2 => "?", _ => Duration.ToString() };
        return $"{Name,-18} : {duration}";
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name,-18} : modifies {Modifies} by {Modifier} for {Duration} ticks";
}
