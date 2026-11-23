using System.Diagnostics;
using System.IO;
using System.Text;
using Avalon.Models;
using Avalon.Scripting;
using Avalon.ViewModels;

namespace Avalon.HashCommands;

// The generic Continuum hash commands.  Each is small, so they live together; DSL specific commands are not ported.

/// <summary>#echo: writes text to the terminal.</summary>
public sealed class EchoCommand : HashCommand
{
    public override string Name => "#echo";
    public override string Description => "Echoes text (ANSI codes allowed) to the terminal.  -d prefixes a timestamp, -t <name> writes to a named terminal window.";
    public override string Usage => "#echo [-d] [-t <terminal>] <text>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var text = context.Arguments;
        bool timestamp = false;
        string? target = null;
        while (true)
        {
            if (text.StartsWith("-d ", StringComparison.Ordinal) || text.StartsWith("--datetime ", StringComparison.Ordinal))
            {
                timestamp = true;
                text = Terminal.Interpreter.SplitFirst(text).Tail;
            }
            else if (text.StartsWith("-t ", StringComparison.Ordinal) || text.StartsWith("--target ", StringComparison.Ordinal))
            {
                (target, text) = Terminal.Interpreter.SplitFirst(Terminal.Interpreter.SplitFirst(text).Tail);
            }
            else
            {
                break;
            }
        }

        if (timestamp)
        {
            text = $"[{DateTime.Now:HH:mm:ss}] {text}";
        }

        if (target is null)
        {
            context.Echo(text);
        }
        else
        {
            Controls.TerminalWindow.With(target, t => t.AppendLine(text));
        }

        return Task.CompletedTask;
    }
}

/// <summary>#send: sends text to the game without alias or hash command processing.</summary>
public sealed class SendCommand : HashCommand
{
    public override string Name => "#send";
    public override string Description => "Sends text to the game exactly as typed.";
    public override string Usage => "#send <text>";
    public override Task ExecuteAsync(HashCommandContext context) => context.Interpreter.SendLineAsync(context.RawArguments, context.Token);
}

/// <summary>#clear: clears the terminal.</summary>
public sealed class ClearCommand : HashCommand
{
    public override string Name => "#clear";
    public override string Description => "Clears the terminal.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Interpreter.Clear();
        return Task.CompletedTask;
    }
}

/// <summary>#line-count: shows the terminal's line count.</summary>
public sealed class LineCountCommand : HashCommand
{
    public override string Name => "#line-count";
    public override string Description => "Echoes the line count in the main terminal window.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Info($"{context.Interpreter.Host.LineCount:N0} lines");
        return Task.CompletedTask;
    }
}

/// <summary>#list: lists variables, aliases, triggers, directions, macros or scripts.</summary>
public sealed class ListCommand : HashCommand
{
    public override string Name => "#list";
    public override string Description => "Lists vars, aliases, triggers, directions, macros or scripts.";
    public override string Usage => "#list <vars|aliases|triggers|directions|macros|scripts>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var game = context.Game;
        IEnumerable<string>? lines = context.Arguments.Trim().ToLowerInvariant() switch
        {
            "vars" or "variables" => game.Variables.OrderBy(v => v.Key).Select(v => $"{v.Key,-25} {v.Value}{(v.Character.Length > 0 ? $"  ({v.Character})" : "")}"),
            "aliases" => game.Aliases.OrderBy(a => a.AliasExpression).Select(a => $"{a.AliasExpression,-20} {(a.Enabled ? "" : "[disabled] ")}{a.Command}"),
            "triggers" => game.Triggers.OrderBy(t => t.Priority).Select(t => $"{(t.Enabled ? " " : "x")} {t.Pattern,-40} {t.Command}"),
            "directions" => game.Directions.OrderBy(d => d.Name).Select(d => $"{d.Name,-25} {d.Speedwalk}"),
            "macros" => game.Macros.Select(m => $"{m.KeyCombination,-20} {m.Command}"),
            "scripts" => game.Scripts.OrderBy(s => s.Name).Select(s => $"{s.Name,-30} {s.Description}"),
            _ => null
        };

