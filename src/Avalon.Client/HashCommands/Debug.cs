using Avalon.Common.Interfaces;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Command used for debugging.
    /// </summary>
    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Command used for developer debugging.";

        public override void Execute()
        {
            Interpreter.Conveyor.EchoLog($"Window Dimensions: {App.MainWindow.Width}x{App.MainWindow.Height}", LogType.Debug);
        }

    }
}
