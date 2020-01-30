namespace Avalon.Common.Colors
{
    public class DarkPurple : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;35m";
        }

        public override string MudColorCode => "{m";

        public override string Name => "Dark Purple";
    }
}