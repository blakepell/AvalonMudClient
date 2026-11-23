using Avalon.Models;

namespace Avalon.Terminal;

/// <summary>
/// The terminal UI an <see cref="Interpreter"/> and its scripts and hash commands work with.
/// </summary>
/// <remarks>
/// Implemented by the MudTerminal.  Members are called on the UI thread.
/// </remarks>
public interface ITerminalHost
{
    /// <summary>
    /// Gets the game the terminal is playing.
    /// </summary>
    Game Game { get; }

    /// <summary>
    /// Gets whether the terminal has a live connection.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the number of lines in the main terminal.
    /// </summary>
    int LineCount { get; }

    /// <summary>
    /// Returns the last <paramref name="count"/> lines of the main terminal, oldest first.
    /// </summary>
    IReadOnlyList<string> LastLines(int count);

    /// <summary>
    /// Returns the newest nonblank line of the main terminal.
    /// </summary>
    string LastNonEmptyLine();

    /// <summary>
    /// Scrolls the main terminal to the newest output.
    /// </summary>
    void ScrollToEnd();

    /// <summary>
    /// Replaces the last or all occurrences of text in the main terminal.
    /// </summary>
    void ReplaceText(string searchText, string replacementText, bool lastOnly);

    /// <summary>
    /// Removes a one-based line from the main terminal.
    /// </summary>
    void RemoveLine(int lineNumber);

    /// <summary>
    /// Removes trailing whitespace from the main terminal.
    /// </summary>
    void TrimEnd();

    /// <summary>
    /// Removes the last line of the main terminal.
    /// </summary>
    void RemoveLastLine();

    /// <summary>
    /// Clears the back buffer (the scroll back split).
    /// </summary>
    void ClearBackBuffer();

    /// <summary>
    /// Sets the status text shown on the main window's status bar.
    /// </summary>
    Task SetStatusAsync(string text, int milliseconds);

    /// <summary>
    /// Sets the title of the terminal's document tab.
    /// </summary>
    void SetTitle(string title);

    /// <summary>
    /// Shows an alert over the terminal.
    /// </summary>
    void ShowAlert(Alert alert);

    /// <summary>
    /// Closes every alert over the terminal.
    /// </summary>
    void CloseAlerts();

    /// <summary>
    /// Closes every alert with the given message.
    /// </summary>
    void RemoveAlertsByMessage(string message);

    /// <summary>
    /// Redraws the terminal, for example after highlight triggers change.
    /// </summary>
    void Redraw();

    /// <summary>
    /// Connects to the game if not already connected.
    /// </summary>
    Task<bool> ConnectAsync(bool executeOnConnected);

    /// <summary>
    /// Disconnects from the game.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Moves keyboard focus to the command input.
    /// </summary>
    void FocusInput();
}
