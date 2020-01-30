using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Scans in all available directions
    /// </summary>
    public class ScanAll : HashCommand
    {
        public ScanAll(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#scan-all";

        public override string Description { get; } = "Scans in all available directions for the current room the user is in.";

        public override void Execute()
        {
            string exitStr = this.Interpreter.Conveyor.GetVariable("Exits");
            var exits = exitStr.Split(' ');

            foreach (string buf in exits)
            {
                if (buf.ToLower() == "none" || buf.IsNullOrEmptyOrWhiteSpace())
                {
                    continue;
                }

                Interpreter.Send($"scan {buf.Replace("(", "").Replace(")", "")}");
            }
        }

    }
}