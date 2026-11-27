using System.ComponentModel;
using Avalon.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// MCP tools that list, add, update and remove a profile's aliases.
/// </summary>
[McpServerToolType]
public sealed class AliasTools
{
    private const string CommandDescription =
        "What the alias runs. For Command: one or more game commands separated by ';', with @Variable references and #hash commands. " +
        "For JavaScript: script code. %1..%9 are the words typed after the alias and %0 is everything typed after it; " +
        "if a Command alias has no placeholders, the words typed after it are appended to the command.";

    private const string ExecuteAsDescription = "\"Command\" to send the text as game commands, or \"JavaScript\" to run it as a script.";

    /// <summary>
    /// Lists aliases.
    /// </summary>
    [McpServerTool(Name = "list_aliases", ReadOnly = true, Idempotent = true)]
    [Description("Lists the aliases in a profile. Aliases expand what the player types: when the first word typed matches an alias, the alias's command or script runs instead.")]
    public static Task<string> ListAliases(
        [Description(McpContext.ProfileDescription)] string? profile = null,
        [Description("Only return aliases whose name, command, character, group or package contains this text.")] string? search = null,
        [Description("Only return aliases in this group.")] string? group = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var aliases = game.Aliases.Where(alias => McpContext.Matches(alias, search, group, alias.Group)).Select(ToResult).ToArray();
            return McpContext.Json(new { profile = game.Name, count = aliases.Length, aliases });
        });

    /// <summary>
    /// Adds an alias.
    /// </summary>
    [McpServerTool(Name = "add_alias")]
    [Description("Adds an alias to a profile and saves it. JavaScript commands are syntax checked first. Read the scripting guide (get_scripting_guide) before writing JavaScript.")]
    public static Task<string> AddAlias(
        [Description("The word the player types to run the alias, such as \"gg\". It can't contain spaces.")] string alias,
        [Description(CommandDescription)] string command,
        [Description(ExecuteAsDescription)] string executeAs = "Command",
        [Description("A group name, used to enable and disable related aliases and triggers together.")] string group = "",
        [Description("Only run the alias when this character is playing (the Character variable). Empty for every character.")] string character = "",
        [Description("Whether the alias is enabled.")] bool enabled = true,
        [Description("Orders aliases with the same name and the buttons in the Alias Button Actions window.")] int sortOrder = 0,
        [Description("Whether the alias has a button in the Alias Button Actions window.")] bool buttonVisible = false,
        [Description("The label on the alias's button.")] string buttonLabel = "",
        [Description("The package the alias belongs to, if any.")] string package = "",
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = new Alias
            {
                AliasExpression = ValidateExpression(alias),
                Command = command ?? "",
                ExecuteAs = McpContext.ParseExecuteAs(executeAs),
                Group = group ?? "",
                Character = character ?? "",
                Enabled = enabled,
                SortOrder = sortOrder,
                ButtonVisible = buttonVisible,
                ButtonLabel = buttonLabel ?? "",
                Package = package ?? ""
            };

            CheckCommand(item);
            game.Aliases.Add(item);
            return Result(game, item, "added");
        });

    /// <summary>
    /// Updates an alias.
    /// </summary>
    [McpServerTool(Name = "update_alias", Idempotent = true)]
    [Description("Changes an existing alias and saves it. Only the values you pass are changed.")]
    public static Task<string> UpdateAlias(
        [Description("The alias's ID, or its name when only one alias has that name.")] string id,
        [Description("A new name for the alias.")] string? alias = null,
        [Description(CommandDescription)] string? command = null,
        [Description(ExecuteAsDescription)] string? executeAs = null,
        [Description("The group name. Pass an empty string to clear it.")] string? group = null,
        [Description("The character the alias is limited to. Pass an empty string for every character.")] string? character = null,
        [Description("Whether the alias is enabled.")] bool? enabled = null,
        [Description("The sort order.")] int? sortOrder = null,
        [Description("Whether the alias has a button in the Alias Button Actions window.")] bool? buttonVisible = null,
        [Description("The label on the alias's button.")] string? buttonLabel = null,
        [Description("The package the alias belongs to.")] string? package = null,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);

            // Validate everything before changing anything so a bad value leaves the alias as it was.
            var newExpression = alias is null ? null : ValidateExpression(alias);
            var newExecuteAs = executeAs is null ? (ExecuteType?)null : McpContext.ParseExecuteAs(executeAs);
            if ((newExecuteAs ?? item.ExecuteAs) == ExecuteType.JavaScript)
            {
                McpContext.RequireValidSyntax(command ?? item.Command, hasPlaceholders: true);
            }

            if (newExpression is not null) item.AliasExpression = newExpression;
            if (command is not null) item.Command = command;
            if (newExecuteAs is { } type) item.ExecuteAs = type;
            if (group is not null) item.Group = group;
            if (character is not null) item.Character = character;
            if (enabled is { } on) item.Enabled = on;
            if (sortOrder is { } order) item.SortOrder = order;
            if (buttonVisible is { } visible) item.ButtonVisible = visible;
            if (buttonLabel is not null) item.ButtonLabel = buttonLabel;
            if (package is not null) item.Package = package;
            return Result(game, item, "updated");
        });

    /// <summary>
    /// Removes an alias.
    /// </summary>
    [McpServerTool(Name = "remove_alias", Destructive = true)]
    [Description("Removes an alias from a profile and saves the profile. This can't be undone, so confirm with the player first if it wasn't clearly requested.")]
    public static Task<string> RemoveAlias(
        [Description("The alias's ID, or its name when only one alias has that name.")] string id,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);
            game.Aliases.Remove(item);
            return Result(game, item, "removed");
        });

    private static Alias Find(Game game, string id) =>
        McpContext.FindItem(game.Aliases, id, alias => alias.Id, alias => alias.AliasExpression, "alias");

    private static string ValidateExpression(string expression)
    {
        var trimmed = expression?.Trim() ?? "";
        if (trimmed.Length == 0 || trimmed.Any(char.IsWhiteSpace))
        {
            throw new McpException("The alias must be a single word with no spaces.");
        }

        return trimmed;
    }

    private static void CheckCommand(Alias alias)
    {
        if (alias.ExecuteAs == ExecuteType.JavaScript)
        {
            McpContext.RequireValidSyntax(alias.Command, hasPlaceholders: true);
        }
    }

    private static string Result(Game game, Alias alias, string action) => McpContext.Json(new
    {
        message = $"Alias \"{alias.AliasExpression}\" {action} {(action == "removed" ? "from" : "in")} profile \"{game.Name}\".",
        warning = McpContext.Save(),
        alias = ToResult(alias)
    });

    private static object ToResult(Alias alias) => new
    {
        id = alias.Id,
        alias = alias.AliasExpression,
        command = alias.Command,
        executeAs = alias.ExecuteAs.ToString(),
        group = alias.Group,
        character = alias.Character,
        enabled = alias.Enabled,
        sortOrder = alias.SortOrder,
        buttonVisible = alias.ButtonVisible,
        buttonLabel = alias.ButtonLabel,
        package = alias.Package,
        count = alias.Count
    };
}
