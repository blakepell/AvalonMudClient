using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Starts an executable
    /// </summary>
    public class ShellUrl : HashCommand
    {
        public ShellUrl(IInterpreter interp) : base (interp)
        {
            this.Interpreter = interp;
        }

        public override string Name { get; } = "#shell-url";

        public override string Description { get; } = "Starts an http or https URL.";

        public override void Execute()
        {
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                return;
            }

            if (!this.Parameters.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
            {
                this.Interpreter.Conveyor.EchoLog("URL must be 'http' or 'https'.", LogType.Warning);
                return;
            }

            if (!App.Settings.AvalonSettings.AllowShell)
            {
                this.Interpreter.Conveyor.EchoLog("Starting executables is currently disabled.  To enable it go into ", LogType.Warning);
                this.Interpreter.Conveyor.EchoLog("Tools -> Settings -> Client Settings -> AllowShell", LogType.Warning);
                return;
            }

            Utilities.Utilities.ShellLink(this.Parameters);
        }

    }
}
