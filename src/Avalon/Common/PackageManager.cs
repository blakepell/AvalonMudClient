using System.IO;
using System.Text.Json;
using System.Windows;
using Avalon.Dialogs;
using Avalon.Models;
using Avalon.Scripting;
using Mosaic.UI.Wpf;

namespace Avalon.Common;

/// <summary>
/// Compiles, imports and uninstalls local .package files.
/// </summary>
/// <remarks>
/// A package is a <see cref="PackageMetadata"/> plus every alias, trigger, direction, variable and script whose Package
/// property names it.  Compiling writes the bundle to Documents\Avalon\Packages; importing merges it into a game and
/// tags each item with the package name so it can be uninstalled.  The online package manager is not part of Avalon.
/// </remarks>
public static class PackageManager
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>
    /// Builds a package from the game's items tagged with the package name.
    /// </summary>
    public static Package Build(Game game, PackageMetadata metadata)
    {
        bool InPackage(string package) => package.Equals(metadata.Name, StringComparison.OrdinalIgnoreCase);
        var package = JsonSerializer.Deserialize<Package>(JsonSerializer.Serialize<PackageMetadata>(metadata))!;
        package.Aliases = game.Aliases.Where(a => InPackage(a.Package)).ToList();
        package.Triggers = game.Triggers.Where(t => InPackage(t.Package)).ToList();
        package.Directions = game.Directions.Where(d => InPackage(d.Package)).ToList();
        package.Variables = game.Variables.Where(v => InPackage(v.Package)).ToList();
        package.Scripts = game.Scripts.Where(s => InPackage(s.Package)).ToList();
        return package;
    }

    /// <summary>
    /// Asks for a package (when not given) and writes it to the packages folder.
    /// </summary>
    public static void CompileInteractive(Game? game, PackageMetadata? metadata = null)
    {
        if (game is null)
        {
            return;
        }

        if (metadata is null)
        {
            if (game.Packages.Count == 0)
            {
                MessageBox.Show("Create a package first (Developer > Packages > New), then set the Package of the aliases, triggers and scripts to include.",
                    "Compile Package", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var name = InputBox.Show("Compile Package", $"Package name ({string.Join(", ", game.Packages.Select(p => p.Name))}):", game.Packages[0].Name);
            metadata = game.Packages.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (metadata is null)
            {
                return;
            }
        }

        var package = Build(game, metadata);
        int count = package.Aliases.Count + package.Triggers.Count + package.Directions.Count + package.Variables.Count + package.Scripts.Count;
        metadata.Version++;
        metadata.ModifiedDate = DateTime.UtcNow;
        package.Version = metadata.Version;
        package.ModifiedDate = metadata.ModifiedDate;

        var path = Path.Combine(AppPaths.PackagesFolder, $"{Sanitize(metadata.Name)}.package");
        File.WriteAllText(path, JsonSerializer.Serialize(package, Options));
        App.SaveAppSettings();
        MessageBox.Show($"{metadata.Name} version {metadata.Version} was compiled with {count} item(s) to:\n\n{path}", "Compile Package",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Asks for a .package file and merges it into the game.
    /// </summary>
    public static void ImportInteractive(Game? game) =>
        ImportInteractive(game, "Import Package", "Packages (*.package)|*.package|All files (*.*)|*.*",
            json => (JsonSerializer.Deserialize<Package>(json), ""));

#if DEBUG
    /// <summary>
    /// Asks for a Continuum export, converts its contents and installs the package into the game.
    /// </summary>
    internal static void ImportContinuumInteractive(Game? game) =>
        ImportInteractive(game, "Import Continuum Package",
            "Continuum packages (*.package;*.json)|*.package;*.json|All files (*.*)|*.*",
            ContinuumPackageImporter.Convert);
#endif

    private static void ImportInteractive(Game? game, string title, string filter,
        Func<string, (Package? Package, string Notes)> readPackage)
    {
        if (game is null)
        {
            return;
        }

        var path = WpfUtilities.OpenFileRequest(title, filter, AppPaths.PackagesFolder);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var (package, notes) = readPackage(File.ReadAllText(path));
            if (package is null || string.IsNullOrWhiteSpace(package.Name))
            {
                throw new JsonException("The file is not a package.");
            }

            int count = Install(game, package);
            App.SaveAppSettings();
            if (!string.IsNullOrWhiteSpace(package.OnInstallEvent) && Argus.Memory.AppServices.GetRequiredService<AppViewModel>().ActiveTerminal is { } terminal
                && ReferenceEquals(terminal.Game, game))
            {
                _ = terminal.Interpreter.RunScriptTextAsync(package.OnInstallEvent);
            }
            MessageBox.Show($"{package.Name} version {package.Version} was installed ({count} item(s)).{notes}", title,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Replaces the game's items from an earlier version of the package with this one.
    /// </summary>
    public static int Install(Game game, Package package)
    {
        Uninstall(game, package.Name);
        foreach (var alias in package.Aliases)
        {
            game.Aliases.Add(Tag(alias, package.Name));
        }

        foreach (var trigger in package.Triggers)
        {
            game.Triggers.Add(Tag(trigger, package.Name));
        }

        foreach (var direction in package.Directions)
        {
            game.Directions.Add(Tag(direction, package.Name));
        }

        foreach (var variable in package.Variables.Where(v => !game.Variables.Any(existing => existing.Key.Equals(v.Key, StringComparison.OrdinalIgnoreCase))))
        {
            game.Variables.Add(Tag(variable, package.Name));
        }

        foreach (var script in package.Scripts)
        {
            game.Scripts.Add(Tag(script, package.Name));
        }

        var metadata = game.Packages.FirstOrDefault(p => p.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));
        if (metadata is null)
        {
            game.Packages.Add(JsonSerializer.Deserialize<PackageMetadata>(JsonSerializer.Serialize<PackageMetadata>(package))!);
        }
        else
        {
            metadata.Version = package.Version;
        }

        return package.Aliases.Count + package.Triggers.Count + package.Directions.Count + package.Variables.Count + package.Scripts.Count;
    }

    /// <summary>
    /// Asks before removing every item tagged with the package.
    /// </summary>
    public static void UninstallInteractive(Game? game, PackageMetadata metadata)
    {
        if (game is null)
        {
            return;
        }

        if (MessageBox.Show($"Remove every alias, trigger, direction and script in the '{metadata.Name}' package?", "Uninstall Package",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        int count = Uninstall(game, metadata.Name);
        App.SaveAppSettings();
        MessageBox.Show($"{count} item(s) were removed.", "Uninstall Package", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Removes every item tagged with the package and returns how many were removed.
    /// </summary>
    public static int Uninstall(Game game, string name)
    {
        bool InPackage(string package) => package.Equals(name, StringComparison.OrdinalIgnoreCase);
        int count = 0;
        count += RemoveAll(game.Aliases, a => InPackage(a.Package));
        count += RemoveAll(game.Triggers, t => InPackage(t.Package));
        count += RemoveAll(game.Directions, d => InPackage(d.Package));
        count += RemoveAll(game.Scripts, s => InPackage(s.Package));
        return count;
    }

    private static int RemoveAll<T>(IList<T> items, Func<T, bool> predicate)
    {
        var remove = items.Where(predicate).ToList();
        foreach (var item in remove)
        {
            items.Remove(item);
        }

        return remove.Count;
    }

    private static T Tag<T>(T item, string package) where T : class
    {
        item.GetType().GetProperty("Package")?.SetValue(item, package);
        return item;
    }

    private static string Sanitize(string name) => string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
