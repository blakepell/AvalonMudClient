using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Argus.Memory;
using Avalon.Common;
using Avalon.Models;
using Avalon.Terminal;
using Mosaic.UI.Wpf.AvalonDock.Layout;
using Mosaic.UI.Wpf.Cache;
using Mosaic.UI.Wpf.Controls.Scripting;
using Mosaic.UI.Wpf.Logging;

namespace Avalon.Scripting;

/// <summary>
/// The <c>terminal</c> object scripts use to talk to the game and the terminal, ported from Continuum.
/// </summary>
/// <remarks>
/// Sends and variable callbacks are queued to avoid re-entering the interpreter. Terminal display operations
/// run immediately so a script can read or edit what it just wrote.
/// </remarks>
[ScriptModule(Name = "terminal", Description = "Sends commands to the game and works with the terminal, variables, triggers and alerts.")]
public sealed class TerminalScriptCommands(Interpreter interpreter)
{
    private ITerminalHost Host => interpreter.Host;

    private Game Game => interpreter.Game;

    /// <summary>
    /// Sends a command, expanding aliases and variables, as if it were typed.
    /// A nonempty string containing only whitespace is sent verbatim.
    /// </summary>
    public void Send(string? cmd)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(cmd))
        {
            interpreter.QueueSendRaw(cmd);
            return;
        }

        interpreter.QueueSend(cmd);
    }

    /// <summary>
    /// Sends text to the server exactly as given.
    /// </summary>
    public void SendRaw(string? cmd)
    {
        if (cmd is not null)
        {
            interpreter.QueueSendRaw(cmd);
        }
    }

    /// <summary>
    /// Sends a GMCP message to the server.
    /// </summary>
    public void SendGmcp(string package, string? json = null) => interpreter.QueueGmcp(package, json);

    /// <summary>
    /// Writes text, which may contain ANSI codes, to the terminal.
    /// </summary>
    public void Echo(string? text) => interpreter.Echo(AnsiText.FromMudColors(text ?? ""));

    /// <summary>
    /// Writes a line, which may contain ANSI codes, to the terminal.
    /// </summary>
    public void EchoLine(string? text) => Echo((text ?? "") + "\n");

    /// <summary>
    /// Writes an empty line to the terminal.
    /// </summary>
    public void EchoNewLine() => Echo("\n");

    /// <summary>
    /// Writes an informational line to the application log.
    /// </summary>
    public void Log(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            Logger.LogInfo(text);
        }
    }

    /// <summary>
    /// Writes text to the main terminal without adding a line break.
    /// </summary>
    public void WriteLine(string text) => Echo(text);

    /// <summary>
    /// Clears the terminal.
    /// </summary>
    public void Clear() => interpreter.Clear();

    // ------------------------------------------------------------------------------------- Named terminals

    /// <summary>
    /// Writes a line, which may contain ANSI codes, to a named terminal window, creating it if needed.
    /// </summary>
    /// <param name="terminalId">The terminal's name, such as "Communication" or "Loot".</param>
    /// <param name="text">The text to write.</param>
    /// <param name="includeTimestamp">Whether to prefix the line with the time.</param>
    public void WriteLine(string terminalId, string text, bool includeTimestamp = true)
        => Controls.TerminalWindow.With(terminalId, t => t.AppendLine(AnsiText.FromMudColors(text ?? ""), includeTimestamp));

    /// <summary>
    /// Writes text, which may contain ANSI codes, to a named terminal window without adding a new line.
    /// </summary>
    public void Write(string terminalId, string text)
        => Controls.TerminalWindow.With(terminalId, t => t.AppendAnsi(AnsiText.FromMudColors(text ?? "")));

    /// <summary>
    /// Clears a named terminal window.
    /// </summary>
    public void Clear(string terminalId) => Controls.TerminalWindow.With(terminalId, t => t.Clear(), create: false);

    /// <summary>
    /// Replaces the text of a named terminal window, creating it if needed.
    /// </summary>
    public void SetText(string terminalId, string text) =>
        Controls.TerminalWindow.With(terminalId, t => t.SetText(AnsiText.FromMudColors(text ?? "")));

    /// <summary>
    /// Returns the plain text of a named terminal window, or an empty string when it doesn't exist.
    /// </summary>
    public string GetText(string terminalId)
    {
        string text = "";
        Controls.TerminalWindow.With(terminalId, t => text = t.Text, create: false);
        return text;
    }

    /// <summary>
    /// Saves a named terminal window to a file, optionally keeping its ANSI styles.
    /// </summary>
    /// <param name="terminalId">The terminal's name.</param>
    /// <param name="filePath">The file to write.</param>
    /// <param name="includeAnsi">Whether to include ANSI styling.</param>
    /// <param name="lines">The number of trailing lines to save, or -1 for all of them.</param>
    public void SaveText(string terminalId, string filePath, bool includeAnsi = false, int lines = -1)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && CaptureText(terminalId, includeAnsi, lines) is { } text)
        {
            File.WriteAllText(filePath, text);
        }
    }

    /// <summary>
    /// Saves the specified number of trailing lines as plain text.
    /// </summary>
    public void SaveText(string terminalId, string filePath, int lines) => SaveText(terminalId, filePath, false, lines);

    /// <summary>
    /// Saves a named terminal asynchronously, optionally keeping its ANSI styles.
    /// </summary>
    public Task SaveTextAsync(string terminalId, string filePath, bool includeAnsi = false, int lines = -1)
    {
        return !string.IsNullOrWhiteSpace(filePath) && CaptureText(terminalId, includeAnsi, lines) is { } text
            ? File.WriteAllTextAsync(filePath, text)
            : Task.CompletedTask;
    }

    private static string? CaptureText(string terminalId, bool includeAnsi, int lines)
    {
        string? text = null;
        Controls.TerminalWindow.With(terminalId, t => text = t.GetText(includeAnsi, lines), create: false);
        return text;
    }

    /// <summary>
    /// Scrolls the game terminal to the newest output.
    /// </summary>
    public void ScrollToEnd() => Host.ScrollToEnd();

    /// <summary>
    /// Scrolls a named terminal window to its last line.
    /// </summary>
    public void ScrollToEnd(string terminalId) => Controls.TerminalWindow.With(terminalId, t => t.ScrollToEnd(), create: false);

    /// <summary>
    /// Adds a named terminal window if it doesn't exist yet.
    /// </summary>
    public void AddTerminal(string terminalId) => Controls.TerminalWindow.With(terminalId, _ => { });

    /// <summary>
    /// Adds a named terminal to a dock group on the specified side, optionally auto-hiding it.
    /// </summary>
    public void AddTerminal(string id, string containerName, bool autoHide = false, AnchorSide side = AnchorSide.Bottom)
    {
        var name = Controls.TerminalWindow.NormalizeName(id);
        if (name.Length > 0 && Application.Current.MainWindow is MainWindow main)
        {
            main.AddScriptTerminal(name, containerName, autoHide, side);
        }
    }

    /// <summary>
    /// Shows a named terminal window, creating it if needed, and optionally gives it the focus.
    /// </summary>
    public void Activate(string id, bool focus = false)
    {
        if (!string.IsNullOrWhiteSpace(id) && Application.Current.MainWindow is MainWindow main)
        {
            main.ActivateScriptTerminal(id, focus);
        }
    }

    /// <summary>
    /// Returns whether a named terminal window exists.
    /// </summary>
    public bool TerminalExists(string terminalId) => Controls.TerminalWindow.With(terminalId, _ => { }, create: false);

    /// <summary>
    /// Clears the back buffer.
    /// </summary>
    public void ClearBackBuffer() => Host.ClearBackBuffer();

    /// <summary>
    /// Waits asynchronously for the specified number of milliseconds.
    /// </summary>
    public Task PauseAsync(int milliseconds) => Task.Delay(milliseconds);

    /// <summary>
    /// Pauses a synchronous script while keeping the terminal UI responsive.
    /// </summary>
    public void Pause(int milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);
        if (milliseconds == 0)
        {
            return;
        }

        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => frame.Continue = false;
        timer.Start();
        try
        {
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            timer.Stop();
        }
    }

    /// <summary>
    /// Returns a variable's value, or an empty string.
    /// </summary>
    public string GetVariable(string key) => interpreter.GetVariable(key);

    /// <summary>
    /// Sets a variable, creating it when needed, and runs its On Change Event.
    /// </summary>
    public void SetVariable(string key, string value) => interpreter.QueueSetVariable(key, value);

    /// <summary>
    /// Sets a variable and the color its value shows in the Variables window.
    /// </summary>
    public void SetVariable(string key, string value, string color)
    {
        interpreter.QueueSetVariable(key, value);
        if (!string.IsNullOrWhiteSpace(color) && interpreter.FindVariable(key) is { } variable)
        {
            variable.ForegroundColor = GetBrush(color);
        }
    }

    /// <summary>
    /// Removes a variable.
    /// </summary>
    public void RemoveVariable(string key) => interpreter.RemoveVariable(key);

    /// <summary>
    /// Replaces @name references with variable values.
    /// </summary>
    public string ReplaceVariables(string? text) => interpreter.ReplaceVariables(text ?? "");

    /// <summary>
    /// Shows a variable in the Variables window.
    /// </summary>
    public void ShowVariable(string key)
    {
        if (interpreter.FindVariable(key) is { } variable)
        {
            variable.IsVisible = true;
        }
    }

    /// <summary>
    /// Hides a variable from the Variables window.
    /// </summary>
    public void HideVariable(string key)
    {
        if (interpreter.FindVariable(key) is { } variable)
        {
            variable.IsVisible = false;
        }
    }

    /// <summary>
    /// Sets the Character variable that character scoped aliases, triggers and variables use.
    /// </summary>
    public void SetCharacter(string name) => interpreter.QueueSetVariable("Character", name);

    /// <summary>
    /// Enables the aliases and triggers in a group.
    /// </summary>
    public void EnableGroup(string? groupName) => SetGroupEnabled(Game, groupName, true);

    /// <summary>
    /// Disables the aliases and triggers in a group.
    /// </summary>
    public void DisableGroup(string? groupName) => SetGroupEnabled(Game, groupName, false);

    /// <summary>
    /// Enables a trigger by its ID or name.
    /// </summary>
    public void EnableTrigger(string id) => SetTriggerEnabled(id, true);

    /// <summary>
    /// Disables a trigger by its ID or name.
    /// </summary>
    public void DisableTrigger(string id) => SetTriggerEnabled(id, false);

    /// <summary>
    /// Sets the title of the terminal's tab.
    /// </summary>
    public void SetTitle(string title) => Host.SetTitle(title);

    /// <summary>
    /// Sets the status bar text.
    /// </summary>
    public Task SetStatus(string text, int ms = 4000) => Host.SetStatusAsync(text, ms);

    /// <summary>
    /// Moves keyboard focus to the command input.
    /// </summary>
    public void FocusInput() => Host.FocusInput();

    /// <summary>
    /// Adds a command or script to the batch started by StartBatch.
    /// </summary>
    public void AddBatchTask(string command, bool isJavaScript) => interpreter.BatchTasks.Add(command, isJavaScript ? ExecuteType.JavaScript : ExecuteType.Command);

    /// <summary>
    /// Runs the batch, waiting the given number of seconds between commands.
    /// </summary>
    public void StartBatch(int secondsInBetweenCommands = 1) => interpreter.BatchTasks.Start(TimeSpan.FromSeconds(secondsInBetweenCommands));

    /// <summary>
    /// Clears all pending scheduled tasks.
    /// </summary>
    public void ClearTasks() => interpreter.ScheduledTasks.Clear();

    /// <summary>
    /// Runs a command or script after a number of seconds.
    /// </summary>
    public void AddScheduledTask(string command, bool isJavaScript, int seconds)
        => interpreter.ScheduledTasks.Add(command, isJavaScript ? ExecuteType.JavaScript : ExecuteType.Command, TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// Shows an information alert over the terminal.
    /// </summary>
    public void AlertInfo(string message, string title = "Alert", int duration = 0) => ShowAlert(message, title, duration, InfoBarSeverity.Information);

    /// <summary>
    /// Shows a warning alert over the terminal.
    /// </summary>
    public void AlertWarning(string message, string title = "Alert", int duration = 0) => ShowAlert(message, title, duration, InfoBarSeverity.Warning);

    /// <summary>
    /// Shows an error alert over the terminal.
    /// </summary>
    public void AlertError(string message, string title = "Alert", int duration = 0) => ShowAlert(message, title, duration, InfoBarSeverity.Error);

    /// <summary>
    /// Shows a success alert over the terminal.
    /// </summary>
    public void AlertSuccess(string message, string title = "Alert", int duration = 0) => ShowAlert(message, title, duration, InfoBarSeverity.Success);

    /// <summary>
    /// Shows a fully specified alert over the terminal.
    /// </summary>
    public void Alert(Alert alert) => Host.ShowAlert(alert);

    /// <summary>
    /// Closes every alert.
    /// </summary>
    public void AlertCloseAll() => Host.CloseAlerts();

    /// <summary>
    /// Closes all alerts with the given message.
    /// </summary>
    public void AlertRemoveByMessage(string message) => Host.RemoveAlertsByMessage(message);

    /// <summary>
    /// Returns the last line that is not empty.
    /// </summary>
    public string LastNonEmptyLine() => Host.LastNonEmptyLine();

    /// <summary>
    /// Returns the last lines of the terminal, oldest first.
    /// </summary>
    public string[] LastLines(int numberToTake) => Host.LastLines(numberToTake).ToArray();

    /// <summary>
    /// Returns the last lines of the terminal, oldest first when <paramref name="reverseOrder"/> is true.
    /// </summary>
    public string[] LastLines(int numberToTake, bool reverseOrder)
    {
        var lines = Host.LastLines(numberToTake).ToArray();
        if (!reverseOrder)
        {
            Array.Reverse(lines);
        }

        return lines;
    }

    /// <summary>
    /// Returns the lines from the last one containing <paramref name="startLineContains"/> to the end.
    /// </summary>
    public string[] LastLinesBetweenContains(string startLineContains) =>
        LastLinesBetween(line => line.Contains(startLineContains, StringComparison.Ordinal));

    /// <summary>
    /// Returns the lines between the last matching start and the first matching end.
    /// </summary>
    public string[] LastLinesBetweenContains(string startLineContains, string endLineContains) =>
        LastLinesBetween(line => line.Contains(startLineContains, StringComparison.Ordinal),
            line => line.Contains(endLineContains, StringComparison.Ordinal));

    /// <summary>
    /// Returns lines from the last line with the given prefix through the end of the terminal.
    /// </summary>
    public string[] LastLinesBetweenStartsWith(string startLineStartsWith) =>
        LastLinesBetween(line => line.StartsWith(startLineStartsWith, StringComparison.Ordinal));

    /// <summary>
    /// Returns the lines between the last start prefix and the first matching end prefix.
    /// </summary>
    public string[] LastLinesBetweenStartsWith(string startLineStartsWith, string endLineStartsWith) =>
        LastLinesBetween(line => line.StartsWith(startLineStartsWith, StringComparison.Ordinal),
            line => line.StartsWith(endLineStartsWith, StringComparison.Ordinal));

    private string[] LastLinesBetween(Predicate<string> start, Predicate<string>? end = null)
    {
        var lines = Host.LastLines(Host.LineCount).ToArray();
        int first = Array.FindLastIndex(lines, start);
        if (first < 0)
        {
            return end is null ? lines : [];
        }

        int last = end is null ? lines.Length - 1 : Array.FindIndex(lines, first, end);
        return lines[first..(Math.Max(first, last) + 1)];
    }

    /// <summary>
    /// Replaces the last occurrence of text in the game terminal.
    /// </summary>
    public void ReplaceLastInstance(string searchText, string replacementText) =>
        Host.ReplaceText(searchText, AnsiText.FromMudColors(replacementText ?? ""), lastOnly: true);

    /// <summary>
    /// Replaces all occurrences of text in the game terminal.
    /// </summary>
    public void ReplaceAll(string searchText, string replacementText) =>
        Host.ReplaceText(searchText, AnsiText.FromMudColors(replacementText ?? ""), lastOnly: false);

    /// <summary>
    /// Replaces all occurrences of text in a named terminal.
    /// </summary>
    public void ReplaceAll(string terminalId, string searchText, string replacementText) =>
        Controls.TerminalWindow.With(terminalId, t => t.ReplaceText(searchText, AnsiText.FromMudColors(replacementText ?? "")), create: false);

    /// <summary>
    /// Removes a one-based line from the game terminal.
    /// </summary>
    public void RemoveLine(int lineNumber) => Host.RemoveLine(lineNumber);

    /// <summary>
    /// Removes trailing whitespace from the game terminal.
    /// </summary>
    public void TrimEnd() => Host.TrimEnd();

    /// <summary>
    /// Removes the last line of the terminal.
    /// </summary>
    public void RemoveLastLine() => Host.RemoveLastLine();

    /// <summary>
    /// Removes ANSI color codes from text.
    /// </summary>
    public string RemoveColor(string buf) => AnsiText.Parse(buf, default).Text;

    /// <summary>
    /// Highlights <paramref name="input"/> wherever it appears, replacing any highlight with the same key.
    /// </summary>
    public void Highlight(string lookupId, string input) => Highlight(lookupId, input, "#000000", "#FFFF00");

    /// <summary>
    /// Highlights <paramref name="input"/> wherever it appears in the given colors.
    /// </summary>
    public void Highlight(string lookupId, string input, string foregroundColor, string backgroundColor)
    {
        var highlight = FindHighlight(lookupId);
        if (highlight is null)
        {
            highlight = new HighlightTrigger { LookupKey = lookupId };
            Game.HighlightTriggers.Add(highlight);
        }

        highlight.Pattern = Regex.Escape(input);
        highlight.ForegroundColor = GetBrush(foregroundColor);
        highlight.BackgroundColor = GetBrush(backgroundColor);
        highlight.Enabled = true;
        Host.Redraw();
    }

    /// <summary>
    /// Adds another word to an existing highlight.
    /// </summary>
    public void HighlightAppend(string lookupId, string input)
    {
        if (FindHighlight(lookupId) is not { } highlight)
        {
            Highlight(lookupId, input);
            return;
        }

        var escaped = Regex.Escape(input);
        if (!highlight.Pattern.Split('|').Contains(escaped))
        {
            highlight.Pattern = string.IsNullOrEmpty(highlight.Pattern) ? escaped : $"{highlight.Pattern}|{escaped}";
        }

        Host.Redraw();
    }

    /// <summary>
    /// Removes a word from a highlight.
    /// </summary>
    public void HighlightRemove(string lookupId, string input)
    {
        if (FindHighlight(lookupId) is not { } highlight)
        {
            return;
        }

        var escaped = Regex.Escape(input);
        highlight.Pattern = string.Join('|', highlight.Pattern.Split('|').Where(part => part != escaped));
        if (string.IsNullOrEmpty(highlight.Pattern))
        {
            Game.HighlightTriggers.Remove(highlight);
        }

        Host.Redraw();
    }

    /// <summary>
    /// Enables a highlight by its lookup key.
    /// </summary>
    public void HighlightEnable(string lookupId)
    {
        if (FindHighlight(lookupId) is { } highlight)
        {
            highlight.Enabled = true;
        }

        Host.Redraw();
    }

    /// <summary>
    /// Disables a highlight by its lookup key.
    /// </summary>
    public void HighlightDisable(string lookupId)
    {
        if (FindHighlight(lookupId) is { } highlight)
        {
            highlight.Enabled = false;
        }

        Host.Redraw();
    }

    /// <summary>
    /// Returns whether the terminal is connected.
    /// </summary>
    public bool IsConnected() => Host.IsConnected;

    /// <summary>
    /// Connects to the game.
    /// </summary>
    public Task<bool> ConnectAsync(bool executeOnConnected = false) => Host.ConnectAsync(executeOnConnected);

    /// <summary>
    /// Disconnects from the game.
    /// </summary>
    public bool Disconnect()
    {
        Host.Disconnect();
        return true;
    }

    /// <summary>
    /// Returns a path in Avalon's application data folder.
    /// </summary>
    public string GetAppDataFilePath(string fileName) => Path.Combine(AppPaths.AppDataFolder, fileName);

    /// <summary>
    /// Returns a path in Avalon's documents folder.
    /// </summary>
    public string GetDocumentsFilePath(string fileName)
    {
        var folder = AppServices.IsRegistered<AppSettings>() ? AppServices.GetRequiredService<AppSettings>().ClientSettings?.DocumentsFolder : null;
        return Path.Combine(string.IsNullOrWhiteSpace(folder) ? AppPaths.DocumentsFolder : folder, fileName);
    }

    /// <summary>
    /// Returns a brush for a color such as #FF0000 or Red.
    /// </summary>
    public SolidColorBrush GetBrush(string hex)
    {
        try
        {
            return hex is { Length: 3 or 4 or 6 or 8 } && hex.All(Uri.IsHexDigit)
                ? ColorPaletteCache.GetBrush(hex)
                : ColorPaletteCache.GetBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        catch (FormatException)
        {
            return Brushes.White;
        }
    }

    /// <summary>
    /// Sets the enabled state of every alias and trigger in a group, returning how many changed.
    /// </summary>
    public static int SetGroupEnabled(Game game, string? groupName, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return 0;
        }

        int count = 0;
        foreach (var alias in game.Aliases.Where(a => a.Group.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
        {
            alias.Enabled = enabled;
            count++;
        }
        foreach (var trigger in game.Triggers.Where(t => t.Group.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
        {
            trigger.Enabled = enabled;
            count++;
        }
        return count;
    }

    private void SetTriggerEnabled(string id, bool enabled)
    {
        foreach (var trigger in Game.Triggers.Where(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
                     || t.Name.Equals(id, StringComparison.OrdinalIgnoreCase)))
        {
            trigger.Enabled = enabled;
        }
    }

    private HighlightTrigger? FindHighlight(string lookupId)
        => Game.HighlightTriggers.FirstOrDefault(h => h.LookupKey.Equals(lookupId, StringComparison.OrdinalIgnoreCase));

    private void ShowAlert(string message, string title, int duration, InfoBarSeverity severity)
        => Host.ShowAlert(new Alert { Message = message, Title = title, Duration = duration, Severity = severity, Permanent = duration == 0 });
}
