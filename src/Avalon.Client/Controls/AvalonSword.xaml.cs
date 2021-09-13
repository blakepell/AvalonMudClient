using System.Windows;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for AvalonSword.xaml
    /// </summary>
    public partial class AvalonSword
    {
        public AvalonSword()
        {
            InitializeComponent();
            //this.DataContext = this;
        }

        public static readonly DependencyProperty ViewBoxWidthProperty = DependencyProperty.Register(
            "ViewBoxWidth", typeof(double), typeof(AvalonSword), new PropertyMetadata(96.0));

        public double ViewBoxWidth
        {
            get => (double) GetValue(ViewBoxWidthProperty);
            set => SetValue(ViewBoxWidthProperty, value);
        }

        public static readonly DependencyProperty ViewBoxHeightProperty = DependencyProperty.Register(
            nameof(ViewBoxHeight), typeof(double), typeof(AvalonSword), new PropertyMetadata(96.0));

        public double ViewBoxHeight
        {
            get => (double) GetValue(ViewBoxHeightProperty);
            set => SetValue(ViewBoxHeightProperty, value);
        }

        public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register(
            nameof(AccentColor), typeof(SolidColorBrush), typeof(AvalonSword), new PropertyMetadata(Brushes.Green));

        public SolidColorBrush AccentColor
        {
            get => (SolidColorBrush) GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }
    }
}