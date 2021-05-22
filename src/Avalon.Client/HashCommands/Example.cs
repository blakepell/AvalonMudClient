/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Utilities;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos an example header.
    /// </summary>
    public class Example : HashCommand
    {
        public Example(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#example";

        public override string Description { get; } = "Echos a header indicating an example alias/trigger is running.";

        public override void Execute()
        {
            Interpreter.Conveyor.EchoText($"{Ascii.Example}");
        }
    }
}
