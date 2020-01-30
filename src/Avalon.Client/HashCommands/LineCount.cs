using Avalon.Common.Interfaces;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos the line count in the main terminal window.
    /// </summary>
    public class LineCount : HashCommand
    {
        public LineCount(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#line-count";

        public override string Description { get; } = "Echos the line count in the main terminal window.";

        public override void Execute()
        {
            string buf = $"Line Count: {Interpreter.Conveyor.LineCount(TerminalTarget.Main)}\r\n";
            Interpreter.EchoText(buf);
        }

    }
}
