using Argus.Extensions;
using Avalon.Common.Colors;
using System.Linq;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Executes a macro.
    /// </summary>
    public class Macro : HashCommand
    {
        public Macro(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#macro";

        public override string Description { get; } = "Executes a macro for the given key.";

        public override void Execute()
        {            
            var macro = App.Settings.ProfileSettings.MacroList.FirstOrDefault(x => x.KeyDescription.ToString() == this.Parameters);

            if (macro == null)
            {
                Interpreter.EchoText($"--> Macro for key '{this.Parameters}' not found.", AnsiColors.Red);
                return;
            }

            if (macro.Command.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.EchoText($"--> Macro key '{this.Parameters}' had a blank command.", AnsiColors.Red);
                Interpreter.EchoText($"");
                return;
            }

            // It was found, now see if it has a command, if it does, send it.
            if (!macro.Command.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Send(macro.Command);
            }

        }

    }
}
