namespace Avalon.Common.Colors
{
    public class Magenta : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[38;5;61m";
        }

        public override string MudColorCode => "{u";

        public override string Name => "Magenta";
    }
}