        if (lines is null)
        {
            context.Error($"Usage: {Usage}");
            return Task.CompletedTask;
        }

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line);
        }

        context.Interpreter.Echo(sb.Length == 0 ? "(none)\n" : sb.ToString());
        return Task.CompletedTask;
    }
}

/// <summary>#cmd-list: lists the hash commands.</summary>
public sealed class CmdListCommand : HashCommand
{
    public override string Name => "#cmd-list";
    public override string Description => "Echoes all of the hash commands that are available for use.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var sb = new StringBuilder();
        foreach (var command in HashCommandRegistry.All)
        {
            sb.AppendLine($"\u001b[93m{command.Name,-18}\u001b[0m {command.Description}");
        }

        context.Interpreter.Echo(sb.ToString());
        return Task.CompletedTask;
    }
}

/// <summary>#version: shows the client version.</summary>
public sealed class VersionCommand : HashCommand
{
    public override string Name => "#version";
    public override string Description => "Echoes the client version.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Info($"Avalon Mud Client {typeof(App).Assembly.GetName().Version}");
        return Task.CompletedTask;
    }
}

/// <summary>#recent-triggers: shows the triggers that matched most recently.</summary>
public sealed class RecentTriggersCommand : HashCommand
{
    public override string Name => "#recent-triggers";
    public override string Description => "Shows the most recent triggers that have matched successfully.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var sb = new StringBuilder();
        foreach (var (time, trigger) in context.Interpreter.RecentTriggers)
        {
            sb.AppendLine($"{time:HH:mm:ss}  {(string.IsNullOrEmpty(trigger.Name) ? "(No Name)" : trigger.Name),-25} {trigger.Pattern}");
        }

        context.Interpreter.Echo(sb.Length == 0 ? "No triggers have matched yet.\n" : sb.ToString());
        return Task.CompletedTask;
    }
}

/// <summary>#globals: lists the script globals.</summary>
public sealed class GlobalsCommand : HashCommand
{
    public override string Name => "#globals";
    public override string Description => "Lists the objects and types available to scripts.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var sb = new StringBuilder();
        foreach (var registration in context.Interpreter.Environment.Registrations.Values.OrderBy(r => r.Alias))
        {
            sb.AppendLine($"{registration.Alias,-20} {(registration.IsType ? "type" : "object")}  {registration.Type.FullName}");
        }

        context.Interpreter.Echo(sb.ToString());
        return Task.CompletedTask;
    }
}

/// <summary>#guid: echoes a new GUID.</summary>
public sealed class GuidCommand : HashCommand
{
    public override string Name => "#guid";
    public override string Description => "Echoes a new GUID.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Echo(Guid.NewGuid().ToString());
        return Task.CompletedTask;
    }
}

/// <summary>#set: sets a variable.</summary>
public sealed class SetCommand : HashCommand
{
    public override string Name => "#set";
    public override string Description => "Sets a variable.";
    public override string Usage => "#set <variable> <value>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var (key, value) = Terminal.Interpreter.SplitFirst(context.Arguments);
        if (key.Length == 0)
        {
            context.Error($"Usage: {Usage}");
            return Task.CompletedTask;
        }
        return context.Interpreter.SetVariableAsync(key, value, context.Silent, context.Depth, context.Token);
    }
}

/// <summary>#get: echoes a variable.</summary>
public sealed class GetCommand : HashCommand
{
    public override string Name => "#get";
    public override string Description => "Echoes the value of a variable.";
    public override string Usage => "#get <variable>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Echo(context.Interpreter.GetVariable(context.Arguments.Trim()));
        return Task.CompletedTask;
    }
}

/// <summary>#var: works with variables (set, add, subtract, increment, decrement, echo, delete).</summary>
public sealed class VarCommand : HashCommand
{
    public override string Name => "#var";
    public override string Description => "Allows interaction with variables: -s value, -a n, -m n, -i, -d, -e, --delete.";
    public override string Usage => "#var <name> [-s|--set value] [-a|--add n] [-m|--subtract n] [-i|--increment] [-d|--decrement] [-e|--echo] [--delete]";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        var args = context.Parse();
        if (args.Values.Count == 0)
        {
            context.Error($"Usage: {Usage}");
            return;
        }

