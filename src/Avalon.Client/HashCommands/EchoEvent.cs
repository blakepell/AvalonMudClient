using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends in an easy to see colored event line.
    /// </summary>
    public class EchoEvent : HashCommand
    {
        public EchoEvent(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#echo-event";

        public override string Description { get; } = "Echos back the full parameter string the caller passes in an easy to see event line.";

        public override void Execute()
        {
            Interpreter.EchoText($"{Parameters}", AnsiColors.Cyan, true);
        }

    }
}