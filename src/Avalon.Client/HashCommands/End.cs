using System.Windows;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Exits the application.
    /// </summary>
    public class End : HashCommand
    {
        public End(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#end";

        public override string Description { get; } = "Exits the application.";

        public override void Execute()
        {
            Application.Current.Shutdown(0);
        }

    }
}