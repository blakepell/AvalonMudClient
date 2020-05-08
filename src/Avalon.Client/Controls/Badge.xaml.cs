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
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);

                // Do we automatically hide or show ourselves?
                if (string.IsNullOrWhiteSpace(value) || value.Equals("0", StringComparison.Ordinal))
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                }
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Badge), new PropertyMetadata("0"));

        public Badge()
        {
            InitializeComponent();
            this.Text = "0";
        }
    }
}
