using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Whether the client has a TCP/IP connection open.
    /// </summary>
    /// <remarks>
    /// This can determine if a connection is open or closed (if a connection has broken but the disconnect event hasn't fired).
    /// </remarks>
    public class IsConnected : HashCommand
    {
        public IsConnected(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#is-connected";

        public override string Description { get; } = "Whether the client currently has an open TCP/IP connection.";

        public override void Execute()
        {
            this.Interpreter.Conveyor.EchoText(this.Interpreter.Telnet.IsConnected().ToString());
        }

    }
}