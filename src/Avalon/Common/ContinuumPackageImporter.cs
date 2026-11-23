#if DEBUG
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalon.Models;
using Avalon.Scripting;

namespace Avalon.Common;

/// <summary>
/// Converts the shared fields in a Continuum export to Avalon's package model.
/// </summary>
internal static class ContinuumPackageImporter
{
    private static readonly string[] CollectionNames =
        [nameof(Package.Aliases), nameof(Package.Triggers), nameof(Package.Directions), nameof(Package.Variables), nameof(Package.Scripts)];

    /// <summary>
    /// Reads a Continuum export and summarizes any entries that could not be converted.
    /// </summary>
    internal static (Package? Package, string Notes) Convert(string json)
    {
        var documentOptions = new JsonDocumentOptions { AllowDuplicateProperties = false };
        if (JsonNode.Parse(json, documentOptions: documentOptions) is not JsonObject root
            || !CollectionNames.Any(name => root[name] is not null))
        {
            throw new JsonException("The file does not contain a Continuum package.");
        }

        var collections = new Dictionary<string, JsonNode?>();
        foreach (var name in CollectionNames)
        {
            root.Remove(name, out var collection);
            collections.Add(name, collection);
        }

        var package = ReadObject<Package>(root);
        if (!package.IsValid())
        {
            throw new JsonException("The Continuum package must have a name.");
        }

        var skipped = new List<string>();
        package.Aliases = ConvertItems<Alias>(collections, nameof(Package.Aliases), skipped);
        package.Triggers = ConvertItems<Trigger>(collections, nameof(Package.Triggers), skipped);
        package.Directions = ConvertItems<Direction>(collections, nameof(Package.Directions), skipped);
        package.Variables = ConvertItems<Variable>(collections, nameof(Package.Variables), skipped);
        package.Scripts = ConvertItems<SourceCode>(collections, nameof(Package.Scripts), skipped);

        string notes = "";
        if (skipped.Count > 0)
        {
            if (package.Aliases.Count + package.Triggers.Count + package.Directions.Count + package.Variables.Count + package.Scripts.Count == 0)
            {
                throw new JsonException($"No package entries could be converted ({string.Join(", ", skipped)}).");
            }

            notes = $"\n\nSkipped entries ({string.Join(", ", skipped)}).";
        }

        if (package.Scripts.Count > 0 || !string.IsNullOrWhiteSpace(package.OnInstallEvent)
            || package.Aliases.Any(item => item.ExecuteAs == ExecuteType.JavaScript)
            || package.Triggers.Any(item => item.ExecuteAs == ExecuteType.JavaScript)
            || package.Variables.Any(item => !string.IsNullOrWhiteSpace(item.OnChangeEvent)))
        {
            notes += "\n\nJavaScript was imported as written. Continuum-specific APIs may need edits.";
        }

        return (package, notes);
    }

    private static List<T> ConvertItems<T>(Dictionary<string, JsonNode?> collections, string name, List<string> skipped)
        where T : class, IEditableModel
    {
        if (collections[name] is not { } collection)
        {
            return [];
        }

        // Continuum uses reference-preserving JSON, which wraps each list in an object with a $values array.
        var entries = collection as JsonArray ?? (collection as JsonObject)?["$values"] as JsonArray
            ?? throw new JsonException($"The Continuum package has an invalid {name} collection.");

        var items = new List<T>();
        foreach (var entry in entries)
        {
            if (entry is not JsonObject item || item.ContainsKey("$ref"))
            {
                continue;
            }

            // Continuum embeds PackageMetadata or a reference here. Avalon tags each installed item by name.
            item.Remove("Package");
            try
            {
                var converted = ReadObject<T>(item);
                if (converted.IsValid())
                {
                    items.Add(converted);
                }
            }
            catch (Exception ex) when (ex is JsonException or FormatException or NotSupportedException)
            {
                // A malformed entry should not prevent the remaining entries from being imported.
            }
        }

        if (entries.Count != items.Count)
        {
            skipped.Add($"{name}: {entries.Count - items.Count}");
        }

        return items;
    }

    private static T ReadObject<T>(JsonObject item) where T : class
    {
        // Legacy nullable fields should use the Avalon model's defaults.
        foreach (var key in item.Where(property => property.Value is null).Select(property => property.Key).ToArray())
        {
            item.Remove(key);
        }

        return item.Deserialize<T>()!;
    }
}
#endif
