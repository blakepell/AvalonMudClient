using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Executes an alias.  The main purpose of this is to be able to short hand lookup aliases
    /// from the autocomplete box.  This will basically just send the alias text to the interpreter.
    /// </summary>
    public class Alias : HashCommand
    {
        public Alias(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#a";

        public override string Description { get; } = "Executes an alias.";

        public override void Execute()
        {
            Interpreter.Send(this.Parameters);
        }

    }
}