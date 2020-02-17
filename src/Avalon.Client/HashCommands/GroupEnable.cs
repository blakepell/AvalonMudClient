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

            bool found = this.Interpreter.Conveyor.EnableGroup(this.Parameters);

            if (found)
            {
                Interpreter.Conveyor.EchoLog($"Group '{this.Parameters}' enabled.", Common.Models.LogType.Information);
                return;
            }

            Interpreter.Conveyor.EchoLog($"Group '{this.Parameters}' was not found", Common.Models.LogType.Information);
            return;
        }

    }
}
