namespace Avalon.Common.Colors
{
    public class DarkGreen : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;32m";
        }

        public override string MudColorCode => "{g";

        public override string Name => "Dark Green";
    }
}