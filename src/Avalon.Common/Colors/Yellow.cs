namespace Avalon.Common.Colors
{
    public class Yellow : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;33m";

        public override string MudColorCode => "{Y";

        public override string Name => "Yellow";
    }
}