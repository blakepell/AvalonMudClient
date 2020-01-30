namespace Avalon.Common.Colors
{
    public class Default : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0;37m";
        }

        public override string MudColorCode => "{w";

        public override string Name => "Default";
    }
}