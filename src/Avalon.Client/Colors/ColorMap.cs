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

        private SolidColorBrush _solidColorBrush;

        /// <summary>
        /// The WPF SolidColorBrush the AnsiColor maps to.
        /// </summary>
        public SolidColorBrush Brush
        { 
            get
            {
                return _solidColorBrush;
            }
            set
            {
                _solidColorBrush = value;

                // So, anything created from the WPF Brushes as most of ours are already frozen.  If
                // new colors are used in the future we can lock them here so they are still performant.
                // However, if we allow these colors to be changed we will need to unlocked them first.
                if (!_solidColorBrush.IsFrozen && _solidColorBrush.CanFreeze)
                {
                    _solidColorBrush.Freeze();
                }
            }
        }
        
    }
}
