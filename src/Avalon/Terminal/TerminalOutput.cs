namespace Avalon.Terminal;

/// <summary>
/// Describes a transcript update for a terminal view.
/// </summary>
/// <param name="Kind">The operation to apply to the transcript.</param>
/// <param name="Text">The plain text to display.</param>
/// <param name="Runs">The ANSI styles to apply to the text.</param>
public sealed record TerminalOutput(TerminalOutputKind Kind, string Text, IReadOnlyList<AnsiRun> Runs);

/// <summary>
/// Specifies how to apply a transcript update.
/// </summary>
public enum TerminalOutputKind
{
    /// <summary>
    /// Replaces the current unfinished server output.
    /// </summary>
    Preview,

    /// <summary>
    /// Replaces the unfinished server output and commits it to scrollback.
    /// </summary>
    Server,

    /// <summary>
    /// Appends local output before any unfinished server output.
    /// </summary>
    Local,

    /// <summary>
    /// Clears the transcript.
    /// </summary>
    Clear
}
