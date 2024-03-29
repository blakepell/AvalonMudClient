﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.GitHub;
using ModernWpf.Controls;
using System.Net.Http;
using System.Windows;

namespace Avalon
{
    public partial class UpdateDialog
    {
        public Release Release { get; set; }

        public Exception Exception { get; set; } = null;

        public bool Progress { get; set; } = false;

        public bool PluginsOnly { get; set; } = false;

        public UpdateDialog()
        {
            InitializeComponent();
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.Closing += this.UpdateDialog_Closing;

            ProgressRingUpdate.IsActive = true;
            TextBlockInfo.Text = "Checking for update.";

            try
            {
                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "Avalon Mud Client");

                // Get the release information from GitHub, including the version and the links to all of the installers
                // for this release.
                using var response = await http.GetAsync(App.Settings.AvalonSettings.ReleaseUrl);
                string json = await response.Content.ReadAsStringAsync();
                this.Release = JsonConvert.DeserializeObject<Release>(json);

                if (this.PluginsOnly)
                {
                    TextBlockInfo.Text = "Would you like to download the latest version of the plugins?";
                    this.SecondaryButtonText = "Update Plugins";
                    ProgressRingUpdate.IsActive = false;
                    return;
                }

                var updateVersion = new Version(this.Release.TagName);
                var thisVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (updateVersion == thisVersion)
                {
                    TextBlockInfo.Text = "You are using the current version.";
                }
                else if (updateVersion > thisVersion)
                {
                    TextBlockInfo.Text = $"There is an update available to version {updateVersion}";
                    this.PrimaryButtonText = "Update";
                }
                else if (updateVersion < thisVersion)
                {
                    TextBlockInfo.Text = "You are using a version that is newer than the general release.";
                    this.PrimaryButtonText = "";
                }

            }
            catch (Exception ex)
            {
                this.Exception = ex;
            }
            finally
            {
                ProgressRingUpdate.IsActive = false;
            }

        }

        /// <summary>
        /// Prevents closing of the dialog which is the default action by a dialog when any of its stock buttons
        /// are clicked, we will handle closing manually by calling this.Hide().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UpdateDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (ProgressRingUpdate.IsActive)
            {
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Update the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                ProgressRingUpdate.IsActive = true;

                string downloadUrl = "";
                string installerFile = "";
                var plugins = new List<string>();

                foreach (var asset in this.Release.Assets)
                {
                    if (Environment.Is64BitProcess && asset.BrowserDownloadUrl.Contains("AvalonSetup-x64.exe"))
                    {
                        downloadUrl = asset.BrowserDownloadUrl;
                        installerFile = Path.Combine(App.Settings.UpdateDirectory, "AvalonSetup-x64.exe");
                    }

                    if (!Environment.Is64BitProcess && asset.BrowserDownloadUrl.Contains("AvalonSetup-x86.exe"))
                    {
                        downloadUrl = asset.BrowserDownloadUrl;
                        installerFile = Path.Combine(App.Settings.UpdateDirectory, "AvalonSetup-x86.exe");
                    }

                    if (asset.BrowserDownloadUrl.EndsWith(".dll"))
                    {
                        plugins.Add(asset.BrowserDownloadUrl);
                    }

                }

                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();

                // Get any plugins first
                foreach (string item in plugins)
                {
                    using var response = await http.GetAsync(item, HttpCompletionOption.ResponseHeadersRead);
                    string pluginName = Argus.IO.FileSystemUtilities.ExtractFileName(item);
                    string pluginSavePath = Path.Combine(App.Settings.UpdateDirectory, pluginName);

                    response.EnsureSuccessStatusCode();

                    using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(pluginSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads++;

                            if (totalReads % 2000 == 0)
                            {
                                TextBlockInfo.Text = $"Total bytes downloaded for {pluginName}: {totalRead:n0}";
                            }
                        }
                    }
                    while (isMoreToRead);
                }

                // Now, get the full installer.
                using (var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(installerFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads++;

                            if (totalReads % 2000 == 0)
                            {
                                TextBlockInfo.Text = $"Total bytes downloaded for installer update: {totalRead:n0}";
                            }
                        }
                    }
                    while (isMoreToRead);
                }

                // Download complete!
                TextBlockInfo.Text = "Download complete, starting installer...";

                // Sanity check that it exists.
                if (File.Exists(installerFile))
                {
                    Utilities.Utilities.ShellLink(installerFile);

                    // Delay 2 seconds.
                    await Task.Delay(2000);

                    // Alas, all good things must come to an end.
                    Environment.Exit(0);
                }
                else
                {
                    TextBlockInfo.Text = $"Error: {installerFile} not found.";
                }
            }
            catch (Exception ex)
            {
                this.Exception = ex;
            }
            finally
            {
                ProgressRingUpdate.IsActive = false;
                this.Hide();
            }
        }

        /// <summary>
        /// Force the updates of the plugins.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                ProgressRingUpdate.IsActive = true;

                var plugins = new List<string>();

                foreach (var asset in this.Release.Assets)
                {
                    if (asset.BrowserDownloadUrl.EndsWith(".dll"))
                    {
                        plugins.Add(asset.BrowserDownloadUrl);
                    }

                }

                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();

                // Get any plugins first
                foreach (string item in plugins)
                {
                    using var response = await http.GetAsync(item, HttpCompletionOption.ResponseHeadersRead);
                    string pluginName = Argus.IO.FileSystemUtilities.ExtractFileName(item);
                    string pluginSavePath = Path.Combine(App.Settings.UpdateDirectory, pluginName);

                    response.EnsureSuccessStatusCode();

                    using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(pluginSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads++;

                            if (totalReads % 2000 == 0)
                            {
                                TextBlockInfo.Text = string.Format("Total bytes downloaded for {0}: {1:n0}", pluginName, totalRead);
                            }
                        }
                    }
                    while (isMoreToRead);
                }

                // Download complete!
                TextBlockInfo.Text = "Download complete, you will need to close and restart the mud client.";
                this.SecondaryButtonText = "";
            }
            catch (Exception ex)
            {
                this.Exception = ex;
            }
            finally
            {
                ProgressRingUpdate.IsActive = false;
                this.Hide();
            }

        }
    }
}
