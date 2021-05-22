/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.HashCommands;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Scans in all available directions
    /// </summary>
    public class ScanAll : HashCommand
    {
        public ScanAll(IInterpreter interp) : base (interp)
        {
        }

        public ScanAll()
        {

        }

        public override string Name { get; } = "#scan-all";

        public override string Description { get; } = "Scans in all available directions for the current room the user is in.";

        public override void Execute()
        {
            string exitStr = this.Interpreter.Conveyor.GetVariable("Exits");
            var exits = exitStr.Split(' ');

            foreach (string buf in exits)
            {
                if (string.Equals(buf, "none", System.StringComparison.OrdinalIgnoreCase) || buf.IsNullOrEmptyOrWhiteSpace())
                {
                    continue;
                }

                Interpreter.Send($"scan {buf.Replace("(", "").Replace(")", "")}");
            }
        }

    }
}