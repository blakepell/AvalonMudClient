using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// Defines an action that runs when game output matches a regular expression.
/// </summary>
[Display(Name = "Triggers")]
public partial class Trigger : ObservableObject, IEditableModel
{
    private Regex? _regex;

    /// <summary>
    /// Gets or sets a stable identifier.
    /// </summary>
    [property: Description("The unique identifier for the trigger.")]
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [property: Category("Internal")]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [property: Display(Name = "Name")]
    [property: Description("An optional friendly name for the trigger.")]
    [property: DataListColumn(Name = "Name", Width = 110)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Name")]
    public partial string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the regular expression applied to plain server output.
    /// </summary>
    [property: Display(Name = "Pattern")]
    [property: Description("The regular expression pattern that will be matched against each line of game output.")]
    [property: DataListColumn(Name = "Pattern", Width = 200, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Regex)]
    [ObservableProperty, JsonPropertyName("Pattern")]
    public partial string Pattern { get; set; } = "";

    /// <summary>
    /// Gets or sets the response command, replacement text, or JavaScript source.
    /// </summary>
    [property: Display(Name = "Command")]
    [property: Description("The command or script that runs when the pattern matches.  %0..%9 are the match groups, %Line is the line.")]
    [property: DataListColumn(Name = "Command", Width = 200, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Command)]
    [ObservableProperty, JsonPropertyName("Command")]
    public partial string Command { get; set; } = "";

    /// <summary>
    /// Gets or sets the group name.
    /// </summary>
    [property: Display(Name = "Group")]
    [property: Description("The name of the group this trigger belongs to.  Groups can be used to enable / disable batches of functionality easily.")]
    [property: DataListColumn(Name = "Group", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Group")]
    public partial string Group { get; set; } = "";

    /// <summary>
    /// Gets or sets the order of evaluation, with lower values first.
    /// </summary>
    [property: Display(Name = "Priority")]
    [property: Description("The order triggers are evaluated in, lowest first.")]
    [property: DataListColumn(Name = "Priority", Width = 55)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Priority")]
    public partial int Priority { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the character name, or an empty string for any character.
    /// </summary>
    [property: Display(Name = "Character")]
    [property: Description("If the trigger should only fire for a specified Character.  This relies on the Character variable being populated.")]
    [property: DataListColumn(Name = "Character", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Character")]
    public partial string Character { get; set; } = "";

    /// <summary>
    /// Gets or sets how to execute the response.
    /// </summary>
    [property: Display(Name = "Execute As")]
    [property: Description("How the command should be executed, either as a command or as JavaScript.")]
    [property: DataListColumn(Name = "Execute As", Width = 75)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("ExecuteAs")]
    public partial ExecuteType ExecuteAs { get; set; } = ExecuteType.Command;

    /// <summary>
    /// Gets or sets the package the trigger belongs to.
    /// </summary>
    [property: Display(Name = "Package")]
    [property: Description("The name of the package the object belongs to.")]
    [property: DataListColumn(Name = "Package", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Package")]
    public partial string Package { get; set; } = "";

    /// <summary>
    /// Gets or sets a value that indicates whether the trigger can run.
    /// </summary>
    [property: Display(Name = "Enabled")]
    [property: Description("If the trigger is currently enabled.")]
    [property: DataListColumn(Name = "Enabled", Width = 55, CheckBox = true)]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("Enabled")]
    public partial bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether to suppress local command echo.
    /// </summary>
    [property: Display(Name = "Silent")]
    [property: Description("If the commands sent by this trigger are echoed to the terminal.")]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("IsSilent")]
    public partial bool IsSilent { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether to disable the trigger after it matches.
    /// </summary>
    [property: Display(Name = "Disable After Triggered")]
    [property: Description("If the trigger disables itself after it fires once.")]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("DisableAfterTriggered")]
    public partial bool DisableAfterTriggered { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether to expand game variables in the pattern.
    /// </summary>
    [property: Display(Name = "Variable Replacement")]
    [property: Description("If @variables in the pattern are replaced with their values before matching.")]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("VariableReplacement")]
    public partial bool VariableReplacement { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the response replaces the matching text.
    /// </summary>
    [property: Display(Name = "Line Transformer")]
    [property: Description("If the command replaces the matched text in the terminal instead of being sent.")]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("LineTransformer")]
    public partial bool LineTransformer { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether to hide a matching line.
    /// </summary>
    [property: Display(Name = "Gag")]
    [property: Description("If the matching line is hidden from the terminal.")]
    [property: DataListColumn(Name = "Gag", Width = 40, CheckBox = true)]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("Gag")]
    public partial bool Gag { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether to skip the remaining triggers after matching.
    /// </summary>
    [property: Display(Name = "Stop Processing")]
    [property: Description("If no other triggers are checked against the line after this one matches.")]
    [property: ModelEditor]
    [property: Category("Options")]
    [ObservableProperty, JsonPropertyName("StopProcessing")]
    public partial bool StopProcessing { get; set; }

    /// <summary>
    /// Gets or sets whether the trigger is temporary and is not saved.
    /// </summary>
    [property: Description("A temporary trigger that is removed after it fires and is never saved.")]
    [property: Category("Options")]
    [ObservableProperty, JsonIgnore]
    public partial bool Temp { get; set; }

    /// <summary>
    /// Gets or sets the number of matches since loading the game.
    /// </summary>
    [property: DataListColumn(Name = "Count", Width = 55)]
    [property: Category("Performance")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonIgnore]
    public partial int Count { get; set; }

    /// <summary>
    /// Gets or sets the UTC time of the most recent match.
    /// </summary>
    [property: Category("Performance")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonIgnore]
    public partial DateTime LastMatched { get; set; }

    /// <summary>
    /// Gets or sets the longest time, in milliseconds, this trigger has taken to process.
    /// </summary>
    [property: Display(Name = "Max Execution Time")]
    [property: DataListColumn(Name = "Max ms", Width = 55)]
    [property: Category("Performance")]
    [property: ReadOnly(true)]
    [ObservableProperty, JsonIgnore]
    public partial double MaxExecutionTime { get; set; }

    internal Match Match(string line, string pattern)
    {
        if (_regex?.ToString() != pattern)
        {
            _regex = new Regex(pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
        }

        return _regex!.Match(line);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} {Pattern} {Command} {Group} {Character} {Package}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Pattern) && IsValidPattern(Pattern);

    /// <inheritdoc/>
    public IEditableModel CloneNew()
    {
        var clone = ModelCloner.Clone(this);
        clone.Id = Guid.NewGuid().ToString();
        return clone;
    }

    private static bool IsValidPattern(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
