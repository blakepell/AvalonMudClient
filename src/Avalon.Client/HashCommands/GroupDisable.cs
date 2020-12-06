using Argus.Extensions;
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
                Interpreter.Conveyor.EchoInfo("Syntax: #group-disable <group name>");
                return;
            }

            bool found = this.Interpreter.Conveyor.DisableGroup(this.Parameters);

            if (found)
            {
                Interpreter.Conveyor.EchoSuccess($"Group '{this.Parameters}' disabled.");
                return;
            }

            Interpreter.Conveyor.EchoError($"Group '{this.Parameters}' was not found");
        }
    }
}
