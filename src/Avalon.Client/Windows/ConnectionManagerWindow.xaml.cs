/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.IO;
using Avalon.Common.Settings;
using Avalon.Utilities;
using ModernWpf.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// Connection manager: Allows the player to easily switch and create new profiles.
    /// </summary>
    public partial class ConnectionManagerWindow
    {

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(ConnectionManagerWindowViewModel), typeof(ConnectionManagerWindow), new PropertyMetadata(default(ConnectionManagerWindowViewModel)));

        public ConnectionManagerWindowViewModel ViewModel
        {
            get => (ConnectionManagerWindowViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ConnectionManagerWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.ViewModel = new ConnectionManagerWindowViewModel
            {
                Profiles = new()
            };
        }

        /// <summary>
        /// On loaded event for the ConnectionManagerWindow.  We'll get our list of available profiles or allow the
        /// player to setup a new profile for a game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManagerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                this.ViewModel.Version = $"Version {version.Major.ToString()}.{version.Minor.ToString()}.{version.Revision}.{version.Build.ToString()}";

                if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
                {
                    return;
                }

                var fs = new FileSystemSearch(App.Settings.AvalonSettings.SaveDirectory, "*.json", SearchOption.TopDirectoryOnly);
                fs.IncludeDirectories = false;

                // Get everything but "avalon.json" in the case where the save profiles folder is the
                // same as the application data folder.
                var files = fs.Where(x => !x.Name.Equals("avalon.json", StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.LastWriteTime).ToList();
            
                // No profiles exist, create a default one.
                if (files.Count == 0)
                {
                    string fileName = Path.Combine($"{App.Settings.AvalonSettings.SaveDirectory}", $"dsl-mud.org-4000.json");

                    var profile = new ProfileSettings
                    {
                        WindowTitle = "Dark and Shattered Lands: 4000",
                        FileName = "dsl-mud.org-4000.json",
                        IpAddress = "dsl-mud.org",
                        Port = 4000
                    };

                    // Write the profile settings file to disk.
                    File.WriteAllText(fileName, JsonConvert.SerializeObject(profile, Formatting.Indented));

                    // Like it always existed.
                    files.Add(new FileInfo(fileName));
                }

                foreach (var fi in files)
                {
                    string json = File.ReadAllText(fi.FullName);

                    try
                    {
                        App.MainWindow.Interp.ScriptHost.Lock = true;

                        var profile = JsonConvert.DeserializeObject<ProfileSettings>(json);
                        SolidColorBrush accentBrush;
                        var daysOld = Math.Abs((fi.LastWriteTime - DateTime.Now).TotalDays);

                        if (daysOld >= 14)
                        {
                            accentBrush = Brushes.Red;
                        }
                        else if (daysOld >= 7)
                        {
                            accentBrush = Brushes.Yellow;
                        }
                        else
                        {
                            accentBrush = Brushes.Green;
                        }

                        this.ViewModel.Profiles.Add(new()
                        {
                            GameAddress = profile?.IpAddress ?? "Empty Game Address",
                            GamePort = profile?.Port ?? 0,
                            GameDescription = profile?.WindowTitle ?? "No Game Description",
                            Filename = fi.Name,
                            FullPath = fi.FullName,
                            LastSaveDate = fi.LastWriteTime,
                            ProfileSize = ((FileInfo)fi).FormattedFileSize(),
                            AccentColor = accentBrush
                        });

                    }
                    finally
                    {
                        // Always, -always- unlock this after.
                        App.MainWindow.Interp.ScriptHost.Lock = false;
                    }
                }

                // Select the first item in the list will which be the last profile the user
                // connected to (and saved).
                if (GridViewProfiles.Items.Count > 0)
                {
                    GridViewProfiles.SelectedIndex = 0;

                    if (GridViewProfiles.SelectedItem is ConnectionManagerWindowViewModel.ProfileViewModel vm)
                    {
                        this.ViewModel.SelectedProfile = vm;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A critical error occurred: '{ex.Message}'");
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
        private void GridViewProfiles_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ConnectionManagerWindowViewModel.ProfileViewModel vm)
            {
                this.ViewModel.SelectedProfile = vm;
            }
        }

        /// <summary>
        /// Allows the user to create a new profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonNewProfile_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new ConnectionManagerNewProfileDialog();

            var result = await win.ShowAsync();

            if (result == ContentDialogResult.Secondary)
            {
                if (string.IsNullOrWhiteSpace(win.ProfileFileName))
                {
                    return;
                }

                if (File.Exists(win.ProfileFileName))
                {
                    await App.MainWindow.OpenProfile(win.ProfileFileName);
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        /// <summary>
        /// To track if the play button has been execluted and it slotted to close.
        /// </summary>
        private bool _playExecuted = false;

        /// <summary>
        /// Sends a message to the main window to connect to the selected game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playExecuted)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this?.ViewModel?.SelectedProfile?.FullPath))
            {
                // Tracking whether the plan was executed must be first.
                _playExecuted = true;
                await App.MainWindow.OpenProfile(this.ViewModel.SelectedProfile.FullPath, this.ViewModel.SelectedProfile.GameAddress, this.ViewModel.SelectedProfile.GamePort);
                App.Settings.ProfileSettings.WindowTitle = this.ViewModel.SelectedProfile.GameDescription;
                this.DialogResult = true;
                this.Close();
                return;
            }
            
            await WindowManager.MsgBox("You must select a profile first.", "Play Game").ConfigureAwait(false);
        }

        /// <summary>
        /// Ensures the DialogResult is false unless Play was clicked and set it to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManagerWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (this.DialogResult.HasValue && this.DialogResult.Value)
            {
                return;
            }

            // Setting the DialogResult twice will result in an exception.
            if (!_playExecuted)
            {
                this.DialogResult = false;
            }
        }
    }
}