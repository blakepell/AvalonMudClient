using Avalon.Common.Interfaces;
using Avalon.Utilities;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos an example header.
    /// </summary>
    public class Example : HashCommand
    {
        public Example(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#example";

        public override string Description { get; } = "Echos a header indicating an example alias/trigger is running.";

        public override void Execute()
        {
            Interpreter.Conveyor.EchoText($"{Ascii.Example}");
        }
    }
}
