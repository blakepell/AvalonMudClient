using System.ComponentModel;
using System.IO;
using System.Windows;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// MCP tools that describe the profiles and serve the scripting documentation.
/// </summary>
[McpServerToolType]
public sealed class ProfileTools
{
    /// <summary>
    /// The guide topics and the resource each is read from.
    /// </summary>
    private static readonly Dictionary<string, string> Topics = new(StringComparer.OrdinalIgnoreCase)
    {
        ["primer"] = "Mcp/ScriptingPrimer.md",
        ["avalon"] = "Docs/Scripting.md",
        ["aliases"] = "Docs/Aliases.md",
        ["triggers"] = "Docs/Triggers.md",
        ["variables"] = "Docs/Variables.md",
        ["gmcp"] = "Docs/GMCP.md",
        ["hash_commands"] = "Docs/HashCommands.md",
        ["colors"] = "Docs/Colors.md",
        ["language"] = "Docs/Scripting/README.md",
        ["getting_started"] = "Docs/Scripting/Getting-Started.md",
        ["variables_and_operators"] = "Docs/Scripting/Variables-and-Operators.md",
        ["control_flow"] = "Docs/Scripting/Control-Flow.md",
        ["functions"] = "Docs/Scripting/Functions.md",
        ["arrays_and_objects"] = "Docs/Scripting/Arrays-and-Objects.md",
        ["errors_and_async"] = "Docs/Scripting/Errors-and-Async.md",
        ["include"] = "Docs/Scripting/Include.md",
        ["linq"] = "Docs/Scripting/LINQ.md",
        ["compatibility"] = "Docs/Scripting/Compatibility.md",
        ["recipes"] = "Docs/Scripting/Recipes.md",
        ["application_objects"] = "Docs/Scripting/Application-Objects.md"
    };

    /// <summary>
    /// Lists profiles.
    /// </summary>
    [McpServerTool(Name = "list_profiles", ReadOnly = true, Idempotent = true)]
    [Description("Lists the saved profiles (games), which one is showing, and how many aliases, triggers, variables and scripts each has. " +
                 "Tools that take a profile use the showing profile when none is given.")]
    public static Task<string> ListProfiles() =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var active = McpContext.ActiveProfile;
            var profiles = McpContext.Profiles.Select(game => new
            {
                id = game.Id?.ToString("D"),
                name = game.Name,
                address = game.Address,
                active = ReferenceEquals(game, active),
                open = McpContext.IsOpen(game),
                character = game.Variables.FirstOrDefault(variable => variable.Key.Equals("Character", StringComparison.OrdinalIgnoreCase)
                                                                      && string.IsNullOrWhiteSpace(variable.Character))?.Value,
                aliasesEnabled = game.AliasesEnabled,
                triggersEnabled = game.TriggersEnabled,
                aliases = game.Aliases.Count,
                triggers = game.Triggers.Count(trigger => !trigger.Temp),
                variables = game.Variables.Count,
                scripts = game.Scripts.Count
            }).ToArray();
            return McpContext.Json(new { profiles });
        });

    /// <summary>
    /// Returns scripting documentation.
    /// </summary>
    [McpServerTool(Name = "get_scripting_guide", ReadOnly = true, Idempotent = true)]
    [Description("Returns Avalon's documentation for writing aliases, triggers, variables and JavaScript. Read \"primer\" before writing any script: " +
                 "it covers the language's differences from browser JavaScript, how scripts are scoped, placeholders and the most used objects. " +
                 "Other topics: avalon (every Avalon scripting object and function), aliases, triggers, variables, gmcp, hash_commands, colors, " +
                 "language, getting_started, variables_and_operators, control_flow, functions, arrays_and_objects, errors_and_async, include, " +
                 "linq, compatibility, recipes, application_objects.")]
    public static Task<string> GetScriptingGuide(
        [Description("The topic to read. Defaults to primer.")] string topic = "primer") =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var key = string.IsNullOrWhiteSpace(topic) ? "primer" : topic.Trim().Replace('-', '_').Replace(' ', '_');
            if (!Topics.TryGetValue(key, out var path))
            {
                throw new McpException($"There is no topic named \"{topic}\". Topics: {string.Join(", ", Topics.Keys)}.");
            }

            var info = Application.GetResourceStream(new Uri($"pack://application:,,,/{path}", UriKind.Absolute))
                       ?? throw new McpException($"The {key} guide could not be found.");
            using var reader = new StreamReader(info.Stream);
            return reader.ReadToEnd();
        });
}
