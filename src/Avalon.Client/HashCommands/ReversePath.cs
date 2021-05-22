/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Reverses a set of directions.
    /// </summary>
    public class ReverseDirections : HashCommand
    {
        public ReverseDirections(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#reverse-path";

        public override string Description { get; } = "Reverse's a fast walk path";

        public override void Execute()
        {
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                this.Interpreter.Conveyor.EchoLog("A set of fast walk direction is required as the parameter.", LogType.Warning);
                return;
            }

            string rev = Utilities.Utilities.SpeedwalkReverse(this.Parameters, true);

            this.Interpreter.Conveyor.EchoLog(rev, LogType.Information);
        }
    }
}
