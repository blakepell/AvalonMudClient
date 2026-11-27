using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Argus.Memory;
using Avalon.Models;
using Avalon.Scripting;
using Esprima;
using ModelContextProtocol;
using Mosaic.UI.Wpf.Cache;

namespace Avalon.Mcp;

/// <summary>
/// Shared helpers for the MCP tools: resolving the profile a request targets, finding items, validating
/// JavaScript and saving. Members other than <see cref="Json"/> must run on the UI thread.
/// </summary>
internal static partial class McpContext
{
    /// <summary>
    /// The description shared by every tool's optional profile parameter.
    /// </summary>
    public const string ProfileDescription =
        "The profile (game) to work with, by name or ID. Leave empty to use the profile whose terminal is showing. Use list_profiles to see the profiles.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static AppSettings Settings => AppServices.GetRequiredService<AppSettings>();

    private static AppViewModel ViewModel => AppServices.GetRequiredService<AppViewModel>();

    /// <summary>
    /// Serializes a tool result.
    /// </summary>
    public static string Json(object value) => JsonSerializer.Serialize(value, JsonOptions);

    /// <summary>
    /// Gets every saved profile.
    /// </summary>
    public static IEnumerable<Game> Profiles => Settings.Games;

    /// <summary>
    /// Gets the profile whose terminal is showing, or null.
    /// </summary>
    public static Game? ActiveProfile => ViewModel.ActiveTerminal?.Game;

    /// <summary>
    /// Returns whether a terminal is open for the profile.
    /// </summary>
    public static bool IsOpen(Game game) =>
        System.Windows.Application.Current.MainWindow is MainWindow main && main.FindTerminal(game) is not null;

    /// <summary>
    /// Resolves the profile a tool call targets.
    /// </summary>
    /// <param name="profile">A profile name or ID, or empty for the active profile.</param>
    /// <exception cref="McpException">No profile matches.</exception>
    public static Game ResolveProfile(string? profile)
    {
        if (string.IsNullOrWhiteSpace(profile))
        {
            if (ActiveProfile is { } active)
            {
                return active;
            }

            var games = Settings.Games.ToArray();
            return games.Length == 1
                ? games[0]
                : throw new McpException($"No game terminal is open, so name the profile to use. Profiles: {ProfileNames()}.");
        }

        var trimmed = profile.Trim();
        var byId = Settings.Games.FirstOrDefault(game => game.Id?.ToString("D").Equals(trimmed, StringComparison.OrdinalIgnoreCase) == true);
        if (byId is not null)
        {
            return byId;
        }

        var byName = Settings.Games.Where(game => string.Equals(game.Name, trimmed, StringComparison.OrdinalIgnoreCase)).ToArray();
        return byName.Length switch
        {
            1 => byName[0],
            0 => throw new McpException($"There is no profile named \"{trimmed}\". Profiles: {ProfileNames()}."),
            _ => throw new McpException($"More than one profile is named \"{trimmed}\". Use the profile's ID from list_profiles.")
        };
    }

    private static string ProfileNames()
    {
        var names = Settings.Games.Select(game => $"\"{game.Name}\"").ToArray();
        return names.Length == 0 ? "(none)" : string.Join(", ", names);
    }

    /// <summary>
    /// Finds an item by its ID, falling back to a unique name match.
    /// </summary>
    /// <exception cref="McpException">No item, or more than one item, matches.</exception>
    public static T FindItem<T>(ObservableCollection<T> items, string idOrName, Func<T, string> id, Func<T, string?> name, string kind)
    {
        if (string.IsNullOrWhiteSpace(idOrName))
        {
            throw new McpException($"Give the {kind}'s ID or name.");
        }

        var key = idOrName.Trim();
        var byId = items.FirstOrDefault(item => string.Equals(id(item), key, StringComparison.OrdinalIgnoreCase));
        if (byId is not null)
        {
            return byId;
        }

        var byName = items.Where(item => string.Equals(name(item), key, StringComparison.OrdinalIgnoreCase)).ToArray();
        return byName.Length switch
        {
            1 => byName[0],
            0 => throw new McpException($"No {kind} has the ID or name \"{key}\"."),
            _ => throw new McpException($"{byName.Length} {kind}s are named \"{key}\". Use the ID instead: {string.Join(", ", byName.Select(id))}.")
        };
    }

