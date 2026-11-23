using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// Defines a named command expansion or script.
/// </summary>
[Display(Name = "Aliases")]
public partial class Alias : ObservableObject, IEditableModel
{
    /// <summary>
    /// Gets or sets a stable identifier.
    /// </summary>
    [property: Description("The unique identifier for the alias.")]
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [property: Browsable(false)]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the first word that invokes the alias.
    /// </summary>
    [property: Display(Name = "Alias")]
    [property: Description("The name of the alias.  This is what will be typed into the input box to trigger the alias.")]
    [property: DataListColumn(Name = "Alias", Width = 120)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("AliasExpression")]
    public partial string AliasExpression { get; set; } = "";

    /// <summary>
    /// Gets or sets the replacement command or JavaScript source.
    /// </summary>
    [property: Display(Name = "Command")]
    [property: Description("The command that will be executed when this alias is triggered.  It is either sent as commands or run as script based on Execute As.")]
    [property: DataListColumn(Name = "Command", Width = 260, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Command)]
    [ObservableProperty, JsonPropertyName("Command")]
    public partial string Command { get; set; } = "";

    /// <summary>
    /// Gets or sets the character name, or an empty string for any character.
    /// </summary>
    [property: Display(Name = "Character")]
    [property: Description("If the alias should only work for a specified Character.  This relies on the Character variable being populated.")]
    [property: DataListColumn(Name = "Character Name", Width = 100)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Character")]
    public partial string Character { get; set; } = "";

    /// <summary>
    /// Gets or sets the group name.
    /// </summary>
    [property: Display(Name = "Group")]
    [property: Description("The name of the group this alias belongs to.  Groups can be used to enable / disable batches of functionality easily.")]
    [property: DataListColumn(Name = "Group", Width = 100)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Group")]
    public partial string Group { get; set; } = "";

    /// <summary>
    /// Gets or sets how to execute the replacement.
    /// </summary>
    [property: Display(Name = "Execute As")]
    [property: Description("How the alias should be executed, either as a command or as JavaScript.")]
    [property: DataListColumn(Name = "Execute As", Width = 75)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("ExecuteAs")]
    public partial ExecuteType ExecuteAs { get; set; } = ExecuteType.Command;

    /// <summary>
    /// Gets or sets the order among aliases of the same name.
    /// </summary>
    [property: Display(Name = "Sort Order")]
    [property: Description("The order the alias should be sorted in if rendered in other views like on a button repeater.")]
    [property: DataListColumn(Name = "Sort", Width = 40)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("SortOrder")]
    public partial int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the package the alias belongs to.
    /// </summary>
    [property: Display(Name = "Package")]
    [property: Description("The name of the package the object belongs to.")]
    [property: DataListColumn(Name = "Package", Width = 100)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Package")]
    public partial string Package { get; set; } = "";

    /// <summary>
    /// Gets or sets a value that indicates whether the alias can run.
    /// </summary>
    [property: Display(Name = "Enabled")]
    [property: Description("If the alias is currently enabled.")]
    [property: DataListColumn(Name = "Enabled", Width = 55, CheckBox = true)]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("Enabled")]
    public partial bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the label on the alias button.
    /// </summary>
    [property: Display(Name = "Button Label")]
    [property: Description("The label to display on the button if its button visible property is true.")]
    [property: DataListColumn(Name = "Button Label", Width = 100, Visibility = Visibility.Hidden)]
    [property: ModelEditor]
    [property: Category("UI")]
    [ObservableProperty, JsonPropertyName("ButtonLabel")]
    public partial string ButtonLabel { get; set; } = "";

    /// <summary>
    /// Gets or sets whether a button for this alias shows in the Alias Button Actions window.
    /// </summary>
    [property: Display(Name = "Button Visible")]
    [property: Description("If a button for this alias should be visible on the alias actions panel.")]
    [property: DataListColumn(Name = "Button Visible", Width = 75, CheckBox = true)]
    [property: ModelEditor]
    [property: Category("UI")]
    [ObservableProperty, JsonPropertyName("ButtonVisible")]
    public partial bool ButtonVisible { get; set; }

    /// <summary>
    /// Gets or sets the number of times the alias has matched since loading the game.
    /// </summary>
    [property: Description("The number of times the alias has been executed.")]
    [property: DataListColumn(Name = "Count", Width = 60)]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonIgnore]
    public partial int Count { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{AliasExpression} {Command} {Character} {Group} {Package}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(AliasExpression);

    /// <inheritdoc/>
    public IEditableModel CloneNew()
    {
        var clone = ModelCloner.Clone(this);
        clone.Id = Guid.NewGuid().ToString();
        return clone;
    }
}
