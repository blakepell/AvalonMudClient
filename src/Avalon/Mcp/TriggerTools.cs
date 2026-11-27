using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalon.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Trigger = Avalon.Models.Trigger;

namespace Avalon.Mcp;

/// <summary>
/// MCP tools that list, add, update, remove and test a profile's triggers.
/// </summary>
[McpServerToolType]
public sealed class TriggerTools
{
    private const string PatternDescription =
        "A .NET regular expression matched against each line from the game with color codes removed, such as " +
        "\"^(\\w+) tells you '(.*)'$\". Named groups such as (?<Target>\\w+) also set the variable of the same name; .NET numbers named groups after the unnamed ones.";

    private const string CommandDescription =
        "What runs when the pattern matches. %0 is the whole line, %1..%9 are the pattern's groups, %Line is the line " +
        "and %LineRaw the line with color codes. For Command: game commands separated by ';'. For JavaScript: script code; " +
        "the placeholders are escaped for use inside a JavaScript string, so quote them (\"%1\"). When line_transformer is " +
        "true, the command (or the JavaScript expression's string value) replaces the matched text instead of running.";

    private const string ExecuteAsDescription = "\"Command\" to send the text as game commands, or \"JavaScript\" to run it as a script.";

    /// <summary>
    /// Lists triggers.
    /// </summary>
    [McpServerTool(Name = "list_triggers", ReadOnly = true, Idempotent = true)]
    [Description("Lists the triggers in a profile. Triggers run a command or script when a line from the game matches their regular expression.")]
    public static Task<string> ListTriggers(
        [Description(McpContext.ProfileDescription)] string? profile = null,
        [Description("Only return triggers whose name, pattern, command, group, character or package contains this text.")] string? search = null,
        [Description("Only return triggers in this group.")] string? group = null,
        [Description("Include temporary triggers created by scripts.")] bool includeTemp = false) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var triggers = game.Triggers
                .Where(trigger => (includeTemp || !trigger.Temp) && McpContext.Matches(trigger, search, group, trigger.Group))
                .Select(ToResult).ToArray();
            return McpContext.Json(new { profile = game.Name, count = triggers.Length, triggers });
        });

    /// <summary>
    /// Adds a trigger.
    /// </summary>
    [McpServerTool(Name = "add_trigger")]
    [Description("Adds a trigger to a profile and saves it. The pattern is checked and JavaScript is syntax checked first. Use test_trigger_pattern to try a pattern on sample lines, and read get_scripting_guide before writing JavaScript.")]
    public static Task<string> AddTrigger(
        [Description(PatternDescription)] string pattern,
        [Description(CommandDescription)] string command = "",
        [Description("A friendly name for the trigger.")] string name = "",
        [Description(ExecuteAsDescription)] string executeAs = "Command",
        [Description("A group name, used to enable and disable related aliases and triggers together.")] string group = "",
        [Description("Only fire when this character is playing (the Character variable). Empty for every character.")] string character = "",
        [Description("The order triggers are checked in, lowest first.")] int priority = 10000,
        [Description("Whether the trigger is enabled.")] bool enabled = true,
        [Description("Don't echo the commands the trigger sends.")] bool isSilent = false,
        [Description("Hide the matching line from the terminal.")] bool gag = false,
        [Description("Stop checking other triggers against the line after this one matches.")] bool stopProcessing = false,
        [Description("Disable the trigger after it fires once.")] bool disableAfterTriggered = false,
        [Description("Replace @Variable references in the pattern with their values before matching.")] bool variableReplacement = false,
        [Description("Replace the matched text in the terminal with the command instead of running it.")] bool lineTransformer = false,
        [Description("A temporary trigger that is removed after it fires and is never saved.")] bool temp = false,
        [Description("The package the trigger belongs to, if any.")] string package = "",
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            McpContext.ValidatePattern(pattern);
            var type = McpContext.ParseExecuteAs(executeAs);
            CheckCommand(command ?? "", type, lineTransformer);
            var item = new Trigger
            {
                Pattern = pattern,
                Command = command ?? "",
                Name = name ?? "",
                ExecuteAs = type,
                Group = group ?? "",
                Character = character ?? "",
                Priority = priority,
                Enabled = enabled,
                IsSilent = isSilent,
                Gag = gag,
                StopProcessing = stopProcessing,
                DisableAfterTriggered = disableAfterTriggered,
                VariableReplacement = variableReplacement,
                LineTransformer = lineTransformer,
                Temp = temp,
                Package = package ?? ""
            };

            game.Triggers.Add(item);
            return Result(game, item, "added");
        });

    /// <summary>
    /// Updates a trigger.
    /// </summary>
    [McpServerTool(Name = "update_trigger", Idempotent = true)]
    [Description("Changes an existing trigger and saves it. Only the values you pass are changed.")]
    public static Task<string> UpdateTrigger(
        [Description("The trigger's ID, or its name when only one trigger has that name.")] string id,
        [Description(PatternDescription)] string? pattern = null,
        [Description(CommandDescription)] string? command = null,
        [Description("A friendly name for the trigger.")] string? name = null,
        [Description(ExecuteAsDescription)] string? executeAs = null,
        [Description("The group name. Pass an empty string to clear it.")] string? group = null,
        [Description("The character the trigger is limited to. Pass an empty string for every character.")] string? character = null,
        [Description("The order triggers are checked in, lowest first.")] int? priority = null,
        [Description("Whether the trigger is enabled.")] bool? enabled = null,
        [Description("Don't echo the commands the trigger sends.")] bool? isSilent = null,
        [Description("Hide the matching line from the terminal.")] bool? gag = null,
        [Description("Stop checking other triggers after this one matches.")] bool? stopProcessing = null,
        [Description("Disable the trigger after it fires once.")] bool? disableAfterTriggered = null,
        [Description("Replace @Variable references in the pattern before matching.")] bool? variableReplacement = null,
        [Description("Replace the matched text with the command instead of running it.")] bool? lineTransformer = null,
        [Description("A temporary trigger that is removed after it fires and is never saved.")] bool? temp = null,
        [Description("The package the trigger belongs to.")] string? package = null,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);

            // Validate everything before changing anything so a bad value leaves the trigger as it was.
            if (pattern is not null)
            {
                McpContext.ValidatePattern(pattern);
            }

            var type = executeAs is null ? item.ExecuteAs : McpContext.ParseExecuteAs(executeAs);
            CheckCommand(command ?? item.Command, type, lineTransformer ?? item.LineTransformer);

            if (pattern is not null) item.Pattern = pattern;
            if (command is not null) item.Command = command;
            if (name is not null) item.Name = name;
            item.ExecuteAs = type;
            if (group is not null) item.Group = group;
            if (character is not null) item.Character = character;
            if (priority is { } order) item.Priority = order;
            if (enabled is { } on) item.Enabled = on;
            if (isSilent is { } silent) item.IsSilent = silent;
            if (gag is { } hide) item.Gag = hide;
            if (stopProcessing is { } stop) item.StopProcessing = stop;
            if (disableAfterTriggered is { } once) item.DisableAfterTriggered = once;
            if (variableReplacement is { } replace) item.VariableReplacement = replace;
            if (lineTransformer is { } transform) item.LineTransformer = transform;
            if (temp is { } isTemp) item.Temp = isTemp;
            if (package is not null) item.Package = package;
            return Result(game, item, "updated");
        });

    /// <summary>
    /// Removes a trigger.
    /// </summary>
    [McpServerTool(Name = "remove_trigger", Destructive = true)]
    [Description("Removes a trigger from a profile and saves the profile. This can't be undone, so confirm with the player first if it wasn't clearly requested.")]
    public static Task<string> RemoveTrigger(
        [Description("The trigger's ID, or its name when only one trigger has that name.")] string id,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var game = McpContext.ResolveProfile(profile);
            var item = Find(game, id);
            game.Triggers.Remove(item);
            return Result(game, item, "removed");
        });

    /// <summary>
    /// Tests a pattern against sample lines.
    /// </summary>
    [McpServerTool(Name = "test_trigger_pattern", ReadOnly = true, Idempotent = true)]
    [Description("Tests a trigger pattern against sample lines of game output without changing anything, showing which lines match and what %0..%9 and named groups would hold.")]
    public static Task<string> TestTriggerPattern(
        [Description("The .NET regular expression to test.")] string pattern,
        [Description("Sample lines from the game, as they appear without color codes.")] string[] lines)
    {
        McpContext.ValidatePattern(pattern);
        var regex = new Regex(pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
        var results = (lines ?? []).Select(line =>
        {
            var match = regex.Match(line);
            if (!match.Success)
            {
                return (object)new { line, matched = false };
            }

            // The interpreter numbers placeholders by group position, and .NET puts named groups after the
            // unnamed ones, so a named group also has a %n.
            var groups = match.Groups.Cast<Group>().ToArray();
            return new
            {
                line,
                matched = true,
                placeholders = groups.Select((group, index) => (group, index))
                    .ToDictionary(item => $"%{item.index}", item => item.group.Value),
                variables = groups.Where(group => !char.IsAsciiDigit(group.Name[0]) && !string.IsNullOrWhiteSpace(group.Value))
                    .ToDictionary(group => group.Name, group => group.Value)
            };
        }).ToArray();

        return Task.FromResult(McpContext.Json(new { pattern, results }));
    }

    private static Trigger Find(Game game, string id) =>
        McpContext.FindItem(game.Triggers, id, trigger => trigger.Id, trigger => trigger.Name, "trigger");

    private static void CheckCommand(string command, ExecuteType type, bool lineTransformer)
    {
        if (type == ExecuteType.JavaScript)
        {
            // A JavaScript line transformer is evaluated as an expression whose string value replaces the match.
            McpContext.RequireValidSyntax(command, hasPlaceholders: true, expression: lineTransformer);
        }
    }

    private static string Result(Game game, Trigger trigger, string action) => McpContext.Json(new
    {
        message = $"Trigger \"{(string.IsNullOrWhiteSpace(trigger.Name) ? trigger.Pattern : trigger.Name)}\" {action} {(action == "removed" ? "from" : "in")} profile \"{game.Name}\".",
        warning = McpContext.Save(),
        trigger = ToResult(trigger)
    });

    private static object ToResult(Trigger trigger) => new
    {
        id = trigger.Id,
        name = trigger.Name,
        pattern = trigger.Pattern,
        command = trigger.Command,
        executeAs = trigger.ExecuteAs.ToString(),
        group = trigger.Group,
        character = trigger.Character,
        priority = trigger.Priority,
        enabled = trigger.Enabled,
        isSilent = trigger.IsSilent,
        gag = trigger.Gag,
        stopProcessing = trigger.StopProcessing,
        disableAfterTriggered = trigger.DisableAfterTriggered,
        variableReplacement = trigger.VariableReplacement,
        lineTransformer = trigger.LineTransformer,
        temp = trigger.Temp,
        package = trigger.Package,
        count = trigger.Count
    };
}
