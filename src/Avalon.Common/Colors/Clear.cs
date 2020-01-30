namespace Avalon.Common.Colors
{
    public class Clear : AnsiColor
    {
        public override string ToString()
        {
            return "\x1B[0m";
        }

        public override string MudColorCode => "{x";

        public override string Name => "Clear";

    }
}