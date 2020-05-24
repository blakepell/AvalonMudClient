using Avalon.Common.Interfaces;
using Avalon.HashCommands;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Tries to open doors in all directions.
    /// </summary>
    public class OpenAll : HashCommand
    {
        public OpenAll(IInterpreter interp) : base (interp)
        {
        }

        public OpenAll()
        {

        }

        public override string Name { get; } = "#open-all";

        public override string Description { get; } = "Tries to open doors in all directions.";

        public override void Execute()
        {
            Interpreter.Send("open north;open south;open east;open west;open northeast;open northwest;open southwest;open southeast;open up;open down");
        }

    }
}