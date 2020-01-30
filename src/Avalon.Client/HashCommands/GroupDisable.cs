using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Disables all triggers and aliases of a specified group.
    /// </summary>
    public class GroupDisable : HashCommand
    {
        public GroupDisable(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#group-disable";

        public override string Description { get; } = "Disables all triggers and aliases of a specified group.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.EchoText("--> Syntax: #group-disable <group name>", AnsiColors.Red);
                return;
            }

            bool found = false;
            this.Parameters = this.Parameters.ToLower();

            foreach (var item in App.Settings.ProfileSettings.TriggerList)
            {
                if (item.Group.ToLower() == this.Parameters)
                {
                    found = true;
                    item.Enabled = false;
                }
            }

            foreach (var item in App.Settings.ProfileSettings.AliasList)
            {
                if (item.Group.ToLower() == this.Parameters)
                {
                    found = true;
                    item.Enabled = false;
                }
            }

            if (found)
            {
                Interpreter.EchoText($"--> Group '{this.Parameters}' disabled.", AnsiColors.Cyan);
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
