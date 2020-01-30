using Avalon.Common.Interfaces;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Clears the main terminal window.
    /// </summary>
    public class Clear : HashCommand
    {
        public Clear(IInterpreter interp) : base(interp)
        {
        }


        public override string Name { get; } = "#clear";

        public override string Description { get; } = "Clears the main terminal window.";

        public override void Execute()
        {
            Interpreter.Conveyor.ClearTerminal(TerminalTarget.Main);
        }

    }
}
