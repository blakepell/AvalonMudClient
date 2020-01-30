namespace Avalon.Common.Colors
{

    /// <summary>
    /// ANSI color abstract class.
    /// </summary>
    public abstract class AnsiColor
    {
        public static implicit operator string(AnsiColor c) => c.ToString();

        public abstract override string ToString();

        public abstract string MudColorCode { get; }

        public abstract string Name { get;  }

    }
}