    /// <summary>
    /// Returns whether the item's searchable text contains the search text.
    /// </summary>
    public static bool Matches(object item, string? search, string? group = null, string? itemGroup = null)
    {
        if (!string.IsNullOrWhiteSpace(group) && !string.Equals(group.Trim(), itemGroup, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(search) || (item.ToString() ?? "").Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses an Execute As value.
    /// </summary>
    public static ExecuteType ParseExecuteAs(string value) => value.Trim().ToLowerInvariant() switch
    {
        "command" or "commands" or "cmd" => ExecuteType.Command,
        "javascript" or "js" or "script" => ExecuteType.JavaScript,
        _ => throw new McpException($"\"{value}\" is not a valid execute_as value. Use \"Command\" or \"JavaScript\".")
    };

    /// <summary>
    /// Throws when a trigger pattern is not a valid .NET regular expression.
    /// </summary>
    public static void ValidatePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new McpException("The trigger pattern can't be empty.");
        }

        try
        {
            _ = new Regex(pattern);
        }
        catch (ArgumentException ex)
        {
            throw new McpException($"The pattern is not a valid .NET regular expression: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks JavaScript syntax with the parser the profile's script engine uses.
    /// </summary>
    /// <param name="code">The code to check.</param>
    /// <param name="hasPlaceholders">Whether %0-%9, %Line and %LineRaw are substituted before the code runs.</param>
    /// <param name="expression">Whether the code runs as an expression (a JavaScript line transformer).</param>
    /// <returns>Null when the code parses, otherwise the error.</returns>
    public static string? CheckSyntax(string code, bool hasPlaceholders, bool expression = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        if (hasPlaceholders)
        {
            // Placeholders are replaced with escaped text before the code runs, so a stand-in keeps the
            // code parseable whether or not the placeholder is inside quotes.
            code = PlaceholderPattern().Replace(code, "0");
        }

        try
        {
            var parser = new JavaScriptParser(code, ScriptEnvironmentFactory.ForActiveGame().Engine.Options.ParserOptions);
            if (expression)
            {
                parser.ParseExpression();
            }
            else
            {
                parser.ParseScript();
            }

            return null;
        }
        catch (ParserException ex)
        {
            return ex.LineNumber > 0
                ? $"Line {ex.LineNumber}, column {ex.Column}: {ex.Description ?? ex.Message}"
                : ex.Description ?? ex.Message;
        }
    }

    /// <summary>
    /// Throws when JavaScript does not parse.
    /// </summary>
    public static void RequireValidSyntax(string code, bool hasPlaceholders, bool expression = false)
    {
        if (CheckSyntax(code, hasPlaceholders, expression) is { } error)
        {
            throw new McpException($"The JavaScript has a syntax error, so nothing was changed. {error}");
        }
    }

    /// <summary>
    /// Converts a hex color to a brush, returning null for empty text.
    /// </summary>
    public static SolidColorBrush? ParseBrush(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return ColorPaletteCache.GetBrush((Color)ColorConverter.ConvertFromString(value.Trim()));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or NotSupportedException)
        {
            throw new McpException($"\"{value}\" is not a valid color. Use a hex color such as #FF0000.");
        }
    }

    /// <summary>
    /// Formats a brush as hex, or null.
    /// </summary>
    public static string? FormatBrush(SolidColorBrush? brush) => brush?.Color.ToString();

    /// <summary>
    /// Saves the settings, which include every profile.
    /// </summary>
    /// <returns>Null when saved, otherwise a warning for the tool result.</returns>
    public static string? Save()
    {
        try
        {
            App.SaveAppSettings();
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"The change was made but could not be saved to disk: {ex.Message}";
        }
    }

    [GeneratedRegex(@"%(?:LineRaw|Line|\d)")]
    private static partial Regex PlaceholderPattern();
}
