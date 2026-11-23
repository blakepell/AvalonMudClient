using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalon.HashCommands;
using Avalon.Models;
using Avalon.Network;
using Avalon.Timers;

namespace Avalon.Terminal;

/// <summary>
/// Interprets commands and server output for one game terminal.
/// </summary>
/// <remarks>
/// One interpreter lives as long as its terminal, so the scripting environment, variables, scheduled tasks and
/// recent trigger history survive reconnects.  The <see cref="Connection"/> is swapped in for each session.  Call
/// from the UI synchronization context: the game settings and the transcript are bound to WPF.
/// </remarks>
public sealed partial class Interpreter
{
    private const int MaxAutomationDepth = 10;
    private const int MaxLineLength = 65536;
    private const int RecentTriggerLimit = 25;
    private static readonly Regex VariablePattern = new(@"@(\w+)", RegexOptions.Compiled);
    private static readonly Regex ArgumentPattern = new(@"%LineRaw|%Line|%([0-9]+)", RegexOptions.Compiled);

    private readonly Game _game;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly StringBuilder _pendingLine = new();
    private readonly LinkedList<(DateTime Time, Trigger Trigger)> _recentTriggers = new();
    private AnsiStyle _serverStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="Interpreter"/> class.
    /// </summary>
    /// <param name="game">The game that owns the automation definitions.</param>
    /// <param name="host">The terminal that displays output.</param>
    public Interpreter(Game game, ITerminalHost host)
    {
        _game = game;
        Host = host;
        ScheduledTasks = new ScheduledTasks(this);
        BatchTasks = new BatchTasks(this);
    }

    /// <summary>
    /// Occurs when text or formatting changes in the transcript.
    /// </summary>
    public event EventHandler<TerminalOutput>? OutputChanged;

    /// <summary>
    /// Occurs when a GMCP package arrives, before the corresponding scripts run.
    /// </summary>
    public event EventHandler<GmcpMessage>? GmcpReceived;

    /// <summary>
    /// Occurs when a command is sent to the game (after alias expansion), used for direction recording.
    /// </summary>
    public event EventHandler<string>? CommandSent;

    /// <summary>
    /// Occurs after a variable is set, with the variable's key.
    /// </summary>
    public event EventHandler<string>? VariableChanged;

    /// <summary>
    /// Gets the game this interpreter automates.
    /// </summary>
    public Game Game => _game;

    /// <summary>
    /// Gets the terminal that displays output.
    /// </summary>
    public ITerminalHost Host { get; }

    /// <summary>
    /// Gets or sets the connection for the current session, or null while disconnected.
    /// </summary>
    public TelnetConnection? Connection { get; set; }

    /// <summary>
    /// Gets a value that indicates whether the server controls input echo.
    /// </summary>
    public bool ServerEchoEnabled { get; private set; }

    /// <summary>
    /// Gets the delayed tasks added by #task-add and scripts.
    /// </summary>
    public ScheduledTasks ScheduledTasks { get; }

    /// <summary>
    /// Gets the batch of commands queued by scripts with AddBatchTask.
    /// </summary>
    public BatchTasks BatchTasks { get; }

    /// <summary>
    /// Gets the triggers that most recently matched, newest first.
    /// </summary>
    public IEnumerable<(DateTime Time, Trigger Trigger)> RecentTriggers => _recentTriggers;

    /// <summary>
    /// Gets the number of scripts currently running.
    /// </summary>
    public int ScriptsActive { get; private set; }

    /// <summary>
    /// Processes user input as commands, expanding aliases and variables.
    /// </summary>
    /// <param name="command">The input to interpret.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    public Task SendAsync(string command, CancellationToken cancellationToken = default)
        => InOrderAsync(() => ServerEchoEnabled && Connection is { } connection
            ? connection.SendLineAsync(command, cancellationToken)
            : SendCoreAsync(command, silent: false, 0, cancellationToken), cancellationToken);

    /// <summary>
    /// Processes commands without echoing them locally.
    /// </summary>
    public Task SendSilentAsync(string command, CancellationToken cancellationToken = default)
        => InOrderAsync(() => SendCoreAsync(command, silent: true, 0, cancellationToken), cancellationToken);

    /// <summary>
    /// Sends input verbatim with local echo suppressed.
    /// </summary>
    /// <param name="line">The line to send directly to the server.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    public Task SendRawAsync(string line, CancellationToken cancellationToken = default)
        => InOrderAsync(() => SendLineAsync(line, cancellationToken), cancellationToken);