        string name = args.Values[0];
        var interpreter = context.Interpreter;
        string current = interpreter.GetVariable(name);
        int.TryParse(current, out int number);

        if (args.Get("a", "add") is { } add && int.TryParse(add, out int a))
        {
            await Set((number += a).ToString());
        }

        if (args.Get("m", "subtract") is { } sub && int.TryParse(sub, out int m))
        {
            await Set((number -= m).ToString());
        }

        if (args.Has("i", "increment"))
        {
            await Set((++number).ToString());
        }

        if (args.Has("d", "decrement"))
        {
            await Set((--number).ToString());
        }

        if (args.Get("s", "set") is { } set)
        {
            await Set(set);
        }

        if (args.Has("e", "echo"))
        {
            context.Echo(interpreter.GetVariable(name));
        }

        if (args.Has("delete"))
        {
            interpreter.RemoveVariable(name);
        }

        Task Set(string value) => interpreter.SetVariableAsync(name, value, context.Silent, context.Depth, context.Token);
    }
}

/// <summary>#enable: enables all aliases or triggers.</summary>
public sealed class EnableCommand : HashCommand
{
    public override string Name => "#enable";
    public override string Description => "Enables all triggers or aliases: #enable triggers | aliases | gmcp.";
    public override Task ExecuteAsync(HashCommandContext context) => Toggle(context, true);

    internal static Task Toggle(HashCommandContext context, bool enabled)
    {
        switch (context.Arguments.Trim().ToLowerInvariant())
        {
            case "triggers":
                context.Game.TriggersEnabled = enabled;
                break;
            case "aliases":
                context.Game.AliasesEnabled = enabled;
                break;
            case "gmcp":
                context.Game.GmcpEnabled = enabled;
                break;
            default:
                context.Error("Specify triggers, aliases or gmcp.");
                return Task.CompletedTask;
        }
        context.Info($"{context.Arguments.Trim()} {(enabled ? "enabled" : "disabled")}.");
        return Task.CompletedTask;
    }
}

/// <summary>#disable: disables all aliases or triggers.</summary>
public sealed class DisableCommand : HashCommand
{
    public override string Name => "#disable";
    public override string Description => "Disables all triggers or aliases: #disable triggers | aliases | gmcp.";
    public override Task ExecuteAsync(HashCommandContext context) => EnableCommand.Toggle(context, false);
}

/// <summary>#group-enable: enables the aliases and triggers in a group.</summary>
public sealed class GroupEnableCommand : HashCommand
{
    public override string Name => "#group-enable";
    public override string Description => "Enables the aliases and triggers in a group.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        int count = TerminalScriptCommands.SetGroupEnabled(context.Game, context.Arguments.Trim(), true);
        context.Info($"{count} item(s) enabled in group '{context.Arguments.Trim()}'.");
        return Task.CompletedTask;
    }
}

/// <summary>#group-disable: disables the aliases and triggers in a group.</summary>
public sealed class GroupDisableCommand : HashCommand
{
    public override string Name => "#group-disable";
    public override string Description => "Disables the aliases and triggers in a group.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        int count = TerminalScriptCommands.SetGroupEnabled(context.Game, context.Arguments.Trim(), false);
        context.Info($"{count} item(s) disabled in group '{context.Arguments.Trim()}'.");
        return Task.CompletedTask;
    }
}

/// <summary>#edit: opens the editor for an alias, trigger or script.</summary>
public sealed class EditCommand : HashCommand
{
    public override string Name => "#edit";
    public override string Description => "Edits an alias, trigger or script by its name, alias expression or ID.";
    public override string Usage => "#edit <alias | trigger name | script name | id>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        string key = context.Arguments.Trim();
        var game = context.Game;
        object? item = (object?)game.Scripts.FirstOrDefault(s => s.Name.Equals(key, StringComparison.OrdinalIgnoreCase) || s.Id == key)
            ?? (object?)game.Triggers.FirstOrDefault(t => t.Name.Equals(key, StringComparison.OrdinalIgnoreCase) || t.Id == key)
            ?? game.Aliases.FirstOrDefault(a => a.AliasExpression.Equals(key, StringComparison.OrdinalIgnoreCase) || a.Id == key);

