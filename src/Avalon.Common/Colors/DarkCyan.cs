namespace Avalon.Common.Colors
{
    public class DarkCyan : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;36m";
        }

        public override string MudColorCode => "{c";

        public override string Name => "Dark Cyan";

    }
}
