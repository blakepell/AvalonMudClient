using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Common.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Avalon
{
    /// <summary>
    /// Partial class for plugin related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Finds plugins in any DLL's in the plugins folder and loads them.
        /// </summary>
        private void LoadPlugins()
        {
            App.Plugins.Clear();

            // Get all the DLL's in the plugin folder.
            var files = Directory.GetFiles(App.Settings.PluginDirectory, "*.dll");

            // Get the type of the plugin interface.
            var pluginType = typeof(IPlugin);

            // Loop through the assemblies looking for only plugins.
            foreach (var file in files)
            {
                // Load the assembly
                var assembly = Assembly.LoadFrom(file);

                // Get the plugins, don't load abstract classes or interfaces.
                var plugins = assembly.GetTypes().Where(x => pluginType.IsAssignableFrom(x)
                                                             && !x.IsAbstract
                                                             && !x.IsInterface);

                foreach (var plugin in plugins)
                {
                    App.Conveyor.EchoLog($"Plugin Found: {Argus.IO.FileSystemUtilities.ExtractFileName(file)} v{assembly.GetName().Version}", LogType.Information);

                    Plugin pluginInstance;

                    try
                    {
                        pluginInstance = (Plugin)Activator.CreateInstance(plugin);
                    }
                    catch (Exception ex)
                    {
                        App.Conveyor.EchoLog($"Plugin Load Error: {ex.Message}", LogType.Error);
                        continue;
                    }

                    App.Plugins.Add(pluginInstance);

                    if (App.Settings.AvalonSettings.DeveloperMode)
                    {
                        App.Conveyor.EchoLog($"   Plugin For: {pluginInstance?.IpAddress ?? "Unknown"}", LogType.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Activates any plugins for the game associated with the currently loaded profile.
        /// </summary>
        private void ActivatePlugins()
        {
            ActivatePlugins(App.Settings.ProfileSettings.IpAddress);
        }

        /// <summary>
        /// Loads any plugins found in any DLL in the plugins folder for the game associated with
        /// the IP address specified.
        ///
        /// TODO - cleanup
        /// 
        /// </summary>
        private void ActivatePlugins(string ipAddress)
        {
            // If there are any system triggers, clear the references to the Conveyor.
            foreach (var trigger in App.SystemTriggers)
            {
                trigger.Conveyor = null;
            }

            // Clear the system triggers list.
            App.SystemTriggers.Clear();

            // Go through all of the found plugins and add in and initialize any triggers so they can be used now.
            foreach (var plugin in App.Plugins)
            {
                // The IP address specified in the plugin matches the IP address we're connecting to.
                if (string.Equals(plugin.IpAddress, ipAddress, StringComparison.OrdinalIgnoreCase))
                {
                    // Set a copy of the current profile settings into the plugin in case it needs them.
                    plugin.ProfileSettings = App.Settings.ProfileSettings;
                    plugin.Conveyor = App.Settings.Conveyor;

                    // Let it run it's own Initialize.
                    plugin.Initialize();

                    // Go through all of the triggers and put them into our system triggers.
                    foreach (var trigger in plugin.Triggers)
                    {
                        trigger.Plugin = true;
                        trigger.Conveyor = App.Conveyor;

                        if (trigger.SystemTrigger)
                        {
                            // System triggers get loaded every time
                            App.SystemTriggers.Add(trigger);
                        }
                        else
                        {
                            // It wasn't a system trigger, try to import it.  The import function will check whether
                            // it can be imported or not (e.g. if it's locked by the user).
                            App.Settings.ImportTrigger(trigger);
                        }
                    }

                    // Not the fastest, but now that we've added or updated the trigger list from a plugin, sort all the plugins
                    // by priority.  This copies the list, sorts it, then clears the original list and re-adds the items into it
                    // as to not break binding.
                    App.Conveyor.SortTriggersByPriority();

                    // Load any top level menu items included in the plugin.
                    foreach (var item in plugin.MenuItems)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        var rd = (ResourceDictionary)item;
                        var menuItem = (MenuItem)rd["PluginMenu"];
                        menuItem.Tag = this.Interp;

                        if (menuItem.Parent == null)
                        {
                            MenuGame.Items.Add(menuItem);
                        }
                    }

                    // Add any custom hash commands.
                    foreach (var item in plugin.HashCommands)
                    {
                        // Only add a hash command if it doesn't already exist.
                        if (!Interp.HashCommands.Any(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Make sure it has a current reference to the interpreter.
                            item.Interpreter = this.Interp;

                            // Add it to the list of HashCommands.
                            Interp.HashCommands.Add(item);
                        }
                    }

                    // Add any custom Lua commands if everything is setup correctly for them.
                    foreach (var item in plugin.LuaCommands)
                    {
                        if (item.Key == null || item.Value == null)
                        {
                            App.Conveyor.EchoLog($"Null LuaCommand from {plugin.IpAddress}", LogType.Error);
                            continue;
                        }

                        Interp.LuaCaller.RegisterType(item.Value, item.Key);
                    }

                    App.Conveyor.EchoLog($"   {plugin.Triggers.Count} Triggers Loaded", LogType.Success);
                }
            }
        }
    }
}