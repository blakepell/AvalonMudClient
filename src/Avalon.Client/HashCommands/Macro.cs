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
    /// Executes a macro.
    /// </summary>
    public class Macro : HashCommand
    {
        public Macro(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#macro";

        public override string Description { get; } = "Executes a macro for the given key.";

        public override void Execute()
        {            
            var macro = App.Settings.ProfileSettings.MacroList.FirstOrDefault(x => x.KeyDescription == this.Parameters);

            if (macro == null)
            {
                Interpreter.Conveyor.EchoError($"Macro for key '{this.Parameters}' not found.");
                return;
            }

            if (macro.Command.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.EchoError($"--> Macro key '{this.Parameters}' had a blank command.\r\n");
                return;
            }

            // It was found, now see if it has a command, if it does, send it.
            if (!macro.Command.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Send(macro.Command);
            }
        }
    }
}
