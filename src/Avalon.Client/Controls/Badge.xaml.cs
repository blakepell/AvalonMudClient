using System;
using System.Windows;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge
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
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(Badge), new PropertyMetadata(0));

        /// <summary>
        /// Incriments the value if the expression is true, sets the value to 0 if false.
        /// </summary>
        /// <param name="expression"></param>
        public void IncrementOrReset(bool expression)
        {
            if (expression)
            {
                this.Value += 1;
            }
            else
            {
                this.Value = 0;
            }
        }

        public Badge()
        {
            InitializeComponent();
            this.Value = 0;
        }
    }
}
