namespace Avalon.Common.Colors
{
    public class DarkGray : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[1;30m";
        }

        public override string MudColorCode => "{D";

        public override string Name => "Dark Gray";

    }
}