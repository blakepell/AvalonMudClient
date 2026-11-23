using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using Avalon.Common;
using Avalon.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalon.Models;

/// <summary>
/// Describes a package: a named, versioned bundle of aliases, triggers, directions, variables and scripts.
/// </summary>
[Display(Name = "Packages")]
public partial class PackageMetadata : ObservableObject, IEditableModel
{
    /// <summary>
    /// Gets or sets a stable identifier.
    /// </summary>
    [property: DataListColumn(Name = "ID", Visibility = Visibility.Hidden)]
    [ObservableProperty, JsonPropertyName("Id")]
    public partial string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the package name, which items reference through their Package property.
    /// </summary>
    [property: Display(Name = "Name")]
    [property: Description("The name of the package.  Items whose Package matches this name are included when compiling.")]
    [property: DataListColumn(Name = "Name", Width = 160)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Name")]
    public partial string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    [property: Display(Name = "Author")]
    [property: DataListColumn(Name = "Author", Width = 110)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Author")]
    public partial string Author { get; set; } = "";

    /// <summary>
    /// Gets or sets the version, which increases each time the package is compiled.
    /// </summary>
    [property: Display(Name = "Version")]
    [property: DataListColumn(Name = "Version", Width = 60)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("Version")]
    public partial int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [property: Display(Name = "Description")]
    [property: DataListColumn(Name = "Description", Width = 220, Trim = true)]
    [property: ModelEditor(TextEditor = TextEditorKind.Text)]
    [ObservableProperty, JsonPropertyName("Description")]
    public partial string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the address of the game the package was written for.
    /// </summary>
    [property: Display(Name = "Site IP Address")]
    [property: Description("The game the package is meant for.")]
    [property: DataListColumn(Name = "Site", Width = 110)]
    [property: ModelEditor]
    [ObservableProperty, JsonPropertyName("SiteIpAddress")]
    public partial string SiteIpAddress { get; set; } = "";

    /// <summary>
    /// Gets or sets JavaScript that runs once when the package is installed.
    /// </summary>
    [property: Display(Name = "On Install Event")]
    [property: Description("JavaScript that runs when the package is installed.")]
    [property: ModelEditor(TextEditor = TextEditorKind.Script)]
    [ObservableProperty, JsonPropertyName("OnInstallEvent")]
    public partial string OnInstallEvent { get; set; } = "";

    /// <summary>
    /// Gets or sets the Markdown documentation.
    /// </summary>
    [property: Display(Name = "Markdown Documentation")]
    [property: ModelEditor(TextEditor = TextEditorKind.Text)]
    [ObservableProperty, JsonPropertyName("MarkdownDocumentation")]
    public partial string MarkdownDocumentation { get; set; } = "";

    /// <summary>
    /// Gets or sets the UTC time the package was last compiled.
    /// </summary>
    [property: DataListColumn(Name = "Modified", Width = 130)]
    [ObservableProperty, JsonPropertyName("ModifiedDate")]
    public partial DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public override string ToString() => $"{Name} {Author} {Description}";

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

/// <summary>
/// A package with its contents, as written to a .package file.
/// </summary>
public sealed class Package : PackageMetadata
{
    /// <summary>
    /// Gets or sets the packaged aliases.
    /// </summary>
    public List<Alias> Aliases { get; set; } = [];

    /// <summary>
    /// Gets or sets the packaged triggers.
    /// </summary>
    public List<Trigger> Triggers { get; set; } = [];

    /// <summary>
    /// Gets or sets the packaged directions.
    /// </summary>
    public List<Direction> Directions { get; set; } = [];

    /// <summary>
    /// Gets or sets the packaged variables.
    /// </summary>
    public List<Variable> Variables { get; set; } = [];

    /// <summary>
    /// Gets or sets the packaged scripts.
    /// </summary>
    public List<SourceCode> Scripts { get; set; } = [];
}