    /// <summary>
    /// Runs a command or script, as an alias, trigger or task would.
    /// </summary>
    public Task ExecuteAsync(ExecuteType type, string command, CancellationToken cancellationToken = default)
        => InOrderAsync(() => ExecuteAsync(type, command, silent: false, 0, cancellationToken), cancellationToken);

    /// <summary>
    /// Executes a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    public Task RunMacroAsync(Macro macro, CancellationToken cancellationToken = default)
        => InOrderAsync(() => macro.Enabled
            ? ExecuteAsync(macro.ExecuteAs, macro.Command, silent: false, 0, cancellationToken)
            : Task.CompletedTask, cancellationToken);

    /// <summary>
    /// Runs an alias by its expression, as clicking its button in the Alias Button Actions window does.
    /// </summary>
    public Task RunAliasAsync(Alias alias, CancellationToken cancellationToken = default)
        => SendAsync(alias.AliasExpression, cancellationToken);

    /// <summary>
    /// Processes a message in the received Telnet stream.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="cancellationToken">A token that cancels message processing.</param>
    public Task ReceiveAsync(TelnetMessage message, CancellationToken cancellationToken = default)
        => InOrderAsync(() => ReceiveCoreAsync(message, cancellationToken), cancellationToken);

    /// <summary>
    /// Processes text as if the server sent it, so triggers can be tested without a connection.
    /// </summary>
    public Task SimulateIncomingAsync(string text, CancellationToken cancellationToken = default)
        => InOrderAsync(async () =>
        {
            await ReceiveTextAsync(text.EndsWith('\n') ? text : text + "\n", cancellationToken);
        }, cancellationToken);

    /// <summary>
    /// Resets the per session state when a new connection starts.
    /// </summary>
    public void ResetSession()
    {
        _pendingLine.Clear();
        _serverStyle = default;
        ServerEchoEnabled = false;
    }

