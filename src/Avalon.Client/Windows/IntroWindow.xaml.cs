/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Avalon
{
    public partial class IntroWindow
    {
        //public ObservableCollection<Profile> Profiles { get; set; }

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(IntroWindow), new PropertyMetadata(default(string)));

        public string Version
        {
            get => (string) GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public IntroWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            //this.Profiles = new();
            //this.Profiles.Add(new Profile() {GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Mortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 8000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
        }

        private void IntroWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Version = $"Version {version.Major.ToString()}.{version.Minor.ToString()}.{version.Revision}.{version.Build.ToString()}";
        }

        private async void MenuItemOpenProfilesFolder_OnClick(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it mostly likely should)
            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                await WindowManager.MsgBox($"The profile folder was not found at:\r\n\r\n{App.Settings.AvalonSettings.SaveDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AvalonSettings.SaveDirectory);
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Open Directory Error");
            }
        }
    }
}