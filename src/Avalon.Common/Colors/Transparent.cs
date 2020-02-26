namespace Avalon.Common.Colors
{
    public class Transparent : AnsiColor
    {
        //[39m - default foreground
        //[49m - default background
        public override string AnsiCode => "\x1B[1;31;49m";

        public override string MudColorCode => "";

        public override string Name => "Transparent";

    }
}