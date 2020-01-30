namespace Avalon.Common.Colors
{
    public class Pink : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[38;5;205m";
        }

        public override string MudColorCode => "{p";

        public override string Name => "Pink";
    }
}