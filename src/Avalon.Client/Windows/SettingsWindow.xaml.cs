﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using System.Windows;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for global client settings.
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Code that runs when the settings form is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProfilePropertyGrid.SelectedObject = App.Settings.ProfileSettings;
            ClientPropertyGrid.SelectedObject = App.Settings.AvalonSettings;
            TextSettingsFilename.Text = App.Settings.AvalonSettings.LastLoadedProfilePath;
        }

        /// <summary>
        /// Closes the settings form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            App.MainWindow.UpdateUISettings();
            this.Close();
        }

        private void TabSettings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TabProfileSettings.IsSelected)
            {
                TextSettingsFilename.Text = App.Settings.AvalonSettings.LastLoadedProfilePath;
            }
            else if (TabClientSettings.IsSelected)
            {
                TextSettingsFilename.Text = App.Settings.AvalonSettingsFile;
            }

        }
    }
}
