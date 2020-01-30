using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends.
    /// </summary>
    public class Echo : HashCommand
    {
        public Echo(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#echo";

        public override string Description { get; } = "Echos back the full parameter string the caller passes in.";

        public override void Execute()
        {
            Interpreter.EchoText($"{Parameters}");
        }

    }
}
