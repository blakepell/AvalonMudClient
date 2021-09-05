/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Settings;
using Avalon.Utilities;
using Argus.Extensions;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Avalon
{
    public partial class IntroWindow
    {

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
        }

        /// <summary>
        /// On loaded event for the IntroWindow.  We'll get our list of available profiles or allow the
        /// player to setup a new profile for a game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntroWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.ViewModel.Version = $"Version {version.Major.ToString()}.{version.Minor.ToString()}.{version.Revision}.{version.Build.ToString()}";

            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                return;
            }

            var files = Directory.GetFiles(App.Settings.AvalonSettings.SaveDirectory, "*.json");
            var fileInfoList = new List<FileInfo>();

            foreach (string file in files)
            {
                var fi = new FileInfo(file);
                fileInfoList.Add(fi);
            }

            foreach (var fi in fileInfoList.OrderByDescending(x => x.LastWriteTime))
            {
                string json = File.ReadAllText(fi.FullName);

                try
                {
                    var profile = JsonConvert.DeserializeObject<ProfileSettings>(json);

                    this.ViewModel.Profiles.Add(new()
                    {
                        GameAddress = profile?.IpAddress ?? "Empty Game Address",
                        GamePort = profile?.Port ?? 0,
                        GameDescription =  profile?.WindowTitle ?? "No Game Description",
                        Filename = fi.Name,
                        FullPath = fi.FullName,
                        LastSaveDate = fi.LastWriteTime,
                        ProfileSize = fi.FormattedFileSize()
                    });
                }
                catch { }
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

        /// <summary>
        /// Attempts to load the selected profile into the main mud client window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GridViewProfiles_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is IntroWindowViewModel.ProfileViewModel vm)
            {
                var result = await WindowManager.InputBox($"Would you like to connect to {vm.GameAddress} port {vm.GamePort}?", "Connect");

                if (result)
                {
                    await App.MainWindow.OpenProfile(vm.FullPath);
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Allows the user to create a new profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonNewProfile_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new IntroWindowNewProfileDialog();

            var result = await win.ShowAsync();

            if (result == ContentDialogResult.Secondary)
            {
                if (File.Exists(win.ProfileFileName))
                {
                    await App.MainWindow.OpenProfile(win.ProfileFileName);
                }

                this.Close();
            }
        }
    }
}