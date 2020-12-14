using System;
using Avalon.Utilities;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace Avalon.Controls
{
    /// <summary>
    /// A base class for the navigation menu items.
    /// </summary>
    public class NavMenuItem : INavMenuItem
    {
        public string Title { get; set; }

        public PackIconMaterialKind Icon { get; set; }

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
                case NavType.Shell:
                    try
                    {
                        Utilities.Utilities.Shell(this.Argument);
                    }
                    catch (Exception ex)
                    {
                        App.Conveyor.EchoError(ex.Message);
                    }
                    break;
                case NavType.ShellWindow:
                    await WindowManager.ShellWindowAsync(this.Title);
                    break;
                case NavType.Alias:
                    // TODO
                    break;
            }
        }

    }
}