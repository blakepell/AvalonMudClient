/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows;

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
            get => (int)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                this.Visibility = value == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(Badge), new PropertyMetadata(0));

        /// <summary>
        /// Increments the value if the expression is true, sets the value to 0 if false.
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
