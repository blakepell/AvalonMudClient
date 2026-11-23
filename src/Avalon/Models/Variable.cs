using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Json;

namespace Avalon.Models;

/// <summary>
/// Stores a named game value for command expansion and scripts.
/// </summary>
[Display(Name = "Variables")]
public partial class Variable : ObservableObject, IEditableModel
{
    /// <summary>
    /// Gets or sets the name used to look up the variable.
    /// </summary>
    [property: Display(Name = "Key")]
    [property: Description("The name of the variable.  Reference it in commands as @Key.")]
    [property: DataListColumn(Name = "Key", Width = 140)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Key")]
    public partial string Key { get; set; } = "";

    /// <summary>
    /// Gets or sets the value as text.
    /// </summary>
    [property: Display(Name = "Value")]
    [property: Description("The value of the variable.")]
    [property: DataListColumn(Name = "Value", Width = 200, Trim = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Value")]
    public partial string Value { get; set; } = "";

    /// <summary>
    /// Gets or sets the character name, or an empty string for any character.
    /// </summary>
    [property: Display(Name = "Character")]
    [property: Description("If the variable is only for a specified Character.  Character variables take precedence over global ones.")]
    [property: DataListColumn(Name = "Character", Width = 100)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Character")]
    public partial string Character { get; set; } = "";

    /// <summary>
    /// Gets or sets the order in the Variables window.
    /// </summary>
    [property: Display(Name = "Display Order")]
    [property: Description("The order the variable is shown in the Variables window.")]
    [property: DataListColumn(Name = "Order", Width = 50)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("DisplayOrder")]
    public partial int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the label in the Variables window.
    /// </summary>
    [property: Display(Name = "Label")]
    [property: Description("The label shown in the Variables window, defaulting to the key.")]
    [property: DataListColumn(Name = "Label", Width = 100)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Label")]
    public partial string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets whether the variable shows in the Variables window.
    /// </summary>
    [property: Display(Name = "Visible")]
    [property: Description("If the variable is shown in the Variables window.")]
    [property: DataListColumn(Name = "Visible", Width = 50, CheckBox = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("IsVisible")]
    public partial bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the value color in the Variables window.
    /// </summary>
    [property: Display(Name = "Foreground Color")]
    [property: Description("The color of the value in the Variables window.")]
    [property: ModelEditor]
    [property: JsonConverter(typeof(SolidColorBrushSerializer))]
    [ObservableProperty, JsonPropertyName("ForegroundColor")]
    public partial SolidColorBrush? ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the row color in the Variables window.
    /// </summary>
    [property: Display(Name = "Background Color")]
    [property: Description("The background of the row in the Variables window.")]
    [property: ModelEditor]
    [property: JsonConverter(typeof(SolidColorBrushSerializer))]
    [ObservableProperty, JsonPropertyName("BackgroundColor")]
    public partial SolidColorBrush? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript to run on change, with %1 and %2 for the old and new values.
    /// </summary>
    [property: Display(Name = "On Change Event")]
    [property: Description("JavaScript that runs when the value changes.  %1 is the old value and %2 is the new value.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("OnChangeEvent")]
    public partial string? OnChangeEvent { get; set; }

    /// <summary>
    /// Gets or sets the package the variable belongs to.
    /// </summary>
    [property: Display(Name = "Package")]
    [property: Description("The name of the package the object belongs to.")]
    [property: DataListColumn(Name = "Package", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Package")]
    public partial string Package { get; set; } = "";

    /// <summary>
    /// Gets the label shown in the Variables window.
    /// </summary>
    [JsonIgnore, Browsable(false)]
    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? Key : Label;

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    partial void OnKeyChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    /// <inheritdoc/>
    public override string ToString() => $"{Key} {Value} {Character} {Label} {Package}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Key) && !Key.Contains(' ');

    /// <inheritdoc/>
    public IEditableModel CloneNew() => ModelCloner.Clone(this);
}
