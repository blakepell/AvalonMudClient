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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Avalon.Common.Settings;
using ModernWpf.Controls;
using Newtonsoft.Json;

namespace Avalon
{
    public partial class IntroWindow : Window
    {
        //public ObservableCollection<Profile> Profiles { get; set; }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(IntroWindowViewModel), typeof(IntroWindow), new PropertyMetadata(default(IntroWindowViewModel)));

        public IntroWindowViewModel ViewModel
        {
            get => (IntroWindowViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public IntroWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.ViewModel = new IntroWindowViewModel
            {
                Profiles = new()
            };

            //this.Profiles = new();
            //this.Profiles.Add(new Profile() {GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 4000, GameName = "Dark and Shattered Lands", CustomDescription = "Mortal" });
            //this.Profiles.Add(new Profile() { GameAddress = "dsl-mud.org", GamePort = 8000, GameName = "Dark and Shattered Lands", CustomDescription = "Immortal" });
        }

        private void IntroWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.ViewModel.Version = $"Version {version.Major.ToString()}.{version.Minor.ToString()}.{version.Revision}.{version.Build.ToString()}";

            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                return;
            }

            var files = Directory.GetFiles(App.Settings.AvalonSettings.SaveDirectory, "*.json");

            foreach (string file in files)
            {
                var fi = new FileInfo(file);

                string json = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<ProfileSettings>(json);
                
                this.ViewModel.Profiles.Add(new()
                {
                    GameAddress = profile?.IpAddress ?? "Empty Game Address",
                    GamePort = profile?.Port ?? 0,
                    Filename = fi.Name,
                    LastSaveDate = fi.LastWriteTime
                });
            }


        }

        /// <summary>
        /// Opens the save/profiles folder which can be changed by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Opens the immutable client settings folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemOpenClientSettingsFolder_OnClick(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it mostly likely should)
            if (!Directory.Exists(App.Settings.AppDataDirectory))
            {
                await WindowManager.MsgBox($"The profile folder was not found at:\r\n\r\n{App.Settings.AppDataDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AppDataDirectory);
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Open Directory Error");
            }
        }

        /// <summary>
        /// Shells the GitHub issue tracking page users can use to report a problem with the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonReportBug_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Utilities.Utilities.ShellLink("https://github.com/blakepell/AvalonMudClient/issues");
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Report a Bug had a Bug.  Eek.");
            }
        }

        private async void GridViewProfiles_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is IntroWindowViewModel.ProfileViewModel vm)
            {
                var result = await WindowManager.InputBox($"Would you like to connect to {vm.GameAddress} port {vm.GamePort}?", "Connect");

                if (result)
                {
                    this.Close();
                }
            }
        }
    }
}