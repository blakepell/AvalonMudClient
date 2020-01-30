namespace Avalon.Common.Colors
{
    public class Underline : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[4m";
        }

        public override string MudColorCode => "{_";

        public override string Name => "Underline";

    }
}