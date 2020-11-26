using Argus.Extensions;
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
                Interpreter.Conveyor.EchoInfo("Syntax: #group-enable <group name>");
                return;
            }

            bool found = this.Interpreter.Conveyor.EnableGroup(this.Parameters);

            if (found)
            {
                Interpreter.Conveyor.EchoSuccess($"Group '{this.Parameters}' enabled.");
                return;
            }

            Interpreter.Conveyor.EchoError($"Group '{this.Parameters}' was not found.");
        }

    }
}
