using System.Reflection;
using System.Text;
using Avalon.Models;
using Avalon.Terminal;

namespace Avalon.HashCommands;

/// <summary>
/// A client command typed with a leading # (for example #repeat 3 kick), handled by Avalon instead of the game.
/// </summary>
/// <remarks>
/// Subclasses are discovered by reflection like Continuum's, so adding a class adds the command.
/// </remarks>
public abstract class HashCommand
{
    /// <summary>
    /// Gets the name including the #, such as #repeat.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a one line description shown by #cmd-list.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the usage text shown when the arguments are wrong.
    /// </summary>
    public virtual string Usage => Name;

    /// <summary>
    /// Runs the command.
    /// </summary>
    public abstract Task ExecuteAsync(HashCommandContext context);
}

/// <summary>
/// What a <see cref="HashCommand"/> runs against.
/// </summary>
public sealed class HashCommandContext(Interpreter interpreter, string rawArguments, bool silent, int depth, CancellationToken token)
{
    /// <summary>
    /// Gets the interpreter the command was typed into.
    /// </summary>
    public Interpreter Interpreter { get; } = interpreter;

    /// <summary>
    /// Gets the game.
    /// </summary>
    public Game Game => Interpreter.Game;

    /// <summary>
    /// Gets the text after the command name, exactly as typed.
    /// </summary>
    public string RawArguments { get; } = rawArguments;

    /// <summary>
    /// Gets the text after the command name with @variables replaced.
    /// </summary>
    public string Arguments => Interpreter.ReplaceVariables(RawArguments);

    /// <summary>
    /// Gets whether local echo is suppressed.
    /// </summary>
    public bool Silent { get; } = silent;

    /// <summary>
    /// Gets the recursion depth.
    /// </summary>
    public int Depth { get; } = depth;

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken Token { get; } = token;

    /// <summary>
    /// Sends commands, expanding aliases, without exceeding the recursion limit.
    /// </summary>
    public Task SendAsync(string command) => Interpreter.SendNestedAsync(command, Silent, Depth, Token);

    /// <summary>
    /// Runs a command or script.
    /// </summary>
    public Task ExecuteAsync(ExecuteType type, string command) => Interpreter.ExecuteNestedAsync(type, command, Silent, Depth, Token);

    /// <summary>
    /// Writes a line to the terminal.
    /// </summary>
    public void Echo(string text) => Interpreter.EchoLine(text);

    /// <summary>
    /// Writes a red error line to the terminal.
    /// </summary>
    public void Error(string text) => Interpreter.EchoError(text);

    /// <summary>
    /// Writes a cyan informational line to the terminal.
    /// </summary>
    public void Info(string text) => Interpreter.EchoInfo(text);

    /// <summary>
    /// Parses the arguments into positional values and -x / --long options.
    /// </summary>
    public CommandArguments Parse() => CommandArguments.Parse(Arguments);
}

/// <summary>
/// Positional values and options parsed from a hash command's arguments, honoring double quotes.
/// </summary>
public sealed class CommandArguments
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the positional values.
    /// </summary>
    public List<string> Values { get; } = [];

    /// <summary>
    /// Returns whether a flag such as -j or --js was given.
    /// </summary>
    public bool Has(params string[] names) => names.Any(_options.ContainsKey);

    /// <summary>
    /// Returns the value of an option such as -c "text" or --command "text".
    /// </summary>
    public string? Get(params string[] names) => names.Select(n => _options.TryGetValue(n, out var v) ? v : null).FirstOrDefault(v => v is not null);

    /// <summary>
    /// Splits <paramref name="text"/> into tokens and options.
    /// </summary>
    public static CommandArguments Parse(string text)
    {
        var result = new CommandArguments();
        var tokens = Tokenize(text);
        for (int i = 0; i < tokens.Count; i++)
        {
            var (token, quoted) = tokens[i];
            if (!quoted && token.Length > 1 && token[0] == '-' && !char.IsDigit(token[1]))
            {
                string name = token.TrimStart('-');
                string? value = i + 1 < tokens.Count && (tokens[i + 1].Quoted || !tokens[i + 1].Text.StartsWith('-'))
                    ? tokens[++i].Text
                    : null;
                result._options[name] = value ?? "";
                continue;
            }
            result.Values.Add(token);
        }
        return result;
    }

    private static List<(string Text, bool Quoted)> Tokenize(string text)
    {
        var tokens = new List<(string, bool)>();
        var sb = new StringBuilder();
        bool inQuote = false, quoted = false;
        foreach (char c in text)
        {
            if (c == '"')
            {
                inQuote = !inQuote;
                quoted = true;
                continue;
            }
            if (char.IsWhiteSpace(c) && !inQuote)
            {
                if (sb.Length > 0 || quoted)
                {
                    tokens.Add((sb.ToString(), quoted));
                }

                sb.Clear();
                quoted = false;
                continue;
            }
            sb.Append(c);
        }
        if (sb.Length > 0 || quoted)
        {
            tokens.Add((sb.ToString(), quoted));
        }

        return tokens;
    }
}

/// <summary>
/// Finds hash commands by name.
/// </summary>
public static class HashCommandRegistry
{
    private static readonly Lazy<Dictionary<string, HashCommand>> Commands = new(() =>
        typeof(HashCommand).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false } && t.IsSubclassOf(typeof(HashCommand)) && t.GetConstructor(Type.EmptyTypes) is not null)
            .Select(t => (HashCommand)Activator.CreateInstance(t)!)
            .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets every command, sorted by name.
    /// </summary>
    public static IEnumerable<HashCommand> All => Commands.Value.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the command with the given name, such as #repeat.
    /// </summary>
    public static HashCommand? Find(string name) => Commands.Value.GetValueOrDefault(name);
}
