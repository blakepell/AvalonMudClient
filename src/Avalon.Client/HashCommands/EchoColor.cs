using Argus.Extensions;
using Avalon.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends.
    /// </summary>
    public class EchoColor : HashCommand
    {
        public EchoColor(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#echo-color";

        public override string Description { get; } = "Echos back the a colored parameter to the user.";

        public override void Execute()
        {
            var argOne = this.Parameters.FirstArgument();

            var foreground = Colorizer.ColorMapByName(argOne.Item1);

            if (foreground != null)
            {
                Interpreter.EchoText(argOne.Item2, foreground.AnsiColor, false);
            }
            else
            {
                Interpreter.EchoText($"{Parameters}");
            }
        }

    }
}
