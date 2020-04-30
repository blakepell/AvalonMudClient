using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Starts an executable
    /// </summary>
    public class Shell : HashCommand
    {
        public Shell(IInterpreter interp) : base (interp)
        {
            this.Interpreter = interp;
        }

        public override string Name { get; } = "#shell";

        public override string Description { get; } = "Starts an executable.";

        public override void Execute()
        {
            if (!App.Settings.AvalonSettings.AllowShell)
            {
                this.Interpreter.Conveyor.EchoLog("Starting executables is currently disabled.  To enable it go into ", LogType.Warning);
                this.Interpreter.Conveyor.EchoLog("Tools -> Settings -> Client Settings -> AllowShell", LogType.Warning);
                return;
            }

            Utilities.Utilities.Shell(this.Parameters);
        }

    }
}
