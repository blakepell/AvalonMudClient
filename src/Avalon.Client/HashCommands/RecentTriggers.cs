using System;
using System.Linq;
using Avalon.Common.Interfaces;
using Argus.Extensions;
using Avalon.Common.Colors;

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
            var list = this.Interpreter.Conveyor.ProfileSettings.TriggerList.Where(x => x.LastMatched > DateTime.MinValue).OrderByDescending(x => x.LastMatched).Take(50);

            this.Interpreter.EchoText("");
            
            foreach (var trigger in list)
            {
                this.Interpreter.EchoText($"{trigger.LastMatched}: {trigger.Pattern}", new Cyan());
            }
        }

    }
}
