using Avalon.Common.Colors;
using System.Collections.Generic;
using System.Linq;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos all of the hash commands that are available for use.
    /// </summary>
    public class CmdList : HashCommand
    {
        public CmdList(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#cmd-list";

        public override string Description { get; } = "Echos all of the hash commands that are available for use.";


        public override void Execute()
        {
            var list = new List<string>();

            foreach (var hc in this.Interpreter.HashCommands)
            {
                list.Add(hc.Name);
            }

            list = list.OrderBy(p => p).ToList();

            foreach (string item in list)
            {
                this.Interpreter.EchoText(item, AnsiColors.Cyan);
            }

            this.Interpreter.EchoText("");
        }

    }
}