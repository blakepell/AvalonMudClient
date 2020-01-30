namespace Avalon.Common.Colors
{
    public class Transparent : AnsiColor
    {
        public override string ToString()
        {
            // TODO - Verify this is the right ANSI code.
            //[39m - default foreground
            //[49m - default background
            return "\x1B[1;31;49m";
        }

        public override string MudColorCode => "";

        public override string Name => "Transparent";

    }
}