using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Sets the title of the window.
    /// </summary>
    public class SetTitle : HashCommand
    {
        public SetTitle(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#set-title";

        public override string Description { get; } = "Sets the title of the window to the specified value.";

        public override void Execute()
        {
            if (!this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                this.Interpreter.Conveyor.Title = this.Parameters;
            }
            else
            {
                Interpreter.EchoText("");
                Interpreter.EchoText("--> Invalid syntax for #set-title", AnsiColors.Red);
                Interpreter.EchoText("");
            }
        }

    }
}
