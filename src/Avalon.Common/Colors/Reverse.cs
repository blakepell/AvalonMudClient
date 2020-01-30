namespace Avalon.Common.Colors
{
    public class Reverse : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[7m";
        }

        public override string MudColorCode => "{&";

        public override string Name => "Reverse";

    }
}