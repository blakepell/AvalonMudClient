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
    /// Speed walks a set of directions given in the speed walk format.
    /// </summary>
    public class Walk : HashCommand
    {
        public Walk(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#walk";

        public override string Description { get; } = "Speed walks a set of directions given in the speed walk format.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }

            // Send the speed walk.
            Interpreter.Send(Utilities.Utilities.Speedwalk(this.Parameters));
        }
    }
}
