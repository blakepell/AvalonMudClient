using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalon.Models;
using Avalon.Scripting;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// MCP tools that list, read, add, update and remove a profile's scripts, and check JavaScript syntax.
/// </summary>
[McpServerToolType]
public sealed partial class ScriptTools
{
    private const string CodeDescription =
        "The JavaScript source. A script without a GMCP package runs in the profile's global scope when the profile opens " +
        "(and again after it changes), so the functions and variables it declares can be used by every alias, trigger and " +
        "other script. Keep it to declarations: top-level commands run every time the scripts reload.";

    private const string GmcpDescription =
        "A GMCP package such as \"Char.Vitals\". When set, the code runs in its own scope every time that package arrives, " +
        "with package and json (the message as a JSON string) defined. Leave empty for a script of shared functions.";

    /// <summary>
    /// Lists scripts.
    /// </summary>
    [McpServerTool(Name = "list_scripts", ReadOnly = true, Idempotent = true)]
    [Description("Lists the scripts in a profile with the functions each declares, without their code. Use get_script to read a script's code.")]
    public static Task<string> ListScripts(
        [Description(McpContext.ProfileDescription)] string? profile = null,
        [Description("Only return scripts whose name, GMCP package, description or package contains this text.")] string? search = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var scripts = game.Scripts.Where(script => McpContext.Matches(script, search)).Select(script => new
            {
                id = script.Id,
                name = script.Name,
                description = script.Description,
                gmcpPackage = string.IsNullOrWhiteSpace(script.GmcpPackage) ? null : script.GmcpPackage,
                package = script.Package,
                lines = script.Code.Split('\n').Length,
                functions = FunctionNames(script.Code)
            }).ToArray();
            return McpContext.Json(new { profile = game.Name, count = scripts.Length, scripts });
        });

    /// <summary>
    /// Gets a script's code.
    /// </summary>
    [McpServerTool(Name = "get_script", ReadOnly = true, Idempotent = true)]
    [Description("Gets a script, including its code.")]
    public static Task<string> GetScript(
        [Description("The script's ID, or its name when only one script has that name.")] string id,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            return McpContext.Json(ToResult(Find(game, id)));
        });

    /// <summary>
    /// Adds a script.
    /// </summary>
    [McpServerTool(Name = "add_script")]
    [Description("Adds a script to a profile and saves it. The code is syntax checked first. Read get_scripting_guide before writing scripts. " +
                 "Before adding a function, check list_scripts so you don't declare a function name that another script already uses.")]
    public static Task<string> AddScript(
        [Description("The script's name. It must be unique in the profile.")] string name,
        [Description(CodeDescription)] string code,
        [Description("What the script does.")] string description = "",
        [Description(GmcpDescription)] string gmcpPackage = "",
        [Description("The package the script belongs to, if any.")] string package = "",
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var trimmed = name?.Trim() ?? "";
            if (trimmed.Length == 0)
            {
                throw new McpException("The script needs a name.");
            }

            if (game.Scripts.Any(script => script.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                throw new McpException($"A script named \"{trimmed}\" already exists. Use update_script to change it, or choose another name.");
            }

            McpContext.RequireValidSyntax(code ?? "", hasPlaceholders: false);
            var item = new SourceCode
            {
                Name = trimmed,
                Code = code ?? "",
                Description = description ?? "",
                GmcpPackage = string.IsNullOrWhiteSpace(gmcpPackage) ? null : gmcpPackage.Trim(),
                Package = package ?? ""
            };

            game.Scripts.Add(item);
            return Result(game, item, "added");
        });

    /// <summary>
    /// Updates a script.
    /// </summary>
    [McpServerTool(Name = "update_script", Idempotent = true)]
    [Description("Changes an existing script and saves it. Only the values you pass are changed. The code replaces the whole script, so read it with get_script first.")]
    public static Task<string> UpdateScript(
        [Description("The script's ID, or its name when only one script has that name.")] string id,
        [Description("A new name for the script.")] string? name = null,
        [Description(CodeDescription)] string? code = null,
        [Description("What the script does.")] string? description = null,
        [Description(GmcpDescription + " Pass an empty string to clear it.")] string? gmcpPackage = null,
        [Description("The package the script belongs to.")] string? package = null,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);
            var newName = name?.Trim();
            if (newName is { Length: 0 })
            {
                throw new McpException("The script needs a name.");
            }

            if (newName is not null && game.Scripts.Any(script => !ReferenceEquals(script, item) && script.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new McpException($"A script named \"{newName}\" already exists.");
            }

            if (code is not null)
            {
                McpContext.RequireValidSyntax(code, hasPlaceholders: false);
            }

            if (newName is not null) item.Name = newName;
            if (code is not null) item.Code = code;
            if (description is not null) item.Description = description;
            if (gmcpPackage is not null) item.GmcpPackage = string.IsNullOrWhiteSpace(gmcpPackage) ? null : gmcpPackage.Trim();
            if (package is not null) item.Package = package;
            return Result(game, item, "updated");
        });

    /// <summary>
    /// Removes a script.
    /// </summary>
    [McpServerTool(Name = "remove_script", Destructive = true)]
    [Description("Removes a script from a profile and saves the profile. Functions it declared stay defined in an open terminal until the profile is reopened. " +
                 "This can't be undone, so confirm with the player first if it wasn't clearly requested.")]
    public static Task<string> RemoveScript(
        [Description("The script's ID, or its name when only one script has that name.")] string id,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);
            game.Scripts.Remove(item);
            return McpContext.Json(new
            {
                message = $"Script \"{item.Name}\" removed from profile \"{game.Name}\".",
                warning = McpContext.Save(),
                id = item.Id
            });
        });

    /// <summary>
    /// Checks JavaScript syntax.
    /// </summary>
    [McpServerTool(Name = "validate_javascript", ReadOnly = true, Idempotent = true)]
    [Description("Checks that JavaScript parses with Avalon's script engine, without saving or running it. Syntax only: it can't tell whether the objects and members you call exist, so check those against get_scripting_guide.")]
    public static Task<string> ValidateJavaScript(
        [Description("The code to check.")] string code,
        [Description("Where the code will run: \"script\" (the Scripts list), \"alias\" or \"trigger\" (placeholders such as %1 are allowed), \"variable\" (an On Change Event), or \"line_transformer\" (a JavaScript line transformer trigger, which is an expression).")]
        string usage = "script") =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var kind = usage?.Trim().ToLowerInvariant() ?? "script";
            var error = kind switch
            {
                "script" or "gmcp" or "event" => McpContext.CheckSyntax(code ?? "", hasPlaceholders: false),
                "alias" or "trigger" or "variable" => McpContext.CheckSyntax(code ?? "", hasPlaceholders: true),
                "line_transformer" or "expression" => McpContext.CheckSyntax(code ?? "", hasPlaceholders: true, expression: true),
                _ => throw new McpException($"\"{usage}\" is not a valid usage. Use script, alias, trigger, variable or line_transformer.")
            };
            return McpContext.Json(error is null ? new { valid = true, error = (string?)null } : new { valid = false, error = (string?)error });
        });

    private static SourceCode Find(Game game, string id) =>
        McpContext.FindItem(game.Scripts, id, script => script.Id, script => script.Name, "script");

    private static string[] FunctionNames(string code) =>
        FunctionPattern().Matches(code).Select(match => match.Groups[1].Value).Distinct().ToArray();

    private static string Result(Game game, SourceCode script, string action) => McpContext.Json(new
    {
        message = $"Script \"{script.Name}\" {action} in profile \"{game.Name}\". " +
                  (string.IsNullOrWhiteSpace(script.GmcpPackage)
                      ? "An open terminal reloads its scripts before the next alias, trigger or event runs."
                      : $"It runs the next time the {script.GmcpPackage} GMCP package arrives."),
        warning = McpContext.Save(),
        script = ToResult(script)
    });

    private static object ToResult(SourceCode script) => new
    {
        id = script.Id,
        name = script.Name,
        description = script.Description,
        gmcpPackage = string.IsNullOrWhiteSpace(script.GmcpPackage) ? null : script.GmcpPackage,
        package = script.Package,
        functions = FunctionNames(script.Code),
        code = script.Code
    };

    [GeneratedRegex(@"(?:^|[\s;{}])(?:async\s+)?function\s+([A-Za-z_$][\w$]*)\s*\(", RegexOptions.Multiline)]
    private static partial Regex FunctionPattern();
}
