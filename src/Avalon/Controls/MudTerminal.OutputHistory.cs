using ICSharpCode.AvalonEdit.Document;

namespace Avalon.Controls;

/// <summary>
/// A command sent to the game and the output line it was sent at.
/// </summary>
/// <param name="Text">The command after alias expansion, as it was sent.</param>
/// <param name="Line">The output line the command was sent at, numbered as <see cref="MudTerminal.FirstOutputLine"/> is.</param>
/// <param name="Time">When the command was sent.</param>
public sealed record SentCommand(string Text, long Line, DateTime Time);

public partial class MudTerminal
{
    private const int CommandHistoryLimit = 200;
    private readonly LinkedList<SentCommand> _sentCommands = new();
    private long _linesTrimmed;

    /// <summary>
    /// Gets the number of the oldest output line still in the scrollback.  Output lines are numbered from the
    /// start of the session, so a line keeps its number when older lines are trimmed from the scrollback.
    /// </summary>
    public long FirstOutputLine => _linesTrimmed + 1;

    /// <summary>
    /// Gets the number of the newest output line, ignoring the empty line left by a final newline.
    /// </summary>
    public long LastOutputLine => _linesTrimmed + LastDocumentLine();

    /// <summary>
    /// Gets the commands sent to the game, newest first.  Commands typed while the server hides input, such as
    /// passwords, aren't recorded.
    /// </summary>
    public IEnumerable<SentCommand> SentCommands => _sentCommands;

    /// <summary>
    /// Returns the text of an output line without color codes, or null when the line isn't in the scrollback.
    /// </summary>
    /// <param name="line">The line number, numbered as <see cref="FirstOutputLine"/> is.</param>
    public string? GetOutputLine(long line)
    {
        long number = line - _linesTrimmed;
        var document = Output.Document;
        return number >= 1 && number <= document.LineCount ? document.GetText(document.GetLineByNumber((int)number)) : null;
    }

    private void InitializeOutputHistory() => Output.Document.Changing += OutputDocument_OnChanging;

    private int LastDocumentLine()
    {
        var document = Output.Document;
        int last = document.LineCount;
        return last > 1 && document.GetLineByNumber(last).Length == 0 ? last - 1 : last;
    }

    private void RecordSentCommand(string command)
    {
        // While the server hides input the text is usually a password, so keep it out of the history.
        if (Interpreter.ServerEchoEnabled || string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        // The command's echo, or the prompt it was typed at, is on the document's last line.
        _sentCommands.AddFirst(new SentCommand(command, _linesTrimmed + Output.Document.LineCount, DateTime.Now));
        while (_sentCommands.Count > CommandHistoryLimit)
        {
            _sentCommands.RemoveLast();
        }
    }

    /// <summary>
    /// Counts the lines removed from the top of the scrollback so line numbers stay the same after a trim.
    /// </summary>
    private void OutputDocument_OnChanging(object? sender, DocumentChangeEventArgs e)
    {
        if (e.Offset != 0 || e.RemovalLength == 0)
        {
            return;
        }

        var removed = e.RemovedText.Text;
        _linesTrimmed += removed.AsSpan().Count('\n');

        // Clearing everything also ends a last line that had no newline.
        if (e.RemovalLength == Output.Document.TextLength && removed[^1] != '\n')
        {
            _linesTrimmed++;
        }
    }
}
