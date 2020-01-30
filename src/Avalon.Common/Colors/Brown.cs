namespace Avalon.Common.Colors
{
    public class Brown : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[38;5;130m";
        }

        public override string MudColorCode => "{n";

        public override string Name => "Brown";
    }
}