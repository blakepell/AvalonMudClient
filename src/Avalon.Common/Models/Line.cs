using Avalon.Common.Colors;

namespace Avalon.Common.Models
{
    /// <summary>
    /// Represents a line of text that goes into a terminal window.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// The raw text with formatting removed.  This is the text that triggers were processed against.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The formatted text which includes the ANSI color codes.
        /// </summary>
        public string FormattedText { get; set; }

        /// <summary>
        /// Whether or not the terminal window should ignore the last color tracking for this line.  This
        /// will allow lines to be inserted in but have the previous color from the last line still in
        /// place (for cases where triggers write to windows in certain colors/etc and you don't want to
        /// break what's been sent from the game).
        /// </summary>
        public bool IgnoreLastColor { get; set; }

        /// <summary>
        /// An override that will set the color for the entire line and ignore the formatted text.
        /// </summary>
        public AnsiColor ForegroundColor { get; set; }

        /// <summary>
        /// Whether or not the ANSI sequence to reverse colors should be sent with the line.
        /// </summary>
        public bool ReverseColors { get; set; } = false;

        /// <summary>
        /// Whether or not the terminal should scroll to the last line when this line is inserted.  Default value is true.
        /// </summary>
        public bool ScrollToLastLine { get; set; } = true;

    }
}
