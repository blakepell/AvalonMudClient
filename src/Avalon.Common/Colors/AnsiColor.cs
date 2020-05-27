using System;

namespace Avalon.Common.Colors
{

    /// <summary>
    /// ANSI color abstract class.
    /// </summary>
    public abstract class AnsiColor
    {
        public static implicit operator string(AnsiColor c) => c.ToString();

        public override string ToString()
        {
            return this.AnsiCode;
        }

        public ReadOnlySpan<char> AsSpan()
        {
            return this.AnsiCode.AsSpan();
        }

        public abstract string AnsiCode { get; }

        public abstract string MudColorCode { get; }

        public abstract string Name { get;  }

    }
}
