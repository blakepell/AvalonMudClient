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
            var sb = Argus.Memory.StringBuilderPool.Take();

            foreach (var hc in this.Interpreter.HashCommands)
            {
                list.Add(hc.Name);
            }

            list = list.OrderBy(p => p).ToList();
            int i = 0;

            sb.Append("\r\n{C");

            foreach (string item in list)
            {
                i++;
                sb.AppendFormat($"{item,-25}");

                // Line break every 4 items
                if (i.IsInterval(3))
                {
                    sb.Append("\r\n{C");
                    i = 0;
                }
            }

            sb.Append("{x\r\n");
            this.Interpreter.EchoText(sb);

            Argus.Memory.StringBuilderPool.Return(sb);
        }

    }
}