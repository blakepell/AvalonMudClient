using Avalon.Utilities;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Avalon.Controls
{
    /// <summary>
    /// A base class for the navigation menu items.
    /// </summary>
    public class NavMenuItem : INavMenuItem
    {
        public string Title { get; set; }

        public IconElement Icon { get; set; }

        public string Argument { get; set; }

        public NavType MenuType { get; set; } = NavType.Default;
        
        public INavMenuItem Reference => this;

        public virtual async Task ExecuteAsync()
        {
            switch (this.MenuType)
            {
                case NavType.Default:
                    // TODO
                    break;
                case NavType.ShellWindow:
                    await WindowManager.ShellWindow(this.Title);
                    break;
                case NavType.Alias:
                    // TODO
                    break;
            }
        }

    }
}
