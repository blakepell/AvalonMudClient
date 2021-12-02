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
    /// Executes a speed walk command.
    /// </summary>
    public class Go : HashCommand
    {
        public Go(IInterpreter interp) : base (interp)
        {
            this.Interpreter = interp;
        }

        public override string Name { get; } = "#go";

        public override string Description { get; } = "Executes a set of directions/speed walk command.";

        public override void Execute()
        {
            // Get the room we are in.
            string room = this.Interpreter.Conveyor.GetVariable("Room");

            // First, search for a exact match location if it exists.
            var dest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => string.Equals(x.Name, this.Parameters, System.StringComparison.OrdinalIgnoreCase) && string.Equals(x.StartingRoom, room, System.StringComparison.OrdinalIgnoreCase));

            // This is it, walk it and get out.
            if (dest != null)
            {
                // Parse the speedwalk and send it to the hash command.
                string buf = Utilities.Utilities.Speedwalk(dest.Speedwalk);
                Interpreter.Send(buf);
                return;
            }

            // Search for the destination room, whether we're in the starting room or not.  If we're not, we'll later search for a bridge room
            dest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => string.Equals(x.Name, this.Parameters, System.StringComparison.OrdinalIgnoreCase));

            if (dest == null)
            {
                Interpreter.Conveyor.EchoError("Direction not found.");
                return;
            }

            // Starting room is correct, start walking
            if (string.Equals(dest.StartingRoom, room, System.StringComparison.OrdinalIgnoreCase))
            {
                // Parse the speed walk and send it to the hash command.
                Interpreter.Send(Utilities.Utilities.Speedwalk(dest.Speedwalk));
                return;
            }

            // See if we can get to the starting room from here.
            var midDest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => string.Equals(x.Name, dest.StartingRoom, System.StringComparison.OrdinalIgnoreCase) && string.Equals(x.StartingRoom, room, System.StringComparison.OrdinalIgnoreCase));

            // Walk to the starting room.
            if (midDest != null)
            {
                // First part
                string buf = Utilities.Utilities.Speedwalk(midDest.Speedwalk);
                Interpreter.Send(buf);

                // Second part.
                buf = Utilities.Utilities.Speedwalk(dest.Speedwalk);
                Interpreter.Send(buf);

                return;
            }

            Interpreter.Conveyor.EchoError($"Path not found from '{room.ToTitleCase()}' to '{dest.Name}'");
        }
    }
}
