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
    /// Enables all triggers or aliases globally (it does not change their individual settings).
    /// </summary>
    public class Enable : HashCommand
    {
        public Enable(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#enable";

        public override string Description { get; } = "Enables all triggers or aliases.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.EchoInfo("Syntax: #enable <alias|trigger>");
                return;
            }

            if (string.Equals(this.Parameters, "alias", System.StringComparison.OrdinalIgnoreCase))
            {
                App.Settings.ProfileSettings.AliasesEnabled = true;
                Interpreter.Conveyor.EchoSuccess(("Aliases enabled"));
            }
            else if (string.Equals(this.Parameters, "trigger", System.StringComparison.OrdinalIgnoreCase))
            {
                App.Settings.ProfileSettings.TriggersEnabled = true;
                Interpreter.Conveyor.EchoSuccess(("Triggers enabled"));
            }
            else
            {
                Interpreter.Conveyor.EchoWarning("Syntax: #enable <alias|trigger>");
            }

        }
    }
}
