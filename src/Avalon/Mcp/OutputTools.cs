using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalon.Controls;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// Read-only MCP tools that let an assistant look at the game's output in small pieces, so it can write
/// trigger patterns and scripts against real text without reading the whole scrollback.
/// </summary>
[McpServerToolType]
public sealed class OutputTools
{
    private const int MaxLineLength = 400;
    private const int MaxContext = 20;
    private const int MaxLinesPerCall = 200;

    private const string ResultNote =
        "Lines are returned without color codes, which is the text trigger patterns are matched against. " +
        "Line n of a block is startLine + n, and line numbers stay the same as the scrollback grows.";

    /// <summary>
    /// Gets the output that followed a command.
    /// </summary>
    [McpServerTool(Name = "get_command_output", ReadOnly = true, Idempotent = true)]
    [Description("Returns what the game sent back after a command was sent, from the command's line up to the next command. " +
                 "Use this to see the real text a trigger has to match, such as the output of \"score\" or \"stats\". " +
                 "If the command hasn't been sent this session, ask the player to type it once. " + ResultNote)]
    public static Task<string> GetCommandOutput(
        [Description("The command, such as \"stats\". It matches a sent command that is this text or starts with it followed by a space, ignoring case. Aliases are expanded, so use the game command.")] string command,
        [Description("Which time it was sent: 1 for the most recent, 2 for the one before, and so on.")] int occurrence = 1,
        [Description("The most lines to return, up to 200.")] int maxLines = 80,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var (game, terminal) = ResolveTerminal(profile);
            var wanted = command?.Trim() ?? "";
            if (wanted.Length == 0)
            {
                throw new McpException("Pass the command whose output you want.");
            }

            var commands = terminal.SentCommands.ToArray();
            var matches = commands.Select((sent, index) => (sent, index)).Where(item => IsCommand(item.sent.Text, wanted)).ToArray();
            if (matches.Length < Math.Max(1, occurrence))
            {
                throw new McpException(matches.Length == 0
                    ? $"\"{wanted}\" hasn't been sent in profile \"{game.Name}\" since it was opened. Ask the player to type it, then try again."
                    : $"\"{wanted}\" has only been sent {matches.Length} time(s).");
            }

            var (target, position) = matches[Math.Max(1, occurrence) - 1];
            if (target.Line < terminal.FirstOutputLine)
            {
                throw new McpException($"The output of \"{target.Text}\" has scrolled out of the scrollback. Ask the player to send it again.");
            }

            // The output runs until the next command that was sent on a later line. Commands sent together share a line.
            var next = commands.Take(position).Reverse().FirstOrDefault(sent => sent.Line > target.Line);
            long end = terminal.LastOutputLine;
            if (next is not null)
            {
                end = next.Line;

                // Leave out the next command's echo when it is on a line of its own, but keep a prompt it was typed at.
                if (string.Equals(terminal.GetOutputLine(next.Line)?.Trim(), next.Text.Trim(), StringComparison.Ordinal))
                {
                    end--;
                }
            }

            int limit = Math.Clamp(maxLines, 1, MaxLinesPerCall);
            long last = Math.Min(end, target.Line + limit - 1);
            return McpContext.Json(new
            {
                profile = game.Name,
                command = target.Text,
                sentAt = target.Time.ToString("HH:mm:ss"),
                nextCommand = next?.Text,
                startLine = target.Line,
                lines = ReadLines(terminal, target.Line, last),
                truncated = last < end ? $"{end - last} more line(s). Call get_output_lines with fromLine {last + 1} for the rest." : null,
                note = next is null && last == target.Line ? "Only the command's own line has arrived so far; the game may not have answered yet." : null
            });
        });

    /// <summary>
    /// Searches the scrollback.
    /// </summary>
    [McpServerTool(Name = "search_output", ReadOnly = true, Idempotent = true)]
    [Description("Searches the game's output for lines matching a pattern, newest first, and returns each match with a few lines around it. " +
                 "Use it to find example lines for a trigger, such as experience gains, deaths or a login menu. " +
                 "Overlapping matches are merged into one block. " + ResultNote)]
    public static Task<string> SearchOutput(
        [Description("The text to find. A .NET regular expression unless regex is false.")] string pattern,
        [Description("Whether the pattern is a regular expression. False to find plain text.")] bool regex = true,
        [Description("Whether matching ignores case.")] bool ignoreCase = true,
        [Description("Lines of context to include before each match, up to 20.")] int before = 2,
        [Description("Lines of context to include after each match, up to 20.")] int after = 2,
        [Description("The most matches to return. The total number of matches is always reported.")] int maxMatches = 10,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var (game, terminal) = ResolveTerminal(profile);
            if (string.IsNullOrEmpty(pattern))
            {
                throw new McpException("Pass the text or pattern to find.");
            }

            var options = RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            Regex matcher;
            try
            {
                matcher = new Regex(regex ? pattern : Regex.Escape(pattern), options, TimeSpan.FromMilliseconds(250));
            }
            catch (ArgumentException ex)
            {
                throw new McpException($"The pattern isn't a valid regular expression: {ex.Message}");
            }

            int take = Math.Clamp(maxMatches, 1, 50);
            int contextBefore = Math.Clamp(before, 0, MaxContext);
            int contextAfter = Math.Clamp(after, 0, MaxContext);
            long first = terminal.FirstOutputLine;
            long lastLine = terminal.LastOutputLine;
            var found = new List<long>();
            int total = 0;
            try
            {
                for (long line = lastLine; line >= first; line--)
                {
                    if (matcher.IsMatch(terminal.GetOutputLine(line) ?? ""))
                    {
                        total++;
                        if (found.Count < take)
                        {
                            found.Add(line);
                        }
                    }
                }
            }
            catch (RegexMatchTimeoutException)
            {
                throw new McpException("The pattern took too long to match. Make it more specific, for example by anchoring it with ^.");
            }

            // Merge the windows around matches that touch or overlap, keeping the newest block first.
            var blocks = new List<(long Start, long End, List<long> Matches)>();
            foreach (var line in found)
            {
                long start = Math.Max(first, line - contextBefore);
                long end = Math.Min(lastLine, line + contextAfter);
                if (blocks.Count > 0 && end >= blocks[^1].Start - 1)
                {
                    var block = blocks[^1];
                    block.Matches.Add(line);
                    blocks[^1] = (Math.Min(start, block.Start), block.End, block.Matches);
                }
                else
                {
                    blocks.Add((start, end, [line]));
                }
            }

            return McpContext.Json(new
            {
                profile = game.Name,
                pattern,
                totalMatches = total,
                searchedLines = $"{first}-{lastLine}",
                blocks = blocks.Select(block => new
                {
                    startLine = block.Start,
                    matchLines = block.Matches.OrderBy(line => line).ToArray(),
                    lines = ReadLines(terminal, block.Start, block.End)
                }).ToArray(),
                more = total > found.Count ? $"{total - found.Count} older match(es) not shown." : null
            });
        });

    /// <summary>
    /// Gets a range of output lines.
    /// </summary>
    [McpServerTool(Name = "get_output_lines", ReadOnly = true, Idempotent = true)]
    [Description("Returns a range of output lines by line number, for example to see more around a block returned by search_output or get_command_output. " + ResultNote)]
    public static Task<string> GetOutputLines(
        [Description("The first line to return.")] long fromLine,
        [Description("How many lines to return, up to 200.")] int count = 40,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var (game, terminal) = ResolveTerminal(profile);
            long first = terminal.FirstOutputLine;
            long last = terminal.LastOutputLine;
            long start = Math.Max(first, fromLine);
            long end = Math.Min(last, start + Math.Clamp(count, 1, MaxLinesPerCall) - 1);
            if (start > last)
            {
                throw new McpException($"Line {fromLine} hasn't been written yet. The scrollback holds lines {first} to {last}.");
            }

            return McpContext.Json(new
            {
                profile = game.Name,
                startLine = start,
                lines = ReadLines(terminal, start, end),
                note = fromLine < first ? $"Lines before {first} have scrolled out of the scrollback." : null,
                more = end < last ? $"{last - end} newer line(s) after this block." : null
            });
        });

    /// <summary>
    /// Gets the newest output.
    /// </summary>
    [McpServerTool(Name = "get_recent_output", ReadOnly = true, Idempotent = true)]
    [Description("Returns the newest lines of game output, up to a size limit. Prefer get_command_output or search_output when you know what you're looking for. " + ResultNote)]
    public static Task<string> GetRecentOutput(
        [Description("How many lines to return, up to 200.")] int count = 40,
        [Description("The most characters to return. Older lines are left out first to stay under it.")] int maxChars = 6000,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var (game, terminal) = ResolveTerminal(profile);
            long first = terminal.FirstOutputLine;
            long end = terminal.LastOutputLine;
            long start = end + 1;
            int budget = Math.Max(200, maxChars);
            int limit = Math.Clamp(count, 1, MaxLinesPerCall);
            while (start > first && end - start + 1 < limit)
            {
                int length = Math.Min(MaxLineLength, terminal.GetOutputLine(start - 1)?.Length ?? 0) + 1;
                if (length > budget)
                {
                    break;
                }

                budget -= length;
                start--;
            }

            return McpContext.Json(new
            {
                profile = game.Name,
                startLine = start,
                lines = ReadLines(terminal, start, end),
                more = start > first ? $"{start - first} older line(s). Call get_output_lines or search_output to see them." : null
            });
        });

    /// <summary>
    /// Lists recent commands.
    /// </summary>
    [McpServerTool(Name = "list_recent_commands", ReadOnly = true, Idempotent = true)]
    [Description("Lists the commands recently sent to the game, newest first, with the output line each was sent at. Commands typed while the game hides input, such as passwords, aren't recorded.")]
    public static Task<string> ListRecentCommands(
        [Description("How many commands to return, up to 200.")] int count = 20,
        [Description(McpContext.ProfileDescription)] string? profile = null) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            var (game, terminal) = ResolveTerminal(profile);
            var commands = terminal.SentCommands.Take(Math.Clamp(count, 1, 200))
                .Select(sent => new { command = sent.Text, line = sent.Line, sentAt = sent.Time.ToString("HH:mm:ss") })
                .ToArray();
            return McpContext.Json(new { profile = game.Name, count = commands.Length, commands });
        });

    private static (Models.Game Game, MudTerminal Terminal) ResolveTerminal(string? profile)
    {
        var game = McpContext.ResolveProfile(profile);
        var terminal = (System.Windows.Application.Current.MainWindow as MainWindow)?.FindTerminal(game)
            ?? throw new McpException($"Profile \"{game.Name}\" isn't open, so it has no game output. Ask the player to open it.");
        return (game, terminal);
    }

    private static bool IsCommand(string sent, string wanted)
    {
        var text = sent.Trim();
        return text.Equals(wanted, StringComparison.OrdinalIgnoreCase)
            || text.StartsWith(wanted + " ", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReadLines(MudTerminal terminal, long start, long end)
    {
        var lines = new List<string>();
        for (long line = start; line <= end; line++)
        {
            var text = terminal.GetOutputLine(line) ?? "";
            lines.Add(text.Length > MaxLineLength ? text[..MaxLineLength] + "…" : text);
        }

        return [.. lines];
    }
}
