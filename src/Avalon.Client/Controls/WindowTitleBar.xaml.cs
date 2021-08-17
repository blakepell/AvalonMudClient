/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using ModernWpf;
using System;
using System.Windows;
using System.Windows.Input;

namespace Avalon.Controls
{
    /// <summary>
    /// The window controls, minimize, maximize, restore and close that a normal window should have.
    /// </summary>
    public partial class WindowTitleBar
    {

        /// <summary>
        /// Represents the parent window which will be found once and then cached for later usage.
        /// </summary>
        public MainWindow ParentWindow { get; set; }

        /// <summary>
        /// The title that should appear.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(WindowTitleBar), new PropertyMetadata("Avalon Mud Client"));

        /// <summary>
        /// The width of all of the control buttons.
        /// </summary>
        public double ButtonWidth
        {
            get => Convert.ToDouble(GetValue(ButtonWidthProperty));
            set => SetValue(ButtonWidthProperty, value);
        }

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(WindowTitleBar), new PropertyMetadata(50.0));

        /// <summary>
        /// The height of all of the control buttons.
        /// </summary>
        public double ButtonHeight
        {
            get => Convert.ToDouble(GetValue(ButtonHeightProperty));
            set => SetValue(ButtonHeightProperty, value);
        }

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(WindowTitleBar), new PropertyMetadata(50.0));

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
            DependencyProperty.Register(nameof(ShowMaximizeButton), typeof(bool), typeof(WindowTitleBar), new PropertyMetadata(true));

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
            DependencyProperty.Register(nameof(ShowRestoreButton), typeof(bool), typeof(WindowTitleBar), new PropertyMetadata(false));

        /// <summary>
        /// Whether or not the NetworkButton is showing the game as being connected.
        /// </summary>
        public bool IsConnected
        {
            get => (bool)GetValue(IsConnectedProperty);
            set => SetValue(IsConnectedProperty, value);
        }

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register(nameof(IsConnected), typeof(bool), typeof(WindowTitleBar), new PropertyMetadata(false));

        /// <summary>
        /// Constructor
        /// </summary>
        public WindowTitleBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Apply styles to our various parts.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_Settings.SetResourceReference(StyleProperty, "WindowButton");
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
                this.ParentWindow.DragMove();
            }
        }

        /// <summary>
        /// Toggles the opening and closing of the split view menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_SplitViewOpen_Click(object sender, RoutedEventArgs e)
        {
            this.ParentWindow.SplitViewMain.IsPaneOpen = !this.ParentWindow.SplitViewMain.IsPaneOpen;
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Close_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this.ParentWindow);
        }

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Maximize_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this.ParentWindow);

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
            SystemCommands.MinimizeWindow(this.ParentWindow);
        }

        /// <summary>
        /// Restore the window size to it's normal state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Restore_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this.ParentWindow);

            this.ShowMaximizeButton = true;
            this.ShowRestoreButton = false;
        }

        /// <summary>
        /// Shows the client settings window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWin = new SettingsWindow();
            this.ParentWindow.ShowDialog(settingsWin);
        }

        /// <summary>
        /// Connects the mud client to the network.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PART_Network_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.IsConnected == false)
                {
                    await this.ParentWindow.Connect();
                }
                else
                {
                    this.ParentWindow.Disconnect();
                }
            }
            catch (Exception ex)
            {
                this.ParentWindow.Interp.Conveyor.EchoLog($"Network Failure: {ex.Message}", LogType.Error);
            }
        }

        /// <summary>
        /// Updates the buttons on the UI to ensure the current ones are shown.
        /// </summary>
        public void UpdateUI()
        {
            if (this.ParentWindow == null)
            {
                this.ParentWindow = this.FindAscendant<MainWindow>();
            }

            if (this.ParentWindow.WindowState == WindowState.Maximized)
            {
                this.ShowMaximizeButton = false;
                this.ShowRestoreButton = true;
            }
            else if (this.ParentWindow.WindowState == WindowState.Normal)
            {
                this.ShowMaximizeButton = true;
                this.ShowRestoreButton = false;
            }
        }
    }
}