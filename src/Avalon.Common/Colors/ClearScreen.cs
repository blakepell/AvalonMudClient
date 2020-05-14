namespace Avalon.Common.Colors
{
    public class ClearScreen : AnsiColor
    {
        public override string AnsiCode => "\x1B[2J";

        public override string MudColorCode => "";

        public override string Name => "ClearScreen";

    }
}