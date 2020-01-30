using Avalon.Common.Colors;
using System;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Force a garbage collection to run on the memory.
    /// </summary>
    public class Gc : HashCommand
    {
        public Gc(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#gc";

        public override string Description { get; } = "Force garbage collection to run on the memory.";

        public override void Execute()
        {
            GC.Collect();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            if (this.Parameters.ToLower() != "silent")
            {
                Interpreter.EchoText("Garbage Collection Executed.", AnsiColors.Cyan);
            }
        }

    }
}
