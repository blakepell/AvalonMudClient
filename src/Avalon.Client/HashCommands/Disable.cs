using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Disable all triggers or aliases globally (it does not change their individual settings).
    /// </summary>
    public class Disable : HashCommand
    {
        public Disable(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#disable";

        public override string Description { get; } = "Disables all aliases or triggers.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.EchoInfo("Syntax: #disable <alias|trigger>");
                return;
            }

            if (string.Equals(this.Parameters, "alias", System.StringComparison.OrdinalIgnoreCase))
            {
                App.Settings.ProfileSettings.AliasesEnabled = false;
                Interpreter.Conveyor.EchoSuccess("Aliases Disabled");
            }
            else if (string.Equals(this.Parameters, "trigger", System.StringComparison.OrdinalIgnoreCase))
            {
                App.Settings.ProfileSettings.TriggersEnabled = false;
                Interpreter.Conveyor.EchoSuccess("Triggers Disabled.");
            }
            else
            {
                Interpreter.Conveyor.EchoWarning("Syntax: #disable <alias|trigger>");
            }
        }
    }
}
