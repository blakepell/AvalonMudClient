using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
                Icon = new SymbolIcon(Symbol.Shuffle),
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Triggers",
                Icon = new SymbolIcon(Symbol.Directions),
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Macros",
                Icon = new SymbolIcon(Symbol.Keyboard),
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Directions",
                Icon = new SymbolIcon(Symbol.Map),
                MenuType = NavType.ShellWindow
            });

            this.NavMenuItems.Add(new NavMenuItem
            {
                Title = "Variables",
                Icon = new SymbolIcon(Symbol.Account),
                MenuType = NavType.ShellWindow
            });

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