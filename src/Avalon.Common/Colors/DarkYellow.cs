namespace Avalon.Common.Colors
{
    public class DarkYellow : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;33m";
        }

        public override string MudColorCode => "{y";

        public override string Name => "Dark Yellow";
    }
}