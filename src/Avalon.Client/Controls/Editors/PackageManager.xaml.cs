using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using ModernWpf;
using ModernWpf.Controls;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Avalon.Controls
{
    public partial class PackageManager : UserControl, IShellControl
    {

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        DispatcherTimer _typingTimer;

        /// <summary>
        /// The package list as downloaded from the API site.
        /// </summary>
        public List<Package> PackageList { get; set; }

        public PackageManager()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = this;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var win = this.FindAscendant<Shell>();

            try
            {
                win.ProgressRingVisibility = Visibility.Visible;
                win.ProgressRingIsActive = true;
                win.StatusBarLeftText = $"Downloading packaging listing from {App.Settings.ProfileSettings.IpAddress}";

                var client = new RestClient(App.Settings.AvalonSettings.PackageManagerApiUrl);
                client.UseNewtonsoftJson();

                // This retrieves all of the packages, but only the metadata, none of the content until something is specifically requested.
                var request = new RestRequest("get-all-metadata", RestSharp.DataFormat.Json).AddQueryParameter("ip", App.Settings.ProfileSettings.IpAddress);
                this.PackageList = await client.GetAsync<List<Package>>(request);

                if (this.PackageList.Count == 0)
                {
                    win.ProgressRingIsActive = false;
                    win.ProgressRingVisibility = Visibility.Collapsed;
                    win.StatusBarRightText = "0 Packages";
                    win.StatusBarLeftText = $"Packages for {App.Settings.ProfileSettings.IpAddress}.";
                    await this.MsgBox($"There are no packages available for {App.Settings.ProfileSettings.IpAddress}", "No Packages Found");
                    return;
                }

                // Update which ones if any are installed.
                this.UpdateInstalledList();

                // Load the variable list the first time that it's requested.
                if (DataList.ItemsSource == null)
                {
                    var lcv = new ListCollectionView(this.PackageList)
                    {
                        Filter = Filter
                    };

                    DataList.ItemsSource = lcv;
                }

                win.StatusBarLeftText = $"Packages for {App.Settings.ProfileSettings.IpAddress} successfully downloaded.";
                win.StatusBarRightText = $"{this.PackageList.Count} Packages";

                this.FocusFilter();
            }
            catch (Exception ex)
            {
                await this.MsgBox($"An error occurred requesting the package list: {ex.Message}", "Package Manager Error");
                win.StatusBarLeftText = $"Package list failed: {ex.Message}";
                win.StatusBarRightText = $"0 Packages";
            }

            win.ProgressRingIsActive = false;
            win.ProgressRingVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// When the control is unloaded: Release any bindings or objects that need to be freed or
        /// detached from this control so it can be GC'd properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageManager_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataList.ItemsSource is ListCollectionView lcv)
            {
                lcv.DetachFromSourceCollection();
                lcv = null;
            }
        }

        /// <summary>
        /// Shows a message box dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public async Task<ContentDialogResult> MsgBox(string message, string title)
        {
            var dialog = new MessageBoxDialog()
            {
                Title = title,
                Content = message,
            };

            return await dialog.ShowAsync();
        }

        /// <summary>
        /// A yes or no input box.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public async Task<bool> InputBox(string message, string title)
        {
            var confirmDialog = new YesNoDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Secondary)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the focus onto the filter text box.
        /// </summary>
        public void FocusFilter()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    TextFilter.Focus();
                }));
        }

        /// <summary>
        /// The number of items currently selected.
        /// </summary>
        public int SelectedCount()
        {
            return DataList?.SelectedItems?.Count ?? 0;
        }

        /// <summary>
        /// The typing delay timer's tick that will refresh the filter after 300ms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// It's important to stop this timer when this fires so that it doesn't continue
        /// to fire until it's needed again.
        /// </remarks>
        private void _typingTimer_Tick(object sender, EventArgs e)
        {
            _typingTimer.Stop();
            ((ListCollectionView)DataList?.ItemsSource)?.Refresh();
        }

        /// <summary>
        /// The actual filter that's used to filter down the DataGrid.
        /// </summary>
        /// <param name="item"></param>
        private bool Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(TextFilter.Text))
            {
                return true;
            }

            var package = (Package)item;

            return (package?.Name?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (package?.Description?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (package?.Author?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// The filter's text changed event that will setup the delay timer and effective callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _typingTimer.Stop();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(300);
            _typingTimer.Start();
        }


        /// <summary>
        /// Looks to see if any of the packages are installed or parts of them are installed.
        /// </summary>
        private void UpdateInstalledList()
        {
            foreach (var item in this.PackageList)
            {
                if (InstallStatus(item.Id))
                {
                    item.IsInstalled = true;

                    // Is there an update available?
                    var package = App.Settings.ProfileSettings.InstalledPackages.FirstOrDefault(x => x.PackageId == item.Id);

                    if (item.Version > package?.Version)
                    {
                        item.UpdateAvailable = true;
                    }
                    else
                    {
                        item.UpdateAvailable = false;
                    }
                }
                else
                {
                    item.IsInstalled = false;
                }
            }
        }

        /// <summary>
        /// Returns info about whether this package currently exists in the profile.
        /// </summary>
        /// <param name="packageId"></param>
        private bool InstallStatus(string packageId)
        {
            bool aliasExists = App.Settings.ProfileSettings.AliasList.Any(x => x.PackageId == packageId);

            if (aliasExists)
            {
                return true;
            }

            var triggerExists = App.Settings.ProfileSettings.TriggerList.Any(x => x.PackageId == packageId);

            if (triggerExists)
            {
                return true;
            }

            var directionExists = App.Settings.ProfileSettings.TriggerList.Any(x => x.PackageId == packageId);

            if (directionExists)
            {
                return true;
            }

            var packageExists = App.Settings.ProfileSettings.InstalledPackages.Any(x => x.PackageId == packageId);

            if (packageExists)
            {
                return true;
            }

            return false;
        }

        private async void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
        {
            var package = ((FrameworkElement)sender).DataContext as Package;

            if (package == null)
            {
                return;
            }

            var result = await InputBox($"Are you sure you want to install: {package.Name} version {package.Version}?", "Confirm Install");

            if (!result)
            {
                return;
            }

            var win = this.FindAscendant<Shell>();

            try
            {
                win.ProgressRingVisibility = Visibility.Visible;
                win.ProgressRingIsActive = true;
                win.StatusBarLeftText = $"Downloading and installing {package.Name}";

                var client = new RestClient(App.Settings.AvalonSettings.PackageManagerApiUrl);
                client.UseNewtonsoftJson();

                // This retrieves only the specific package by the ID we want to install.
                var request = new RestRequest("get", RestSharp.DataFormat.Json).AddQueryParameter("id", package.Id);
                var fullPackage = await client.GetAsync<Package>(request);

                // Loop through all items in this package and make sure the PackageId is set of every part
                // that will be installed.
                fullPackage.AliasList.ForEach(x => x.PackageId = fullPackage.Id);
                fullPackage.TriggerList.ForEach(x => x.PackageId = fullPackage.Id);
                fullPackage.DirectionList.ForEach(x => x.PackageId = fullPackage.Id);

                // This will update this profile with the items from the json package.
                App.Settings.ImportPackage(fullPackage);

                // Update which ones if any are installed.
                this.UpdateInstalledList();

                await MsgBox($"{package.Name} version {package.Version} has successfully been installed.", "Success");
                win.StatusBarLeftText = $"{package.Name} was installed successfully.";
            }
            catch (Exception ex)
            {
                await this.MsgBox($"An error occurred requesting the package list: {ex.Message}", "Package Manager Error");
                win.StatusBarLeftText = $"Package list failed: {ex.Message}";
                win.StatusBarRightText = $"0 Packages";
            }

            win.ProgressRingIsActive = false;
            win.ProgressRingVisibility = Visibility.Collapsed;
        }

        private async void ButtonUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            var package = ((FrameworkElement)sender).DataContext as Package;

            if (package == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(package.Id))
            {
                await MsgBox("The Package ID for this package is blank and thus cannot be uninstalled.", "Error");
                return;
            }

            var result = await InputBox($"Are you sure you want to uninstall: {package.Name} version {package.Version}?", "Confirm Uninstall");

            if (!result)
            {
                return;
            }

            // Walk backwards through each list and remove anything from this package.
            for (int i = App.Settings.ProfileSettings.AliasList.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.AliasList[i].PackageId == package.Id)
                {
                    App.Settings.ProfileSettings.AliasList.RemoveAt(i);
                }
            }

            for (int i = App.Settings.ProfileSettings.TriggerList.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.TriggerList[i].PackageId == package.Id)
                {
                    App.Settings.ProfileSettings.TriggerList.RemoveAt(i);
                }
            }

            for (int i = App.Settings.ProfileSettings.DirectionList.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.DirectionList[i].PackageId == package.Id)
                {
                    App.Settings.ProfileSettings.DirectionList.RemoveAt(i);
                }
            }

            // Remove the package ID from the list.
            for (int i = App.Settings.ProfileSettings.InstalledPackages.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.InstalledPackages[i].PackageId == package.Id)
                {
                    App.Settings.ProfileSettings.InstalledPackages.RemoveAt(i);
                }
            }

            // Update which ones if any are installed.
            this.UpdateInstalledList();

            await MsgBox($"{package.Name} version {package.Version} was successfully uninstalled.", "Success");
        }

        public void PrimaryButtonClick()
        {
            // Do nothing.
        }

        public void SecondaryButtonClick()
        {
            // Do nothing.
        }

    }
}
