using Argus.Extensions;
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

            if (!argOne.Item1.IsNullOrEmptyOrWhiteSpace())
            {
                this.Interpreter.Conveyor.SetVariable(argOne.Item1, argOne.Item2 ?? "");
            }
            else
            {
                Interpreter.Conveyor.EchoError("Syntax: #set <variable> <value>");
            }
        }
    }
}
