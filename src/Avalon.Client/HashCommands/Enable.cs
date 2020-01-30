using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Enables all triggers or aliases globally (it does not change their individual settings).
    /// </summary>
    public class Enable : HashCommand
    {
        public Enable(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#enable";

        public override string Description { get; } = "Enables all triggers or aliases.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.EchoText("--> Syntax: #enable <alias|trigger>", AnsiColors.Red);
                return;
            }

            this.Parameters = this.Parameters.ToLower();

            if (this.Parameters == "alias")
            {
                App.Settings.ProfileSettings.AliasesEnabled = true;
                Interpreter.EchoText($"--> Aliases Enabled", AnsiColors.Cyan);
            }
            else if (this.Parameters == "trigger")
            {
                App.Settings.ProfileSettings.TriggersEnabled = true;
                Interpreter.EchoText($"--> Triggers Enabled", AnsiColors.Cyan);
            }
            else
            {
                Interpreter.EchoText("--> Syntax: #enable <alias|trigger>", AnsiColors.Red);
                return;
            }

        }

    }
}
