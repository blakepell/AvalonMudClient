using System.ComponentModel;
using Avalon.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// MCP tools that list, set and remove a profile's variables.
/// </summary>
[McpServerToolType]
public sealed class VariableTools
{
    private const string CharacterDescription =
        "The character the variable belongs to. Empty for a variable shared by every character. When both exist, the " +
        "current character's variable is used in place of the shared one.";

    /// <summary>
    /// Lists variables.
    /// </summary>
    [McpServerTool(Name = "list_variables", ReadOnly = true, Idempotent = true)]
    [Description("Lists the variables in a profile. Variables hold string values that commands use as @Key and scripts read with terminal.GetVariable(\"Key\").")]
    public static Task<string> ListVariables(
        [Description(McpContext.ProfileDescription)] string? profile = null,
        [Description("Only return variables whose key, value, character, label or package contains this text.")] string? search = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var variables = game.Variables.Where(variable => McpContext.Matches(variable, search)).Select(ToResult).ToArray();
            return McpContext.Json(new { profile = game.Name, count = variables.Length, variables });
        });

    /// <summary>
    /// Creates or updates a variable.
    /// </summary>
    [McpServerTool(Name = "set_variable", Idempotent = true)]
    [Description("Creates a variable, or updates the one with the same key and character, and saves the profile. Only the values you pass are changed on an existing variable. " +
                 "This edits the variable directly, so its On Change Event does not run.")]
    public static Task<string> SetVariable(
        [Description("The variable's name, such as \"Target\". It can't contain spaces. Keys aren't case-sensitive.")] string key,
        [Description("The value. Variable values are always text.")] string? value = null,
        [Description(CharacterDescription)] string character = "",
        [Description("The label shown in the Variables window in place of the key.")] string? label = null,
        [Description("Whether the variable is shown in the Variables window.")] bool? isVisible = null,
        [Description("The position in the Variables window, lowest first.")] int? displayOrder = null,
        [Description("The value's color in the Variables window as hex, such as #00FF00. Empty to clear.")] string? foregroundColor = null,
        [Description("The row's background color in the Variables window as hex. Empty to clear.")] string? backgroundColor = null,
        [Description("JavaScript that runs when the value changes; %0 is the key, %1 the old value and %2 the new value, escaped for use inside a string. Empty to clear.")] string? onChangeEvent = null,
        [Description("The package the variable belongs to.")] string? package = null,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var trimmedKey = key?.Trim() ?? "";
            if (trimmedKey.Length == 0 || trimmedKey.Any(char.IsWhiteSpace))
            {
                throw new McpException("The variable key must be a single word with no spaces.");
            }

            // Validate everything before changing anything.
            var foreground = foregroundColor is null ? null : McpContext.ParseBrush(foregroundColor);
            var background = backgroundColor is null ? null : McpContext.ParseBrush(backgroundColor);
            if (!string.IsNullOrWhiteSpace(onChangeEvent))
            {
                McpContext.RequireValidSyntax(onChangeEvent, hasPlaceholders: true);
            }

            var scope = character?.Trim() ?? "";
            var item = FindExact(game, trimmedKey, scope);
            var created = item is null;
            if (item is null)
            {
                item = new Variable { Key = trimmedKey, Character = scope };
                game.Variables.Add(item);
            }

            if (value is not null) item.Value = value;
            if (label is not null) item.Label = label;
            if (isVisible is { } visible) item.IsVisible = visible;
            if (displayOrder is { } order) item.DisplayOrder = order;
            if (foregroundColor is not null) item.ForegroundColor = foreground;
            if (backgroundColor is not null) item.BackgroundColor = background;
            if (onChangeEvent is not null) item.OnChangeEvent = string.IsNullOrWhiteSpace(onChangeEvent) ? null : onChangeEvent;
            if (package is not null) item.Package = package;
            return Result(game, item, created ? "created" : "updated");
        });

    /// <summary>
    /// Removes a variable.
    /// </summary>
    [McpServerTool(Name = "remove_variable", Destructive = true)]
    [Description("Removes a variable from a profile and saves the profile. This can't be undone, so confirm with the player first if it wasn't clearly requested.")]
    public static Task<string> RemoveVariable(
        [Description("The variable's key.")] string key,
        [Description("The character the variable belongs to. Empty for the shared variable.")] string character = "",
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var scope = character?.Trim() ?? "";
            var item = FindExact(game, key?.Trim() ?? "", scope)
                ?? throw new McpException(scope.Length == 0
                    ? $"There is no shared variable named \"{key}\". Pass its character to remove a character's variable."
                    : $"There is no variable named \"{key}\" for the character \"{scope}\".");
            game.Variables.Remove(item);
            return Result(game, item, "removed");
        });

    private static Variable? FindExact(Game game, string key, string character) =>
        game.Variables.FirstOrDefault(variable => variable.Key.Equals(key, StringComparison.OrdinalIgnoreCase)
            && string.Equals(variable.Character?.Trim() ?? "", character, StringComparison.OrdinalIgnoreCase));

    private static string Result(Game game, Variable variable, string action) => McpContext.Json(new
    {
        message = $"Variable \"{variable.Key}\"{(string.IsNullOrWhiteSpace(variable.Character) ? "" : $" for {variable.Character}")} {action} {(action == "removed" ? "from" : "in")} profile \"{game.Name}\".",
        warning = McpContext.Save(),
        variable = ToResult(variable)
    });

    private static object ToResult(Variable variable) => new
    {
        key = variable.Key,
        value = variable.Value,
        character = variable.Character,
        label = variable.Label,
        isVisible = variable.IsVisible,
        displayOrder = variable.DisplayOrder,
        foregroundColor = McpContext.FormatBrush(variable.ForegroundColor),
        backgroundColor = McpContext.FormatBrush(variable.BackgroundColor),
        onChangeEvent = variable.OnChangeEvent,
        package = variable.Package
    };
}
