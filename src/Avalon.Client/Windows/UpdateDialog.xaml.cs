using Avalon.GitHub;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Avalon
{
    public partial class UpdateDialog : ContentDialog
    {
        public Release Release { get; set; }

        public Exception Exception { get; set; } = null;

        public bool Progress { get; set; } = false;

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
                using (var response = await http.GetAsync(App.Settings.AvalonSettings.ReleaseUrl))
                {
                    string json = await response.Content.ReadAsStringAsync();
                    this.Release = JsonConvert.DeserializeObject<Release>(json);

                    var updateVersion = new Version(this.Release.TagName);
                    var thisVersion = Assembly.GetExecutingAssembly().GetName().Version;

                    if (updateVersion == thisVersion)
                    {
                        TextBlockInfo.Text = "You are using the current version.";
                        return;
                    }
                    else if (updateVersion > thisVersion)
                    {
                        TextBlockInfo.Text = $"There is an update available to version {updateVersion}";
                        this.PrimaryButtonText = "Update";
                        return;
                    }
                    else if (updateVersion < thisVersion)
                    {
                        TextBlockInfo.Text = $"You are using a version that is newer than the general release.";
                        this.PrimaryButtonText = "";
                        return;
                    }
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
                    using (var response = http.GetAsync(item, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        string pluginName = Argus.IO.FileSystemUtilities.ExtractFileName(item);
                        string pluginSavePath = Path.Combine(App.Settings.UpdateDirectory, pluginName);

                        response.EnsureSuccessStatusCode();

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(pluginSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
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
                                    totalReads += 1;

                                    if (totalReads % 2000 == 0)
                                    {
                                        TextBlockInfo.Text = string.Format("Total bytes downloaded for {0}: {1:n0}", pluginName, totalRead);
                                    }
                                }
                            }
                            while (isMoreToRead);
                        }
                    }
                }

                using (var response = http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(installerFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
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
                                totalReads += 1;

                                if (totalReads % 2000 == 0)
                                {
                                    TextBlockInfo.Text = string.Format("Total bytes downloaded for installer update: {0:n0}", totalRead);
                                }
                            }
                        }
                        while (isMoreToRead);
                    }
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

    }
}
