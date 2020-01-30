using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Enables all triggers and aliases of a specified group.
    /// </summary>
    public class GroupEnable : HashCommand
    {
        public GroupEnable(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#group-enable";

        public override string Description { get; } = "Enables all triggers and aliases of a specified group.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.EchoText("--> Syntax: #group-enable <group name>", AnsiColors.Red);
                return;
            }

            bool found = false;
            this.Parameters = this.Parameters.ToLower();

            foreach (var item in App.Settings.ProfileSettings.TriggerList)
            {
                if (item.Group.ToLower() == this.Parameters)
                {
                    found = true;
                    item.Enabled = true;
                }
            }

            foreach (var item in App.Settings.ProfileSettings.AliasList)
            {
                if (item.Group.ToLower() == this.Parameters)
                {
                    found = true;
                    item.Enabled = true;
                }
            }

            if (found)
            {
                Interpreter.EchoText($"--> Group '{this.Parameters}' enabled.", AnsiColors.Cyan);
                return;
            }
            else
            {
                Interpreter.EchoText($"--> Group '{this.Parameters}' was not found", AnsiColors.Red);
                return;
            }

        }

    }
}
