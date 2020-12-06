using System;
using System.Linq;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Shows the most recent triggers that have matched successfully.
    /// </summary>
    public class RecentTriggers : HashCommand
    {
        public RecentTriggers(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#recent-triggers";

        public override string Description { get; } = "Shows the most recent triggers that have matched successfully.";

        public override void Execute()
        {
            var list = this.Interpreter.Conveyor.ProfileSettings.TriggerList.Where(x => x.LastMatched > DateTime.MinValue).OrderByDescending(x => x.LastMatched).Take(25);

            var sb = Argus.Memory.StringBuilderPool.Take();


            sb.Append("\r\nThere are currently {y").Append(this.Interpreter.Conveyor.ProfileSettings.TriggerList.Count).Append("{x user triggers loaded.\r\n");
            sb.Append("There are currently {y").Append(App.SystemTriggers.Count).Append("{x system triggers loaded via plugin.\r\n\r\n");

            foreach (var trigger in list)
            {
                sb.Append("{C").Append(trigger.LastMatched).Append(":{x ").Append(trigger.Pattern).Append("\r\n");
            }

            this.Interpreter.Conveyor.EchoText(sb.ToString());
            Argus.Memory.StringBuilderPool.Return(sb);
        }

    }
}
