/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Starts an executable.
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
                this.Interpreter.Conveyor.EchoWarning("Starting executables is currently disabled.  To enable it go into ");
                this.Interpreter.Conveyor.EchoWarning("Tools -> Settings -> Client Settings -> AllowShell");
                return;
            }

            Utilities.Utilities.Shell(this.Parameters);
        }
    }
}
