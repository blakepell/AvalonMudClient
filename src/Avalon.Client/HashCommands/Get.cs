﻿/*
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
    /// Gets a variable.
    /// </summary>
    public class Get : HashCommand
    {
        public Get(IInterpreter interp) : base (interp)
        {
            this.Interpreter = interp;
        }

        public override string Name { get; } = "#get";

        public override string Description { get; } = "Gets a variable to a value.";

        public override void Execute()
        {
            string temp = this.Interpreter.Conveyor.GetVariable(this.Parameters);
            Interpreter.Conveyor.EchoInfo(temp);
        }
    }
}
