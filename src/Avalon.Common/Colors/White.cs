namespace Avalon.Common.Colors
{
    public class White : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;37m";

        public override string MudColorCode => "{W";

        public override string Name => "White";

    }
}