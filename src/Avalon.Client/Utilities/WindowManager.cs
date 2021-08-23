/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using Avalon.Controls;
using Avalon.Sqlite;
using Avalon.Windows;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using Avalon.Sqlite.Common;

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
        /// TODO Clean this function up, there's a lot of redundant code here.
        /// </summary>
        public static async Task ShellWindowAsync(string windowName)
        {
            // First see if an instance of this window already exists.
            var instance = App.Conveyor.WindowList.Find(x => x.Name == windowName);

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
                // Pass the variable list collection we want to bind to this control.
                var win = new Shell(new VariableList(App.Settings.ProfileSettings.Variables), null)
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
                    var ctrl = new QueryControl
                    {
                        ConnectionString = $"Data Source={App.Settings.ProfileSettings.SqliteDatabase}",
                        Theme = ControlTheme.Gray
                    };

                    ctrl.ExpandTableNode();
                    ctrl.ExpandViewsNode();

                    // Removed dialog/blur
                    //var win = new Shell(ctrl, App.MainWindow)

                    var win = new Shell(ctrl, null)
                    {
                        Name = "Database",
                        HeaderTitle = "Database Query Editor",
                        HeaderIcon = Symbol.Tag,
                        SecondaryButtonVisibility = Visibility.Collapsed
                    };

                    await ctrl.RefreshSchemaAsync();

                    win.SetSizeAndPosition(.85);
                    win.Show();
                    //App.MainWindow.ShowDialog(win);
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoLog(ex.Message, LogType.Error);
                }
            }
            else if (windowName == "PackageManager")
            {
                try
                {
                    var win = new Shell(new PackageManager(), null)
                    {
                        Name = "PackageManager",
                        HeaderTitle = "Package Manager",
                        HeaderIcon = Symbol.SyncFolder,
                        SecondaryButtonVisibility = Visibility.Collapsed
                    };

                    win.SetSizeAndPosition(.85);
                    win.Show();

                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoLog(ex.Message, LogType.Error);
                }
            }
            else if (windowName == "UpdateDLLPlugin")
            {
                var confirmDialog = new UpdateDialog()
                {
                    PluginsOnly = true
                };

                _ = await confirmDialog.ShowAsync();

                App.MainWindow.Interp.Conveyor.EchoText("\r\n");
                App.MainWindow.Interp.Conveyor.EchoLog("In order for the plugin updates to take effect you will need to close and then restart this application.", LogType.Warning);
            }
        }
    }
}
