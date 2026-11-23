using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using Avalon.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Scripting;

/// <summary>
/// Stores JavaScript source shared by commands, triggers, and GMCP handlers.
/// </summary>
[Display(Name = "Scripts")]
public partial class SourceCode : ObservableObject, IEditableModel
{
    /// <summary>
    /// Gets or sets a stable identifier.
    /// </summary>
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the script name or GMCP package that invokes the script.
    /// </summary>
    [property: Display(Name = "Name")]
    [property: Description("The name of the script.  A script named after a GMCP package, such as Char.Vitals, handles that package.")]
    [property: DataListColumn(Name = "Name", Width = 180)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Name")]
    public partial string Name { get; set; } = "New Script";

    /// <summary>
    /// Gets or sets the scripting language.
    /// </summary>
    [property: DataListColumn(Name = "Type", Width = 75, Visibility = Visibility.Hidden)]
    [ObservableProperty, JsonPropertyName("ScriptType")]
    public partial ScriptType ScriptType { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript source.
    /// </summary>
    [property: Display(Name = "Code")]
    [property: Description("The JavaScript source.  Functions it declares can be called from any alias, trigger or script.")]
    [property: DataListColumn(Name = "Code", Width = 260, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("Code")]
    public partial string Code { get; set; } = "";

    /// <summary>
    /// Gets or sets the GMCP package that runs this source with package and json variables in scope.
    /// </summary>
    [property: Display(Name = "GMCP Package")]
    [property: Description("If set, the code runs whenever this GMCP package arrives, with package and json in scope.")]
    [property: DataListColumn(Name = "GMCP Package", Width = 110)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("GmcpPackage")]
    public partial string? GmcpPackage { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [property: Display(Name = "Description")]
    [property: DataListColumn(Name = "Description", Width = 160, Trim = true)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Description")]
    public partial string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the package the script belongs to.
    /// </summary>
    [property: Display(Name = "Package")]
    [property: Description("The name of the package the object belongs to.")]
    [property: DataListColumn(Name = "Package", Width = 90)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Package")]
    public partial string Package { get; set; } = "";

    /// <inheritdoc/>
    public override string ToString() => $"{Name} {GmcpPackage} {Description} {Package}";

    /// <inheritdoc/>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Name);

    /// <inheritdoc/>
    public IEditableModel CloneNew()
    {
        var clone = ModelCloner.Clone(this);
        clone.Id = Guid.NewGuid().ToString();
        return clone;
    }
}
