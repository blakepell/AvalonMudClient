using System.Windows.Media;
using Avalon.Common.Colors;

namespace Avalon.Colors
{
    /// <summary>
    /// Mapping class that contains the various forms that a color can manifest as.
    /// </summary>
    public class ColorMap
    {
        /// <summary>
        /// The AnsiColor from the common library.
        /// </summary>
        public AnsiColor AnsiColor { get; set; }

        /// <summary>
        /// The WPF SolidColorBrush the AnsiColor maps to.
        /// </summary>
        public SolidColorBrush Brush { get; set; }
        
    }
}
