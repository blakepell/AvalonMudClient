namespace Avalon.Common.Colors
{
    public class DarkRed : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;31m";
        }

        public override string MudColorCode => "{r";

        public override string Name => "Dark Red";
    }
}
