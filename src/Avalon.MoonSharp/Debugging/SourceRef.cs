namespace MoonSharp.Interpreter.Debugging
{
    /// <summary>
    /// Class representing a reference to source code interval
    /// </summary>
    public class SourceRef
    {
        public SourceRef(SourceRef src, bool isStepStop)
        {
            this.FromChar = src.FromChar;
            this.ToChar = src.ToChar;
            this.FromLine = src.FromLine;
            this.ToLine = src.ToLine;
            this.IsStepStop = isStepStop;
        }


        public SourceRef(int from, int to, int fromLine, int toLine, bool isStepStop)
        {
            this.FromChar = from;
            this.ToChar = to;
            this.FromLine = fromLine;
            this.ToLine = toLine;
            this.IsStepStop = isStepStop;
        }

        /// <summary>
        /// Gets a value indicating whether this location is inside CLR .
        /// </summary>
        public bool IsClrLocation { get; private set; }

        /// <summary>
        /// Gets from which column the source code ref starts
        /// </summary>
        public int FromChar { get; }

        /// <summary>
        /// Gets to which column the source code ref ends
        /// </summary>
        public int ToChar { get; }

        /// <summary>
        /// Gets from which line the source code ref starts
        /// </summary>
        public int FromLine { get; }

        /// <summary>
        /// Gets to which line the source code ref ends
        /// </summary>
        /// 
        public int ToLine { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a stop "step" in source mode
        /// </summary>
        public bool IsStepStop { get; }

        internal static SourceRef GetClrLocation()
        {
            return new SourceRef(0, 0, 0, 0, false) {IsClrLocation = true};
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{(IsStepStop ? "*" : " ")} ({FromLine.ToString()}, {FromChar.ToString()}) -> ({ToLine.ToString()}, {ToChar.ToString()})";
        }

        /// <summary>
        /// Formats the location according to script preferences
        /// </summary>
        /// <param name="script">The script.</param>
        public string FormatLocation(Script script)
        {
            if (IsClrLocation)
            {
                return "[clr]";
            }

            if (FromLine == ToLine)
            {
                if (FromChar == ToChar)
                {
                    return $"(Line {FromLine.ToString()}, Position {FromChar.ToString()})";
                }

                return $"(Line {FromLine.ToString()}, Position {FromChar.ToString()}-{ToChar.ToString()})";
            }

            return $"(Line {FromLine.ToString()}, Position {FromChar.ToString()}-{ToLine.ToString()},{ToChar.ToString()})";
        }
    }
}