    private async Task InOrderAsync(Func<Task> action, CancellationToken token)
    {
        await _gate.WaitAsync(token);
        try
        {
            await action();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task ReceiveCoreAsync(TelnetMessage message, CancellationToken token)
    {
        switch (message)
        {
            case TelnetMessage.Text text:
                await ReceiveTextAsync(text.Value, token);
                break;
            case TelnetMessage.Prompt when _pendingLine.Length > 0:
                await FinishLineAsync(newLine: false, token);
                break;
            case TelnetMessage.ServerEcho echo:
                ServerEchoEnabled = echo.Enabled;
                break;
            case TelnetMessage.Gmcp gmcp:
                GmcpReceived?.Invoke(this, gmcp.Message);
                await RunGmcpScriptsAsync(gmcp.Message, token);
                break;
        }
    }

    private async Task ReceiveTextAsync(string text, CancellationToken token)
    {
        int start = 0;
        while (start < text.Length)
        {
            int newline = text.IndexOf('\n', start);
            int end = newline < 0 ? text.Length : newline;
            int take = Math.Min(end - start, MaxLineLength - _pendingLine.Length);
            _pendingLine.Append(text, start, take);
            start += take;

            if (start == newline)
            {
                await FinishLineAsync(newLine: true, token);
                start++;
            }
            else if (_pendingLine.Length == MaxLineLength)
            {
                await FinishLineAsync(newLine: false, token);
            }
        }

        if (_pendingLine.Length > 0)
        {
            var (plain, runs, _) = AnsiText.Parse(_pendingLine.ToString(), _serverStyle);
            Emit(TerminalOutputKind.Preview, plain, runs);
        }
    }

    private async Task FinishLineAsync(bool newLine, CancellationToken token)
    {
        string raw = _pendingLine.ToString();
        _pendingLine.Clear();
        var (plain, runs, style) = AnsiText.Parse(raw, _serverStyle);
        _serverStyle = style;
        var display = plain;
        var commands = new List<(ExecuteType Type, string Command, bool Silent)>();
        bool gag = false;

        if (_game.TriggersEnabled && _game.Triggers.Count > 0 && plain.Length > 0)
        {
            var character = GetVariable("Character");
            foreach (var trigger in TriggersInPriorityOrder())
            {
                token.ThrowIfCancellationRequested();
                if (!trigger.Enabled || string.IsNullOrEmpty(trigger.Pattern) || !AppliesTo(trigger.Character, character))
                {
                    continue;
                }

                if (trigger.LineTransformer && !_game.ReplacementTriggersEnabled)
                {
                    continue;
                }

                long started = Stopwatch.GetTimestamp();
                try
                {
                    string pattern = trigger.VariableReplacement ? ReplaceVariables(trigger.Pattern) : trigger.Pattern;
                    var subject = trigger.LineTransformer ? display : plain;
                    var match = trigger.Match(subject, pattern);
                    if (!match.Success)
                    {
                        continue;
                    }

                    trigger.Count++;
                    trigger.LastMatched = DateTime.UtcNow;
                    RememberTrigger(trigger);
                    if (trigger.DisableAfterTriggered)
                    {
                        trigger.Enabled = false;
                    }

                    if (trigger.Temp)
                    {
                        _game.Triggers.Remove(trigger);
                    }

                    if (!trigger.LineTransformer)
                    {
                        foreach (Group group in match.Groups)
                        {
                            if (group.Name is { Length: > 0 } name && !char.IsAsciiDigit(name[0]) &&
                                !string.IsNullOrWhiteSpace(group.Value))
                            {
                                await SetVariableAsync(name, group.Value, trigger.IsSilent, 0, token);
                            }
                        }
                    }

                    gag |= trigger.Gag;
                    var arguments = match.Groups.Cast<Group>().Select(group => group.Value).ToArray();
                    if (trigger.ExecuteAs == ExecuteType.JavaScript)
                    {
                        arguments[0] = subject;
                    }

                    string command = ReplaceArguments(trigger.Command, arguments, subject, raw,
                        escape: trigger.ExecuteAs == ExecuteType.JavaScript);
                    if (trigger.LineTransformer)
                    {
                        if (trigger.ExecuteAs == ExecuteType.JavaScript)
                        {
                            if (await RunScriptAsync(command, expression: true, trigger.IsSilent, 0, token) is string replacement)
                            {
                                display = replacement;
                            }
                        }
                        else
                        {
                            display = display.Remove(match.Index, match.Length)
                                .Insert(match.Index, ReplaceVariables(command));
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(command))
                    {
                        commands.Add((trigger.ExecuteAs, command, trigger.IsSilent));
                    }

                    if (trigger.StopProcessing)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or RegexMatchTimeoutException)
                {
                    Echo($"Trigger {trigger.Name}: {ex.Message}\n");
                }
                finally
                {
                    double elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                    if (elapsed > trigger.MaxExecutionTime)
                    {
                        trigger.MaxExecutionTime = elapsed;
                    }
                }
            }
        }

        if (gag)
        {
            Emit(TerminalOutputKind.Server, "", []);
        }
        else
        {
            Emit(TerminalOutputKind.Server, newLine ? display + "\n" : display, display == plain ? runs : []);
        }

        foreach (var (type, command, silent) in commands)
        {
            await ExecuteAsync(type, command, silent, 0, token);
        }
    }

    private IEnumerable<Trigger> TriggersInPriorityOrder()
    {
        // Capture the UI-owned collection before running any trigger that can edit it or await.
        var triggers = _game.Triggers;
        int count = triggers.Count;
        var pool = ArrayPool<(int Priority, int Index, Trigger Trigger)>.Shared;
        var snapshot = pool.Rent(count);
        try
        {
            for (int i = 0; i < count; i++)
            {
                var trigger = triggers[i];
                snapshot[i] = (trigger.Priority, i, trigger);
            }

            // Keep collection order for equal priorities, as OrderBy did.
            Array.Sort(snapshot, 0, count);
            for (int i = 0; i < count; i++)
            {
                yield return snapshot[i].Trigger;
            }
        }
        finally
        {
            pool.Return(snapshot, clearArray: true);
        }
    }

    private void RememberTrigger(Trigger trigger)
    {
        _recentTriggers.AddFirst((DateTime.Now, trigger));
        while (_recentTriggers.Count > RecentTriggerLimit)
        {
            _recentTriggers.RemoveLast();
        }
    }

    private async Task SendCoreAsync(string input, bool silent, int depth, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (depth >= MaxAutomationDepth)
        {
            Echo("Command expansion exceeded the recursion limit.\n");
            return;
        }

        var (verb, rawArgument) = SplitFirst(input);
        if (verb.Equals("#send", StringComparison.OrdinalIgnoreCase))
        {
            await SendLineAsync(rawArgument, token);
            return;
        }

        // A hash command owns the whole line so its arguments can contain semicolons and @ signs.
        if (verb.StartsWith('#'))
        {
            await RunHashCommandAsync(input.Trim(), silent, depth, token);
            return;
        }

        foreach (var part in ReplaceVariables(input).Split(';'))
        {
            token.ThrowIfCancellationRequested();
            string text = part.Trim();
            if (text.Length == 0 && input.Length != 0)
            {
                continue;
            }

            if (text.StartsWith('#'))
            {
                await RunHashCommandAsync(text, silent, depth, token);
                continue;
            }

            var (word, tail) = SplitFirst(text);
            var character = GetVariable("Character");
            var alias = _game.AliasesEnabled && _game.Aliases.Count > 0 && word.Length > 0
                ? _game.Aliases.Where(item => item.Enabled && item.AliasExpression.Equals(word, StringComparison.OrdinalIgnoreCase)
                    && AppliesTo(item.Character, character)).OrderBy(item => item.SortOrder).FirstOrDefault()
                : null;

            if (alias is null)
            {
                await SendLineAsync(text, token);
                CommandSent?.Invoke(this, text);
                if (!silent && !ServerEchoEnabled && text.Length > 0 && _pendingLine.Length == 0)
                {
                    EchoCommand(text);
                }

                continue;
            }

            alias.Count++;
            bool script = alias.ExecuteAs == ExecuteType.JavaScript;
            var arguments = new[] { tail }.Concat(tail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            string expansion = ReplaceArguments(alias.Command, arguments, text, text, script);
            if (!script && !ArgumentPattern.IsMatch(alias.Command) && tail.Length > 0)
            {
                expansion = $"{expansion} {tail}".TrimStart();
            }

            await ExecuteAsync(alias.ExecuteAs, expansion, silent, depth + 1, token);
        }
    }

    private Task ExecuteAsync(ExecuteType type, string command, bool silent, int depth, CancellationToken token)
        => type == ExecuteType.JavaScript
            ? RunScriptAsync(command, expression: false, silent, depth, token)
            : SendCoreAsync(command, silent, depth, token);

    private async Task RunHashCommandAsync(string text, bool silent, int depth, CancellationToken token)
    {
        var (name, arguments) = SplitFirst(text);
        if (HashCommandRegistry.Find(name) is not { } command)
        {
            Echo($"Client command '{name}' was not found.  Use #cmd-list to see them, or #send to send it as game input.\n");
            return;
        }

        try
        {
            await command.ExecuteAsync(new HashCommandContext(this, arguments, silent, depth, token));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            EchoError($"{command.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs nested commands for a hash command, keeping the recursion depth.
    /// </summary>
    internal Task SendNestedAsync(string command, bool silent, int depth, CancellationToken token)
        => SendCoreAsync(command, silent, depth + 1, token);

    /// <summary>
    /// Runs nested commands or script for a hash command, keeping the recursion depth.
    /// </summary>
    internal Task ExecuteNestedAsync(ExecuteType type, string command, bool silent, int depth, CancellationToken token)
        => ExecuteAsync(type, command, silent, depth + 1, token);

    /// <summary>
    /// Sends a line to the server, or explains that there is no connection.
    /// </summary>
    internal async Task SendLineAsync(string line, CancellationToken token)
    {
        if (Connection is not { } connection || !Host.IsConnected)
        {
            EchoError("You are not connected to the game.");
            return;
        }

        await connection.SendLineAsync(line, token);
    }

    /// <summary>
    /// Sends a GMCP message to the server.
    /// </summary>
    internal async Task SendGmcpAsync(string package, string? json, CancellationToken token)
    {
        if (Connection is not { } connection || !Host.IsConnected)
        {
            EchoError("You are not connected to the game.");
            return;
        }

        try
        {
            await connection.SendGmcpAsync(new GmcpMessage(package, json), token);
        }
        catch (InvalidOperationException ex) when (ex is not ObjectDisposedException)
        {
            EchoError(ex.Message);
        }
    }

    /// <summary>
    /// Finds a variable, preferring the one scoped to the current character.
    /// </summary>
    public Variable? FindVariable(string key)
    {
        var variables = _game.Variables.Where(variable => variable.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        var global = variables.FirstOrDefault(variable => string.IsNullOrWhiteSpace(variable.Character));
        if (key.Equals("Character", StringComparison.OrdinalIgnoreCase) || _game.Variables.Count == 0)
        {
            return global;
        }

        var character = _game.Variables.FirstOrDefault(variable => variable.Key.Equals("Character", StringComparison.OrdinalIgnoreCase)
                                                                   && string.IsNullOrWhiteSpace(variable.Character))?.Value;
        return variables.FirstOrDefault(variable => !string.IsNullOrWhiteSpace(variable.Character)
            && variable.Character.Equals(character, StringComparison.OrdinalIgnoreCase)) ?? global;
    }

    /// <summary>
    /// Returns a variable's value, or an empty string when it does not exist.
    /// </summary>
    public string GetVariable(string key) => FindVariable(key)?.Value ?? "";

    /// <summary>
    /// Sets a variable, creating it when needed, and returns its OnChangeEvent script when the value changed.
    /// </summary>
    internal string? SetVariable(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (FindVariable(key) is not { } variable)
        {
            _game.Variables.Add(new Variable { Key = key, Value = value });
            VariableChanged?.Invoke(this, key);
            return null;
        }

        var old = variable.Value;
        if (old == value)
        {
            return null;
        }

        variable.Value = value;
        VariableChanged?.Invoke(this, key);
        return string.IsNullOrWhiteSpace(variable.OnChangeEvent) ? null :
            ReplaceArguments(variable.OnChangeEvent, [key, old, value], "", "", escape: true);
    }

    /// <summary>
    /// Sets a variable and runs its change script.
    /// </summary>
    internal async Task SetVariableAsync(string key, string value, bool silent, int depth, CancellationToken token)
    {
        if (SetVariable(key, value) is { } change)
        {
            await RunScriptAsync(change, expression: false, silent, depth + 1, token);
        }
    }

    /// <summary>
    /// Removes a variable.
    /// </summary>
    public bool RemoveVariable(string key) => FindVariable(key) is { } variable && _game.Variables.Remove(variable);

    /// <summary>
    /// Replaces @name references with variable values.
    /// </summary>
    public string ReplaceVariables(string text)
        => text.Contains('@') ? VariablePattern.Replace(text, match => FindVariable(match.Groups[1].Value)?.Value ?? match.Value) : text;

    private static bool AppliesTo(string scope, string character)
        => string.IsNullOrWhiteSpace(scope) || scope.Equals(character, StringComparison.OrdinalIgnoreCase);

    private static string ReplaceArguments(string text, string[] arguments, string line, string raw, bool escape)
        => ArgumentPattern.Replace(text, match =>
        {
            string value = match.Value switch
            {
                "%Line" => line,
                "%LineRaw" => raw,
                _ when int.TryParse(match.Groups[1].ValueSpan, out int index) && index < arguments.Length => arguments[index],
                _ => ""
            };
            return escape ? JsonEncodedText.Encode(value).ToString() : value;
        });

    internal static (string First, string Tail) SplitFirst(string text)
    {
        var span = text.AsSpan().TrimStart();
        int index = span.IndexOfAny(' ', '\t');
        return index < 0 ? (span.ToString(), "") : (span[..index].ToString(), span[(index + 1)..].TrimStart().ToString());
    }

    /// <summary>
    /// Writes text, which may contain ANSI codes, to the terminal.
    /// </summary>
    public void Echo(string raw)
    {
        var (text, runs, _) = AnsiText.Parse(raw, default);
        Emit(TerminalOutputKind.Local, text, runs);
    }

    /// <summary>
    /// Writes a line, which may contain ANSI codes, to the terminal.
    /// </summary>
    public void EchoLine(string raw) => Echo(raw + "\n");

    /// <summary>
    /// Writes a line in the given palette color (0 to 15).
    /// </summary>
    public void EchoColor(string text, int color)
    {
        var line = text + "\n";
        Emit(TerminalOutputKind.Local, line, [new AnsiRun(0, text.Length, new AnsiStyle(Foreground: color))]);
    }

    /// <summary>
    /// Writes an error line in red.
    /// </summary>
    public void EchoError(string text) => EchoColor(text, 9);

    /// <summary>
    /// Writes an informational line in cyan.
    /// </summary>
    public void EchoInfo(string text) => EchoColor(text, 14);

    /// <summary>
    /// Clears the terminal.
    /// </summary>
    public void Clear() => Emit(TerminalOutputKind.Clear, "", []);

    /// <summary>
    /// Echoes a command sent to the game in the palette's bright yellow, like Continuum.  The text is
    /// styled directly rather than wrapped in ANSI codes so nothing the user typed is interpreted.
    /// </summary>
    private void EchoCommand(string command) => EchoColor(command, 11);

    private void Emit(TerminalOutputKind kind, string text, IReadOnlyList<AnsiRun> runs)
        => OutputChanged?.Invoke(this, new TerminalOutput(kind, text, runs));
}
