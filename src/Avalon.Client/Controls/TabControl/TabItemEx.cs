using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// Subclass of the TabItem capable of housing an Icon.
    /// </summary>
    public class TabItemEx : TabItem
    {

        public Symbol Icon
        {
            get => (Symbol)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(Symbol), typeof(TabItemEx), new PropertyMetadata(Symbol.Document));

    }
}