        if (item is null)
        {
            context.Error($"Nothing named '{key}' was found.");
        }
        else
        {
            AppCommands.EditItem(game, item);
        }

        return Task.CompletedTask;
    }
}

/// <summary>#script: calls a script function.</summary>
public sealed class ScriptCommand : HashCommand
{
    public override string Name => "#script";
    public override string Description => "Calls a script function: #script <function> [args...].  -r resets the scripting environment.";
    public override string Usage => "#script <function> [args...] | #script -r";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        var args = context.Parse();
        if (args.Has("r", "reset"))
        {
            context.Interpreter.RefreshScripts();
            context.Info("The scripting environment has been refreshed.");
            return;
        }
        if (args.Values.Count == 0)
        {
            context.Error($"Usage: {Usage}");
            return;
        }

        var result = await context.Interpreter.InvokeNestedAsync(args.Values[0], args.Values.Skip(1).Cast<object>().ToArray(),
            context.Silent, context.Depth, context.Token);
        if (result is not null and not Mosaic.UI.Wpf.Scripting.Undefined)
        {
            context.Echo(result.ToString() ?? "");
        }
    }
}

/// <summary>#dir: shows the directions dialog.</summary>
public sealed class DirCommand : HashCommand
{
    public override string Name => "#dir";
    public override string Description => "Shows the quick directions window.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        AppCommands.ShowDirectionsDialog();
        return Task.CompletedTask;
    }
}

/// <summary>#walk: speed walks a path.</summary>
public sealed class WalkCommand : HashCommand
{
    public override string Name => "#walk";
    public override string Description => "Speed walks a set of directions given in the speed walk format, such as 2n e (open door) 3w.";
    public override string Usage => "#walk <speedwalk>";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        foreach (var step in Direction.Expand(context.Arguments))
        {
            await context.SendAsync(step);
        }
    }
}

/// <summary>#go: walks a saved direction, chaining two paths when needed.</summary>
public sealed class GoCommand : HashCommand
{
    public override string Name => "#go";
    public override string Description => "Executes a saved direction by name, starting from the Room variable when it is set.";
    public override string Usage => "#go <direction name>";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        string name = context.Arguments.Trim();
        string room = context.Interpreter.GetVariable("Room");
        var directions = context.Game.Directions;

        var direct = directions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                         && (room.Length == 0 || d.StartingRoom.Length == 0 || d.StartingRoom.Equals(room, StringComparison.OrdinalIgnoreCase)))
                     ?? directions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (direct is null)
        {
            context.Error($"No direction named '{name}' was found.");
            return;
        }

        // When the path starts somewhere else, look for a path from here to its start.
        if (room.Length > 0 && direct.StartingRoom.Length > 0 && !direct.StartingRoom.Equals(room, StringComparison.OrdinalIgnoreCase))
        {
            var bridge = directions.FirstOrDefault(d => d.EndingRoom.Equals(direct.StartingRoom, StringComparison.OrdinalIgnoreCase)
                                                        && d.StartingRoom.Equals(room, StringComparison.OrdinalIgnoreCase));
            if (bridge is null)
            {
                context.Error($"Path not found from '{room}' to '{direct.Name}'.");
                return;
            }
            foreach (var step in Direction.Expand(bridge.Speedwalk))
            {
                await context.SendAsync(step);
            }
        }

        foreach (var step in Direction.Expand(direct.Speedwalk))
        {
            await context.SendAsync(step);
        }
    }
}

/// <summary>#reverse-path: echoes a speedwalk reversed.</summary>
public sealed class ReversePathCommand : HashCommand
{
    public override string Name => "#reverse-path";
    public override string Description => "Reverses a speed walk path.";
    public override string Usage => "#reverse-path <speedwalk>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Echo(Direction.Reverse(context.Arguments));
        return Task.CompletedTask;
    }
}

/// <summary>#repeat: repeats a command.</summary>
public sealed class RepeatCommand : HashCommand
{
    public override string Name => "#repeat";
    public override string Description => "Repeats a command N times.";
    public override string Usage => "#repeat <count> <command>";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        var (countText, command) = Terminal.Interpreter.SplitFirst(context.RawArguments);
        if (!int.TryParse(countText, out int count) || count < 1 || command.Length == 0)
        {
            context.Error($"Usage: {Usage}");
            return;
        }

