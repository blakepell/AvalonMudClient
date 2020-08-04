using Avalon.Common.Models;
using Avalon.Controls;
using Avalon.Sqlite;
using Avalon.Windows;
using ModernWpf.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Avalon.Utilities
{
    /// <summary>
    /// Static class for the management of common windows that should be accessible in a way that scripts
    /// can call in.
    /// </summary>
    public static class WindowManager
    {

        /// <summary>
        /// Event used from menus to shell any number of Shell based UserControls into windows.
        /// TODO Clean this function up, there's a lot of redudant code here.
        /// </summary>
        public static async Task ShellWindow(string windowName)
        {
            // First see if an instance of this window already exists.
            var instance = App.Conveyor.WindowList.FirstOrDefault(x => x.Name == windowName);

            if (instance != null)
            {
                // Focus and activate the existing window.
                instance.Activate();
                return;
            }

            if (windowName == "Directions Select")
            {
                var win = new DirectionsSelectWindow
                {
                    Owner = App.MainWindow
                };

                win.ShowDialog();
            }
            else if (windowName == "Variables")
            {
                var win = new Shell(new VariableList(), null)
                {
                    Name = "Variables",
                    HeaderTitle = "Variables",
                    HeaderIcon = Symbol.Account,
                    SecondaryButtonVisibility = Visibility.Collapsed
                };

                win.SetSizeAndPosition(.85);
                win.Show();
            }
            else if (windowName == "Aliases")
            {
                var win = new Shell(new AliasList(), null)
                {
                    Name = "Aliases",
                    HeaderTitle = "Aliases",
                    HeaderIcon = Symbol.Account,
                    SecondaryButtonVisibility = Visibility.Collapsed
                };

                win.SetSizeAndPosition(.85);
                win.Show();
            }
            else if (windowName == "Macros")
            {
                var win = new Shell(new MacroList(), null)
                {
                    Name = "Macros",
                    HeaderTitle = "Macros",
                    HeaderIcon = Symbol.Keyboard,
                    SecondaryButtonVisibility = Visibility.Collapsed
                };

                win.SetSizeAndPosition(.85);
                win.Show();
            }
            else if (windowName == "Triggers")
            {
                var win = new Shell(new TriggerList(), null)
                {
                    Name = "Triggers",
                    HeaderTitle = "Triggers",
                    HeaderIcon = Symbol.Directions,
                    SecondaryButtonVisibility = Visibility.Collapsed
                };

                win.SetSizeAndPosition(.85);
                win.Show();
            }
            else if (windowName == "Directions")
            {
                var win = new Shell(new DirectionList(), null)
                {
                    Name = "Directions",
                    HeaderTitle = "Directions",
                    HeaderIcon = Symbol.Map,
                    SecondaryButtonVisibility = Visibility.Collapsed
                };

                win.SetSizeAndPosition(.85);
                win.Show();
            }
            else if (windowName == "Database")
            {
                try
                {
                    // Setup the database control.            
                    var ctrl = new SqliteQueryControl();
                    ctrl.ConnectionString = $"Data Source={App.Settings.ProfileSettings.SqliteDatabase}";
                    await ctrl.RefreshSchema();

                    var win = new Shell(ctrl, App.MainWindow)
                    {
                        Name = "Database",
                        HeaderTitle = "Database Query Editor",
                        HeaderIcon = Symbol.Tag,
                        SecondaryButtonVisibility = Visibility.Collapsed
                    };

                    win.SetSizeAndPosition(.85);

                    App.MainWindow.ShowDialog(win);
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoLog(ex.Message, LogType.Error);
                }
            }
        }
    }
}
