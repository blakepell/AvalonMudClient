using Argus.Extensions;
using Avalon.Common.Colors;
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
                Interpreter.EchoText("--> Syntax: #disable <alias|trigger>", AnsiColors.Red);
                return;
            }

            this.Parameters = this.Parameters.ToLower();

            if (this.Parameters == "alias")
            {
                App.Settings.ProfileSettings.AliasesEnabled = false;
                Interpreter.EchoText($"--> Aliases Disabled", AnsiColors.Cyan);
            }
            else if (this.Parameters == "trigger")
            {
                App.Settings.ProfileSettings.TriggersEnabled = false;
                Interpreter.EchoText($"--> Triggers Disable", AnsiColors.Cyan);
            }
            else
            {
                Interpreter.EchoText("--> Syntax: #disable <alias|trigger>", AnsiColors.Red);
                return;
            }

        }

    }
}
