using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// Binds a key combination to a command or script.
/// </summary>
[Display(Name = "Macros")]
public partial class Macro : ObservableObject, IEditableModel
{
    /// <summary>
    /// Gets or sets the shortcut, such as Ctrl+NumPad8.
    /// </summary>
    [property: Display(Name = "Key Combination")]
    [property: Description("The keys that run the macro, such as Ctrl+NumPad8.")]
    [property: DataListColumn(Name = "Key Combination", Width = 140)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("KeyCombination")]
    public partial string KeyCombination { get; set; } = "";

    /// <summary>
    /// Gets or sets the command or JavaScript source.
    /// </summary>
    [property: Display(Name = "Command")]
    [property: Description("The command or script that runs when the keys are pressed.")]
    [property: DataListColumn(Name = "Command", Width = 300, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Command)]
    [ObservableProperty, JsonPropertyName("Command")]
    public partial string Command { get; set; } = "";

    /// <summary>
    /// Gets or sets how to execute the macro.
    /// </summary>
    [property: Display(Name = "Execute As")]
    [property: DataListColumn(Name = "Execute As", Width = 75)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("ExecuteAs")]
    public partial ExecuteType ExecuteAs { get; set; } = ExecuteType.Command;

    /// <summary>
    /// Gets or sets a value that indicates whether the macro can run.
    /// </summary>
    [property: Display(Name = "Enabled")]
    [property: DataListColumn(Name = "Enabled", Width = 55, CheckBox = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Enabled")]
    public partial bool Enabled { get; set; } = true;

    /// <inheritdoc/>
    public override string ToString() => $"{KeyCombination} {Command}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(KeyCombination);

    /// <inheritdoc/>
    public IEditableModel CloneNew() => ModelCloner.Clone(this);
}
