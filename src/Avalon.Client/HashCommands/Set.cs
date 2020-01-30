using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Sets a variable
    /// </summary>
    public class Set : HashCommand
    {
        public Set(IInterpreter interp) : base (interp)
        {
            this.Interpreter = interp;
        }

        public override string Name { get; } = "#set";

        public override string Description { get; } = "Sets a variable to a value.";

        public override void Execute()
        {
            var argOne = this.Parameters.FirstArgument();
            string argTwo = argOne.Item2;

            if (!argOne.Item1.IsNullOrEmptyOrWhiteSpace())
            {
                this.Interpreter.Conveyor.SetVariable(argOne.Item1, argTwo ?? "");
            }
            else
            {
                Interpreter.EchoText("");
                Interpreter.EchoText("--> Invalid syntax for #set", AnsiColors.Red);
                Interpreter.EchoText("");
            }
        }

    }
}
