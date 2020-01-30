using Avalon.Common.Colors;
using System.Linq;
using Argus.Extensions;
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
            string room = this.Interpreter.Conveyor.GetVariable("Room").ToLower();

            // First, search for a exact match location if it exists.
            var dest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => x.Name.ToLower() == this.Parameters.ToLower() && x.StartingRoom.ToLower() == room);

            // This is it, walk it and get out.
            if (dest != null)
            {
                // Parse the speedwalk and send it to the hash command.
                string buf = Utilities.Utilities.Speedwalk(dest.Speedwalk);
                Interpreter.Send(buf);
                return;
            }

            // Search for the destination room, whether we're in the starting room or not.  If we're not, we'll later search for a bridge room
            dest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => x.Name.ToLower() == this.Parameters.ToLower());

            if (dest == null)
            {
                Interpreter.EchoText($"--> Direction not found.", AnsiColors.Red);
                return;
            }

            // Starting room is correct, start walking
            if (dest != null && dest.StartingRoom.ToLower() == room.ToLower())
            {
                // Parse the speedwalk and send it to the hash command.
                string buf = Utilities.Utilities.Speedwalk(dest.Speedwalk);
                Interpreter.Send(buf);
                return;
            }

            // See if we can get to the starting room from here.
            var midDest = App.Settings.ProfileSettings.DirectionList.FirstOrDefault(x => x.Name.ToLower() == dest.StartingRoom.ToLower() && x.StartingRoom.ToLower() == room);

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

            Interpreter.EchoText($"--> Path not found from '{room.ToTitleCase()}' to '{dest.Name}'", AnsiColors.Red);

        }

    }
}
