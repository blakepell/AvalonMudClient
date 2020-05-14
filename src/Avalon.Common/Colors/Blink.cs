namespace Avalon.Common.Colors
{
    public class Blink : AnsiColor
    {
        public override string AnsiCode => "\x1B[5m";

        public override string MudColorCode => "{*";

        public override string Name => "Blink";

    }
}