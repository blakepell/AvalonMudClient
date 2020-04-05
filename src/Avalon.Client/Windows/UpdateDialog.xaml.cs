using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
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
        public string AvalonUrl { get; set; } = "";

        public string DownloadUrl { get; set; } = "";

        public Exception Exception { get; set; } = null;

        public bool Progress { get; set; } = false;

        public UpdateDialog()
        {
            InitializeComponent();
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Closing += this.UpdateDialog_Closing;

                ProgressRingUpdate.IsActive = true;

                string versionJson = "";

                TextBlockInfo.Text = "Requesting update url.";

                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();

                // This is my site for now, it will always be there.. it will get the update url to the actual update site
                // which is currently, a free azure site that will probably move.
                using (var response = await http.GetAsync("https://www.blakepell.com/api/avalon-site-url"))
                {
                    this.AvalonUrl = await response.Content.ReadAsStringAsync();
                }

                TextBlockInfo.Text = $"Checking for update.";

                using (var response = await http.GetAsync($"{this.AvalonUrl}/api/current-client-version"))
                {
                    versionJson = await response.Content.ReadAsStringAsync();
                }

                var updateVersion = JsonConvert.DeserializeObject<Version>(versionJson, new VersionConverter());
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

                string requestUrl = $"{this.AvalonUrl}/api/download-url?is64Bit={Environment.Is64BitProcess}";
                string downloadFile = Path.Combine(App.Settings.UpdateDirectory, "Installer.zip");

                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();

                // This is my site for now, it will always be there.. it will get the update url to the actual update site
                // which is currently, a free azure site that will probably move.
                using (var response = await http.GetAsync(requestUrl))
                {
                    this.DownloadUrl = await response.Content.ReadAsStringAsync();
                    this.DownloadUrl = JsonConvert.DeserializeObject<string>(this.DownloadUrl);
                }

                using (var response = http.GetAsync(this.DownloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(downloadFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
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
                                    TextBlockInfo.Text = string.Format("Total bytes downloaded: {0:n0}", totalRead);
                                }
                            }
                        }
                        while (isMoreToRead);
                    }
                }

                // Download complete!
                TextBlockInfo.Text = "Download complete, starting installer...";

                // Unzip the archive.
                ZipFile.ExtractToDirectory(downloadFile, App.Settings.UpdateDirectory);

                // Installer file
                string installer = "";

                if (Environment.Is64BitProcess)
                {
                    installer = Path.Combine(App.Settings.UpdateDirectory, "AvalonSetup-x64.exe");
                }
                else
                {
                    installer = Path.Combine(App.Settings.UpdateDirectory, "AvalonSetup-x86.exe");
                }

                if (File.Exists(installer))
                {
                    Utilities.Utilities.ShellLink(installer);

                    // Delay 2 seconds.
                    await Task.Delay(2000);

                    // Alas, all good things must come to an end.
                    Environment.Exit(0);
                }
                else
                {
                    TextBlockInfo.Text = $"Error: {installer} not found.";
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                ProgressRingUpdate.IsActive = false;
                this.Hide();
            }
        }

    }
}
