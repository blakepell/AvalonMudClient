using System.Windows.Media;
using Avalon.Common.Colors;

namespace Avalon.Colors
{
    /// <summary>
    /// Mapping class that contains the various forms that a color can manifest as.
    /// </summary>
    public class ColorMap
    {
        public AnsiColor AnsiColor { get; set; }

        public SolidColorBrush Brush { get; set; }
        
    }
}
