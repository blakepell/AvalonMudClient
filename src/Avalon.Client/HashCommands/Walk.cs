using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Speed walks a set of directions given in the speed walk format.
    /// </summary>
    public class Walk : HashCommand
    {
        public Walk(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#walk";

        public override string Description { get; } = "Speed walks a set of directions given in the speed walk format.";

        public override void Execute()
        {
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }

            // Parse the speed walk.
            string buf = Utilities.Utilities.Speedwalk(this.Parameters);

            // Send the speed walk.
            Interpreter.Send(buf);
        }

    }
}
