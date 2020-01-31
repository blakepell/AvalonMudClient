using Avalon.Common.Colors;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Argus.Extensions;
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
            var sb = new StringBuilder();

            foreach (var hc in this.Interpreter.HashCommands)
            {
                list.Add(hc.Name);
            }

            list = list.OrderBy(p => p).ToList();
            int i = 0;

            this.Interpreter.EchoText("");

            foreach (string item in list)
            {
                i++;
                sb.AppendFormat($"{item,-25}");

                // Line break every 4 items
                if (i.IsInterval(3))
                {
                    this.Interpreter.EchoText(sb.ToString(), AnsiColors.Cyan);
                    sb.Clear();
                    i = 0;
                }
            }

            // Whatever was left.
            this.Interpreter.EchoText($"{sb,-25}", AnsiColors.Cyan);
            this.Interpreter.EchoText("");
        }

    }
}