        for (int i = 0; i < Math.Min(count, 1000); i++)
            await context.SendAsync(command);
    }
}

/// <summary>#task-add: schedules a command or script.</summary>
public sealed class TaskAddCommand : HashCommand
{
    public override string Name => "#task-add";
    public override string Description => "Adds a scheduled task (command or JavaScript) to run after a number of seconds.";
    public override string Usage => "#task-add -c \"command\" -s <seconds> [-j]";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var args = context.Parse();
        if (args.Get("c", "command") is not { Length: > 0 } command || !int.TryParse(args.Get("s", "seconds"), out int seconds))
        {
            context.Error($"Usage: {Usage}");
            return Task.CompletedTask;
        }

        context.Interpreter.ScheduledTasks.Add(command, args.Has("j", "js") ? ExecuteType.JavaScript : ExecuteType.Command, TimeSpan.FromSeconds(seconds));
        return Task.CompletedTask;
    }
}

/// <summary>#task-list: lists scheduled tasks.</summary>
public sealed class TaskListCommand : HashCommand
{
    public override string Name => "#task-list";
    public override string Description => "Lists the scheduled tasks.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var tasks = context.Interpreter.ScheduledTasks.Tasks;
        if (tasks.Count == 0)
        {
            context.Info("There are no scheduled tasks.");
        }

        foreach (var task in tasks)
        {
            context.Echo($"{task.RunAfter:HH:mm:ss}  {task.ExecuteAs,-10}  {task.Command}");
        }

        return Task.CompletedTask;
    }
}

/// <summary>#task-clear: removes scheduled tasks.</summary>
public sealed class TaskClearCommand : HashCommand
{
    public override string Name => "#task-clear";
    public override string Description => "Clears all scheduled and batch tasks.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Interpreter.ScheduledTasks.Clear();
        context.Interpreter.BatchTasks.Clear();
        context.Info("Tasks cleared.");
        return Task.CompletedTask;
    }
}

/// <summary>#shell: starts a program.</summary>
public sealed class ShellCommand : HashCommand
{
    public override string Name => "#shell";
    public override string Description => "Starts an executable: #shell <program> [arguments].";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var args = context.Parse();
        if (args.Values.Count == 0)
        {
            context.Error("Usage: #shell <program> [arguments]");
            return Task.CompletedTask;
        }
        Process.Start(new ProcessStartInfo(args.Values[0], string.Join(' ', args.Values.Skip(1))) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}

/// <summary>#shell-url: opens a web page.</summary>
public sealed class ShellUrlCommand : HashCommand
{
    public override string Name => "#shell-url";
    public override string Description => "Opens a URL in the default browser.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var url = context.Arguments.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            context.Error("Usage: #shell-url <http(s) url>");
            return Task.CompletedTask;
        }
        Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}

/// <summary>#toast: shows a toast in the main window.</summary>
public sealed class ToastCommand : HashCommand
{
    public override string Name => "#toast";
    public override string Description => "Shows a toast notification in the client.";
    public override string Usage => "#toast <message>";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        new UiScriptCommands().Toast("Avalon", context.Arguments);
        return Task.CompletedTask;
    }
}

/// <summary>#toast-alarm: shows a toast after a delay.</summary>
public sealed class ToastAlarmCommand : HashCommand
{
    public override string Name => "#toast-alarm";
    public override string Description => "Shows a toast notification after a number of seconds: #toast-alarm <seconds> <message>.";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        var (secondsText, message) = Terminal.Interpreter.SplitFirst(context.Arguments);
        if (!int.TryParse(secondsText, out int seconds))
        {
            context.Error("Usage: #toast-alarm <seconds> <message>");
            return;
        }
        context.Info($"Alarm set for {DateTime.Now.AddSeconds(seconds):HH:mm:ss}.");
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        new UiScriptCommands().ToastWarning("Alarm", message);
        new AudioScriptCommands().PlayNotify1();
    }
}

/// <summary>#flash: flashes the window on the task bar.</summary>
public sealed class FlashCommand : HashCommand
{
    public override string Name => "#flash";
    public override string Description => "Makes the window on the Windows task bar flash.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        WindowFlasher.Flash(System.Windows.Application.Current.MainWindow);
        return Task.CompletedTask;
    }
}

