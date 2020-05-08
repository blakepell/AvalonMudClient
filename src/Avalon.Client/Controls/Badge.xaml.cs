using System;
using System.Windows;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge : UserControl
    {


        /// <summary>
        /// The text to display in the badge notification.
        /// </summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

                // Do we automatically hide or show ourselves?
                if (value == 0)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                }
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(Badge), new PropertyMetadata(0));

        public Badge()
        {
            InitializeComponent();
            this.Value = 0;
        }
    }
}
