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
        public GroupDisable(IInterpreter interp) : base(interp)
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

            bool found = this.Interpreter.Conveyor.DisableGroup(this.Parameters);

            if (found)
            {
                Interpreter.Conveyor.EchoLog($"Group '{this.Parameters}' disabled.", Common.Models.LogType.Information);
                return;
            }

            Interpreter.Conveyor.EchoLog($"Group '{this.Parameters}' was not found", Common.Models.LogType.Information);
            return;
        }

    }
}
