namespace Avalon.Common.Colors
{
    public class Orange : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[38;5;166m";
        }

        public override string MudColorCode => "{o";

        public override string Name => "Orange";
    }
}