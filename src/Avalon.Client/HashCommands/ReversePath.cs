using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

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
