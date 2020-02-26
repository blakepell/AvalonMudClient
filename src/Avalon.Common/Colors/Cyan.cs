namespace Avalon.Common.Colors
{
    public class Cyan : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;36m";

        public override string MudColorCode => "{C";

        public override string Name => "Cyan";
    }
}