/// <summary>#append-to-file: appends text to a file.</summary>
public sealed class AppendToFileCommand : HashCommand
{
    public override string Name => "#append-to-file";
    public override string Description => "Appends text to a file: -f file -t \"text\" [-n] (add a new line).";

    public override async Task ExecuteAsync(HashCommandContext context)
    {
        var args = context.Parse();
        if (args.Get("f", "file") is not { Length: > 0 } file || args.Get("t", "text") is not { } text)
        {
            context.Error("Usage: #append-to-file -f <file> -t \"text\" [-n]");
            return;
        }
        if (!Path.IsPathRooted(file))
        {
            file = Path.Combine(Common.AppPaths.DocumentsFolder, file);
        }

        await File.AppendAllTextAsync(file, args.Has("n", "newline") ? text + Environment.NewLine : text, context.Token);
    }
}

/// <summary>#save: saves the settings.</summary>
public sealed class SaveCommand : HashCommand
{
    public override string Name => "#save";
    public override string Description => "Saves the settings and every game's aliases, triggers and variables.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        App.SaveAppSettings();
        context.Info("Settings saved.");
        return Task.CompletedTask;
    }
}

/// <summary>#is-connected: shows whether the terminal is connected.</summary>
public sealed class IsConnectedCommand : HashCommand
{
    public override string Name => "#is-connected";
    public override string Description => "Echoes whether the terminal is connected.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Info(context.Interpreter.Host.IsConnected ? "Connected" : "Disconnected");
        return Task.CompletedTask;
    }
}

/// <summary>#sql-debug: shows the script database's queued writes.</summary>
public sealed class SqlDebugCommand : HashCommand
{
    public override string Name => "#sql-debug";
    public override string Description => "Shows the script database file and its pending writes.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        context.Info($"Database: {DbScriptCommands.DatabasePath}");
        context.Info($"Pending writes: {DbScriptCommands.Instance.PendingCount}");
        return Task.CompletedTask;
    }
}

/// <summary>#gc: runs a garbage collection.</summary>
public sealed class GcCommand : HashCommand
{
    public override string Name => "#gc";
    public override string Description => "Runs a garbage collection and shows the memory in use.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        long before = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long after = GC.GetTotalMemory(true);
        context.Info($"Memory: {before / 1024:N0} KB -> {after / 1024:N0} KB");
        return Task.CompletedTask;
    }
}

/// <summary>#debug: shows interpreter diagnostics.</summary>
public sealed class DebugCommand : HashCommand
{
    public override string Name => "#debug";
    public override string Description => "Shows diagnostic information about the terminal and its automation.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        var game = context.Game;
        var interpreter = context.Interpreter;
        context.Info($"Game: {game.Name} ({game.Address})  Connected: {interpreter.Host.IsConnected}");
        context.Info($"Aliases: {game.Aliases.Count}  Triggers: {game.Triggers.Count}  Highlights: {game.HighlightTriggers.Count}  Variables: {game.Variables.Count}");
        context.Info($"Scripts: {game.Scripts.Count}  Directions: {game.Directions.Count}  Macros: {game.Macros.Count}");
        context.Info($"Scheduled tasks: {interpreter.ScheduledTasks.Tasks.Count}  Batch tasks: {interpreter.BatchTasks.Count}  Lines: {interpreter.Host.LineCount:N0}");
        return Task.CompletedTask;
    }
}

/// <summary>#null: does nothing, useful as a trigger's command placeholder.</summary>
public sealed class NullCommand : HashCommand
{
    public override string Name => "#null";
    public override string Description => "Does nothing.";
    public override Task ExecuteAsync(HashCommandContext context) => Task.CompletedTask;
}

/// <summary>#end: closes the client.</summary>
public sealed class EndCommand : HashCommand
{
    public override string Name => "#end";
    public override string Description => "Saves the settings and closes the client.";

    public override Task ExecuteAsync(HashCommandContext context)
    {
        System.Windows.Application.Current.MainWindow?.Close();
        return Task.CompletedTask;
    }
}
