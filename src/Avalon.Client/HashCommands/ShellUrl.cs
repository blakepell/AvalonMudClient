/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

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
                this.Interpreter.Conveyor.EchoWarning("URL must be 'http' or 'https'.");
                return;
            }

            if (!App.Settings.AvalonSettings.AllowShell)
            {
                this.Interpreter.Conveyor.EchoWarning("Starting executables is currently disabled.  To enable it go into ");
                this.Interpreter.Conveyor.EchoWarning("Tools -> Settings -> Client Settings -> AllowShell");
                return;
            }

            Utilities.Utilities.ShellLink(this.Parameters);
        }

    }
}
