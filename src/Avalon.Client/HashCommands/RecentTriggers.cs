/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common;
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
            var tb = new TableBuilder("Last Matched", "Trigger");

            foreach (var trigger in list)
            {
                //in this case n = 10 - adjust as needed
                tb.AddRow(trigger.LastMatched.ToString(), trigger.Pattern.Truncate(50, "..."));
            }

            this.Interpreter.Conveyor.EchoText(tb.ToString());
        }
    }
}
