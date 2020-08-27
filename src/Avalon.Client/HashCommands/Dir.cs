using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Shows the directions window.
    /// </summary>
    /// <remarks>
    /// This is being provided as a hash command so that it can be used in a macro other than
    /// the provided Control+D on the main window.
    /// </remarks>
    public class Dir : HashCommand
    {
        public Dir(IInterpreter interp) : base (interp)
        {            
        }

        public override string Name { get; } = "#dir";

        public override string Description { get; } = "Shows the quick directions window.";

        public override void Execute()
        {
            // Fire and forget.
            Utilities.WindowManager.ShellWindow("Directions Select");
        }
    }
}
