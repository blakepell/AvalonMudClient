namespace Avalon.Common.Colors
{
    public class Green : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;32m";

        public override string MudColorCode => "{G";

        public override string Name => "Green";
    }
}