/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Input;

namespace Avalon.Controls
{
    /// <summary>
    /// The window controls, minimize, maximize, restore and close that a normal window should have.
    /// </summary>
    public partial class ChildWindowTitleBar
    {
        /// <summary>
        /// The title that should appear.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChildWindowTitleBar), new PropertyMetadata("Avalon Mud Client"));

        /// <summary>
        /// The width of all of the control buttons.
        /// </summary>
        public double ButtonWidth
        {
            get => Convert.ToDouble(GetValue(ButtonWidthProperty));
            set => SetValue(ButtonWidthProperty, value);
        }

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(ChildWindowTitleBar), new PropertyMetadata(50.0));

        /// <summary>
        /// The height of all of the control buttons.
        /// </summary>
        public double ButtonHeight
        {
            get => Convert.ToDouble(GetValue(ButtonHeightProperty));
            set => SetValue(ButtonHeightProperty, value);
        }

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(ChildWindowTitleBar), new PropertyMetadata(50.0));

        /// <summary>
        /// Whether or not the restore button should be shown.  This gets automatically toggled on or off to be the opposite
        /// of if the restore button is visible.
        /// </summary>
        public bool ShowMaximizeButton
        {
            get => Convert.ToBoolean(GetValue(ShowMaximizeButtonProperty));
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register(nameof(ShowMaximizeButton), typeof(bool), typeof(ChildWindowTitleBar), new PropertyMetadata(true));

        /// <summary>
        /// Whether or not the restore button should be shown.  This gets automatically toggled on or off to be the opposite
        /// of if the maximize button is visible.
        /// </summary>
        public bool ShowRestoreButton
        {
            get => Convert.ToBoolean(GetValue(ShowRestoreButtonProperty));
            set => SetValue(ShowRestoreButtonProperty, value);
        }

        public static readonly DependencyProperty ShowRestoreButtonProperty =
            DependencyProperty.Register(nameof(ShowRestoreButton), typeof(bool), typeof(ChildWindowTitleBar), new PropertyMetadata(false));

        /// <summary>
        /// The Symbol to display on the TabItem.
        /// </summary>
        public Symbol HeaderIcon
        {
            get => (Symbol)GetValue(HeaderIconProperty);
            set => SetValue(HeaderIconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderIconProperty =
            DependencyProperty.Register(nameof(HeaderIcon), typeof(Symbol), typeof(ChildWindowTitleBar), new PropertyMetadata(Symbol.NewWindow));

        /// <summary>
        /// Constructor
        /// </summary>
        public ChildWindowTitleBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Apply styles to our various parts.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_Minimize.SetResourceReference(StyleProperty, "WindowButton");
            this.PART_Maximize.SetResourceReference(StyleProperty, "WindowButton");
            this.PART_Restore.SetResourceReference(StyleProperty, "WindowButton");
            this.PART_Close.SetResourceReference(StyleProperty, "WindowButtonClose");
        }

        /// <summary>
        /// Allows for the moving of the window with the left mouse button if the area where the title bar
        /// would be is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.FindAscendant<Window>().DragMove();
            }
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Close_Click(object sender, RoutedEventArgs e)
        {
            var win = this.FindAscendant<Window>();
            SystemCommands.CloseWindow(win);
        }

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Maximize_Click(object sender, RoutedEventArgs e)
        {
            var win = this.FindAscendant<Window>();
            SystemCommands.MaximizeWindow(win);

            this.ShowMaximizeButton = false;
            this.ShowRestoreButton = true;
        }

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Minimize_Click(object sender, RoutedEventArgs e)
        {
            var win = this.FindAscendant<Window>();
            SystemCommands.MinimizeWindow(win);
        }

        /// <summary>
        /// Restore the window size to it's normal state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Restore_Click(object sender, RoutedEventArgs e)
        {
            var win = this.FindAscendant<Window>();
            SystemCommands.RestoreWindow(win);

            this.ShowMaximizeButton = true;
            this.ShowRestoreButton = false;
        }

    }
}
