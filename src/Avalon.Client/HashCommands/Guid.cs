using System;
using System.Reflection;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Echos a unique Guid.
    /// </summary>
    /// <remarks>
    /// Named as to not conflict with System.Guid.
    /// </remarks>
    public class GuidHashCmd : HashCommand
    {
        public GuidHashCmd(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#guid";

        public override string Description { get; } = "Echos a unique guid.";

        public override void Execute()
        {
            Interpreter.Conveyor.EchoText("\r\n");
            Interpreter.Conveyor.EchoLog(Guid.NewGuid().ToString(), Common.Models.LogType.Information);
        }

    }
}
