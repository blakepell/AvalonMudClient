using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Json;

namespace Avalon.Models;

/// <summary>
/// Colors any text in the terminal that matches a word or regular expression.
/// </summary>
[Display(Name = "Highlight Triggers")]
public partial class HighlightTrigger : ObservableObject, IEditableModel
{
    private Regex? _regex;

    /// <summary>
    /// Gets or sets a key that scripts can use to find this highlight.
    /// </summary>
    [property: Display(Name = "Lookup Key")]
    [property: Description("A key that can be used to match a specific Highlight Trigger.")]
    [property: DataListColumn(Name = "Lookup Key", Width = 120)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("LookupKey")]
    public partial string LookupKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the word or regular expression to highlight.
    /// </summary>
    [property: Display(Name = "Pattern")]
    [property: Description("The word to highlight or the regular expression to match.")]
    [property: DataListColumn(Name = "Pattern", Width = 220, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Regex)]
    [ObservableProperty, JsonPropertyName("Pattern")]
    public partial string Pattern { get; set; } = "";

    /// <summary>
    /// Gets or sets the background of the matched text.
    /// </summary>
    [property: Display(Name = "Background Color")]
    [property: Description("The background color of the matching pattern.")]
    [property: ModelEditor]
    [property: JsonConverter(typeof(SolidColorBrushSerializer))]
    [ObservableProperty, JsonPropertyName("BackgroundColor")]
    public partial SolidColorBrush BackgroundColor { get; set; } = Brushes.Black;

    /// <summary>
    /// Gets or sets the foreground of the matched text.
    /// </summary>
    [property: Display(Name = "Foreground Color")]
    [property: Description("The foreground color of the matching pattern.")]
    [property: ModelEditor]
    [property: JsonConverter(typeof(SolidColorBrushSerializer))]
    [ObservableProperty, JsonPropertyName("ForegroundColor")]
    public partial SolidColorBrush ForegroundColor { get; set; } = Brushes.DarkRed;

    /// <summary>
    /// Gets or sets whether the highlight is applied.
    /// </summary>
    [property: Display(Name = "Enabled")]
    [property: Description("If the trigger is currently enabled.")]
    [property: DataListColumn(Name = "Enabled", Width = 55, CheckBox = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Enabled")]
    public partial bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the longest observed time to scan one line, in milliseconds.
    /// </summary>
    [property: DataListColumn(Name = "Max ms", Width = 55)]
    [property: Category("Performance")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonIgnore]
    public partial double MaxExecutionTime { get; set; }

    /// <summary>
    /// Gets the compiled expression, or null when the pattern is empty or invalid.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public Regex? Regex
    {
        get
        {
            if (_regex is null && !string.IsNullOrWhiteSpace(Pattern))
            {
                try
                {
                    _regex = new Regex(Pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            return _regex;
        }
    }

    partial void OnPatternChanged(string value) => _regex = null;

    /// <inheritdoc/>
    public override string ToString() => $"{LookupKey} {Pattern}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Pattern) && Regex is not null;

    /// <inheritdoc/>
    public IEditableModel CloneNew() => ModelCloner.Clone(this);
}
