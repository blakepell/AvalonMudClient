using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Sets the prompt required by the mud client in order to read out information from the game.
    /// </summary>
    public class SetPrompt : HashCommand
    {
        public SetPrompt(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#set-prompt";

        public override string Description { get; } = "Sets the prompt required by the mud client in order to read out information from the game.";

        public override void Execute()
        {
            Interpreter.Send("prompt <%h/%Hhp %m/%Mm %v/%Vmv (%y|%S) ({g%r{x) ({C%e{x) %X %g %s %q %l %o %O %t>%c");
        }

    }
}