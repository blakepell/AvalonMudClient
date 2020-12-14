using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using MahApps.Metro.IconPacks;

namespace Avalon.Controls
{
    /// <summary>
    /// Handles the loading of the navigation menu items.
    /// </summary>
    public class NavManager : INotifyPropertyChanged
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public NavManager()
        {
            this.NavMenuItems = new ObservableCollection<NavMenuItem>();
        }

        /// <summary>
        /// Loads (or reloads) the items for the navigation.
        /// </summary>
        public void Load()
        {
            this.NavMenuItems.Clear();
            this.LoadImmutableItems();
        }

        /// <summary>
        /// Loads the immutable static items.
        /// </summary>
        private void LoadImmutableItems()
        {
            // Setup the NavBar with the immutable items.
            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Aliases",
                Icon = PackIconMaterialKind.ShuffleVariant,
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Triggers",
                Icon = PackIconMaterialKind.LightningBolt,
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Macros",
                Icon = PackIconMaterialKind.Keyboard,
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Directions",
                Icon = PackIconMaterialKind.MapOutline,
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Variables",
                Icon = PackIconMaterialKind.IframeVariableOutline,
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Database",
                Icon = PackIconMaterialKind.Database,
                MenuType = NavType.ShellWindow
            });

            // TODO support arguments so someone could pass arguments to something like Putty for SSH or a path with the terminal.  These
            // probably should be their own data structure and not one offed in the settings so we may change that in the future to provide
            // better flexibility.
            // A default SSH client to shell.
            if (!string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.SshAppPath) && File.Exists(App.Settings.ProfileSettings.SshAppPath))
            {
                this.NavMenuItems.Add(new NavMenuItem
                {
                    Title = "SSH",
                    Icon = PackIconMaterialKind.Ssh,
                    MenuType = NavType.Shell,
                    Argument = App.Settings.ProfileSettings.SshAppPath
                });
            }

            // A default SSH client to shell (we're not checking for the existence since things like cmd or wt are in the path).
            if (!string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.TerminalAppPath))
            {
                this.NavMenuItems.Add(new NavMenuItem
                {
                    Title = "Terminal",
                    Icon = PackIconMaterialKind.Console,
                    MenuType = NavType.Shell,
                    Argument = App.Settings.ProfileSettings.TerminalAppPath
                });
            }

        }

        /// <summary>
        /// A list of the built in nav menu items.
        /// </summary>
        public ObservableCollection<NavMenuItem> NavMenuItems { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}