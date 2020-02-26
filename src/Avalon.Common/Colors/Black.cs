namespace Avalon.Common.Colors
{
    public class Black : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;30m";

        public override string MudColorCode => "";

        public override string Name => "Black";

    }
}