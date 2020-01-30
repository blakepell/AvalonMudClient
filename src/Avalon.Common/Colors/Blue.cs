namespace Avalon.Common.Colors
{
    public class Blue : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[1;34m";
        }

        public override string MudColorCode => "{B";

        public override string Name => "Blue";
    }
}