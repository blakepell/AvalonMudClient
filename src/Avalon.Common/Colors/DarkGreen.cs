namespace Avalon.Common.Colors
{
    public class DarkGreen : AnsiColor
    {
        public override string AnsiCode => "\x1B[0;32m";

        public override string MudColorCode => "{g";

        public override string Name => "Dark Green";
    }
}