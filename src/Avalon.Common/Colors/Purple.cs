namespace Avalon.Common.Colors
{
    public class Purple : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[1;35m";
        }

        public override string MudColorCode => "{M";

        public override string Name => "Purple";

    }
}