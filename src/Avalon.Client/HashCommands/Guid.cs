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

        public override string Description { get; } = "Echos a unique guid as well as settings a variable named guid so that it can be used via scripts.";

        public override void Execute()
        {
            string guid = Guid.NewGuid().ToString();
            Interpreter.Conveyor.EchoText("\r\n");
            Interpreter.Conveyor.EchoLog(guid, Common.Models.LogType.Information);
            this.Interpreter.Conveyor.SetVariable("Guid", guid);
        }
    }
}
