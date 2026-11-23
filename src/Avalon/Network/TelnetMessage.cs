namespace Avalon.Network;

/// <summary>
/// Represents an item in the ordered stream received from a Telnet server.
/// </summary>
public abstract record TelnetMessage
{
    /// <summary>
    /// Carries decoded text, which can end in the middle of a line.
    /// </summary>
    /// <param name="Value">The decoded text.</param>
    public sealed record Text(string Value) : TelnetMessage;

    /// <summary>
    /// Marks a prompt boundary signaled by Telnet or inferred when unterminated text stops arriving.
    /// </summary>
    public sealed record Prompt : TelnetMessage;

    /// <summary>
    /// Indicates a change in whether the server controls input echo.
    /// </summary>
    /// <param name="Enabled">Whether the server has enabled its echo option.</param>
    public sealed record ServerEcho(bool Enabled) : TelnetMessage;

    /// <summary>
    /// Carries a GMCP package received from the server.
    /// </summary>
    /// <param name="Message">The received package.</param>
    public sealed record Gmcp(GmcpMessage Message) : TelnetMessage;
}
