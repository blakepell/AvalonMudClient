using Avalon.Common.Colors;
using Avalon.Common.Settings;
using Avalon.Controls;
using Avalon.Controls.AutoCompleteTextBox;
using Avalon.Lua;
using Avalon.Timers;
using MoonSharp.Interpreter;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Reflection;
using System.Threading.Tasks;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Common.Plugins;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the global reference.
            App.MainWindow = this;
        }

        /// <summary>
        /// The input history from the user so we can scroll back through it, etc.
        /// </summary>
        public Interpreter Interp;

        /// <summary>
        /// Lua control token to pause, stop and resume scripts.
        /// </summary>
        public ExecutionControlToken LuaControl;

        /// <summary>
        /// Timer to time ticks with.
        /// </summary>
        public TickTimer TickTimer;

        /// <summary>
        /// Window initialization.  This occurs before the Loaded event.  We'll set the initial
        /// window positioning here before the UI is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// The Loaded event for the Window where we will execute code that should run when the Window
        /// is first put into place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // The settings for the app load in the app startup, they will then try to load the last profile
            // that was used.
            App.Conveyor.EchoLog($"Avalon Mud Client Version {Assembly.GetExecutingAssembly()?.GetName()?.Version.ToString() ?? "Unknown"}", LogType.Information);
            App.Conveyor.EchoLog($"Global Settings Folder: {App.Settings.AppDataDirectory}", LogType.Information);
            App.Conveyor.EchoLog($"Global Settings File:   {App.Settings.AvalonSettingsFile}", LogType.Information);
            App.Conveyor.EchoLog($"Profiles Folder: {App.Settings.AvalonSettings.SaveDirectory}", LogType.Information);

            // Parse the command line arguments to see if a profile was specified.
            var args = Environment.GetCommandLineArgs();


            // Try to load the last profile loaded, if not found create a new profile.
            if (File.Exists(App.Settings.AvalonSettings.LastLoadedProfilePath))
            {
                LoadSettings(App.Settings.AvalonSettings.LastLoadedProfilePath);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(App.Settings.AvalonSettings.LastLoadedProfilePath))
                {
                    App.Conveyor.EchoLog($"New Profile being created.", LogType.Information);
                }
                else
                {
                    App.Conveyor.EchoLog($"Last Profile Loaded Not Found: {App.Settings.AvalonSettings.LastLoadedProfilePath}", LogType.Warning);
                }
            }

            // Set the startup position.
            switch (App.Settings.AvalonSettings.WindowStartupPosition)
            {
                case WindowStartupPosition.OperatingSystemDefault:
                    this.WindowState = WindowState.Normal;
                    break;
                case WindowStartupPosition.Maximized:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowStartupPosition.LastUsed:
                    this.Left = App.Settings.AvalonSettings.LastWindowPosition.Left;
                    this.Top = App.Settings.AvalonSettings.LastWindowPosition.Top;
                    this.Height = App.Settings.AvalonSettings.LastWindowPosition.Height;
                    this.Width = App.Settings.AvalonSettings.LastWindowPosition.Width;
                    this.WindowState = (WindowState)App.Settings.AvalonSettings.LastWindowPosition.WindowState;
                    break;
            }

            // Inject the Conveyor into the Triggers so the Triggers know how to talk to the UI.  Not doing this
            // causes ya know, problems.
            TriggersList.TriggerConveyorSetup();

            // Wire up any events that have to be wired up through code.
            TextInput.Editor.PreviewKeyDown += this.Editor_PreviewKeyDown;
            AddHandler(TabControlEx.NetworkButtonClickEvent, new RoutedEventHandler(NetworkButton_Click));
            AddHandler(TabControlEx.SettingsButtonClickEvent, new RoutedEventHandler(SettingsButton_Click));

            // Pass the necessary reference from this page to the Interpreter.
            Interp = new Interpreter(App.Conveyor);

            // Setup the handler so when it wants to write to the main window it can by raising the echo event.
            Interp.Echo += this.InterpreterEcho;

            // Setup the tick timer.
            TickTimer = new TickTimer(App.Conveyor);

            // Setup the auto complete commands.  If they're found refresh them, if they're not
            // report it to the terminal window.  It should -always be found-.
            RefreshAutoCompleteEntries();

            // Setup the database control.            
            try
            {
                App.Conveyor.EchoLog($"Initializing SQLite Database: {App.Settings.ProfileSettings.SqliteDatabase}", LogType.Information);
                SqliteQueryControl.ConnectionString = @$"Data Source={App.Settings.ProfileSettings.SqliteDatabase}";
                await SqliteQueryControl.RefreshSchema();
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }

            // Auto connect to the game if the setting is set.
            if (App.Settings.ProfileSettings.AutoConnect)
            {
                NetworkButton_Click(null, null);
            }

            // Is there an auto execute command or set of commands to run?
            if (!string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.AutoExecuteCommand))
            {
                Interp.Send(App.Settings.ProfileSettings.AutoExecuteCommand, true, false);
            }

            // Finally, all is done, set the focus to the command box.
            TextInput.Focus();
        }

        /// <summary>
        /// Loads the settings for the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadSettings(string fileName)
        {
            try
            {
                App.Settings.LoadSettings(fileName);

                this.Title = string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.WindowTitle)
                    ? "Avalon Mud Client"
                    : App.Settings.ProfileSettings.WindowTitle;

                App.Conveyor.EchoLog($"Settings Loaded: {fileName}", LogType.Success);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }
        }

        /// <summary>
        /// Loads any plugins found in any DLL in the plugins folder if the IP address for that plugin matches
        /// the IP address that the user is currently connecting to.
        ///
        /// TODO - cleanup
        /// 
        /// </summary>
        private void LoadPlugins()
        {
            // Clear the system triggers
            App.SystemTriggers.Clear();

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
                    App.Conveyor.EchoLog($"Plugin File Found: {Argus.IO.FileSystemUtilities.ExtractFileName(file)}", LogType.Information);

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

                    // The IP address specified in the plugin matches the IP address we're connecting to.
                    if (pluginInstance.IpAddress.ToUpper() == App.Settings.ProfileSettings.IpAddress.ToUpper())
                    {
                        // Let it run it's own Initialize.
                        pluginInstance.Initialize();

                        // Go through all of the triggers and put them into our system triggers.
                        foreach (var trigger in pluginInstance.Triggers)
                        {
                            trigger.Plugin = true;
                            trigger.Conveyor = App.Conveyor;
                            App.SystemTriggers.Add(trigger);
                        }

                        App.Conveyor.EchoLog($"Plugins Loaded For: {pluginInstance.IpAddress}", LogType.Success);
                        App.Conveyor.EchoLog($"   => {pluginInstance.Triggers.Count()} Triggers Loaded", LogType.Success);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the auto complete values.
        /// </summary>
        public void RefreshAutoCompleteEntries()
        {
            var ac = this.Resources["AutoCompleteCommandProvider"] as AutoCompleteCommandProvider;

            if (ac != null)
            {
                ac.RefreshAutoCompleteEntries();
            }
            else
            {
                this.GameTerminal.Append("Warning: AutoCompleteCommandProvider was not found.", AnsiColors.Yellow);
            }
        }

        /// <summary>
        /// Shows a message box dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
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
        /// TODO - Move to utilities.
        /// </summary>
        /// <param name="window"></param>
        public void ShowDialog(Window window)
        {
            Dispatcher.BeginInvoke(
                new Func<bool?>(() =>
                {
                    window.Owner = Application.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    return window.ShowDialog();
                }));
        }

        /// <summary>
        /// Event for when the settings button is clicked.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void SettingsButton_Click(object o, RoutedEventArgs args)
        {
            var win = new ProfileSettingsWindow();
            this.ShowDialog(win);
        }

        /// <summary>
        /// Event for when the tab control's selected tab changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When the game tab gets the focus always put the focus into the input box.
            if (TabGame.IsSelected && TextInput.Editor != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // In order to get the focus in this instance an UpdateLayout() call has to be called first.
                    UpdateLayout();

                    // When the app is first loaded the Editor was coming up null so we'll just check the nulls
                    // and then default the caret position to 0 if that's the case.
                    TextInput.Editor.CaretIndex = TextInput?.Editor?.Text?.Length ?? 0;
                    TextInput.Editor.Focus();
                }));
            }
        }

        /// <summary>
        /// Exits the mud client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Exits the mud client and SKIPS saving the settings file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonExitWithoutSave_OnClickAsync(object sender, RoutedEventArgs e)
        {
            App.SkipSaveOnExit = true;
            App.Conveyor.EchoLog("Exiting WITHOUT saving the current profile.", LogType.Warning);
            await Task.Delay(1000);
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Saves the currently loaded profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSaveProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Settings.SaveSettings();
                App.Conveyor.EchoLog($"Settings Saved", LogType.Success);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }
        }

        /// <summary>
        /// Opens a profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = App.Settings.AvalonSettings.SaveDirectory;
                dialog.Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*";

                if (dialog.ShowDialog() == true)
                {
                    // Load the settings for the file that was selected.
                    LoadSettings(dialog.FileName);

                    // Inject the Conveyor into the Triggers.
                    foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
                    {
                        trigger.Conveyor = App.Conveyor;
                    }

                    // Important!  This is a new profile, we have to reload ALL of the DataList controls
                    // and reset their bindings.
                    AliasList.Reload();
                    DirectionList.Reload();
                    MacroList.Reload();
                    TriggersList.Reload();
                    VariableList.Reload();

                    // We have a new profile, refresh the auto complete command list.
                    RefreshAutoCompleteEntries();

                    // Auto connect if it's setup to do so (this will disconnect from the previous server if it was connected.
                    if (App.Settings.ProfileSettings.AutoConnect)
                    {
                        Disconnect();
                        Connect();
                    }

                }

            }
            catch (Exception ex)
            {
                Interp.EchoText("");
                Interp.EchoText($"--> An error occured: {ex.Message}.\r\n", AnsiColors.Red);
            }
        }

        /// <summary>
        /// Merges one profile with another.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// We don't need to reload/reset after these have been imported because
        /// the bindings haven't changed, we're loading into what is currently setup.
        /// </remarks>
        private async void ButtonImportProfile_OnClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO - Instead of a straight import everything have this do an upsert if there are matching guids so that packages can be updated.
            try
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = App.Settings.AvalonSettings.SaveDirectory;
                dialog.Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*";

                if (dialog.ShowDialog() == true)
                {
                    var confirmDialog = new YesNoDialog()
                    {
                        Title = "Are you sure?",
                        Content = $"Are you sure you want to import from: {Argus.IO.FileSystemUtilities.ExtractFileName(dialog.FileName)}?",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No"
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result == ContentDialogResult.Secondary)
                    {
                        Interp.EchoText("");
                        Interp.EchoText($"--> Cancelled Import\r\n", AnsiColors.Cyan);
                        return;
                    }

                    // Load the file, then set it as the last loaded file -if- it existed.
                    string json = File.ReadAllText(dialog.FileName);
                    var settings = JsonConvert.DeserializeObject<ProfileSettings>(json);

                    // For now we're using going to import the aliases and triggers
                    foreach (var alias in settings.AliasList)
                    {
                        App.Settings.ProfileSettings.AliasList.Add(alias);
                    }

                    // For now we're using going to import the aliases and triggers
                    foreach (var trigger in settings.TriggerList)
                    {
                        App.Settings.ProfileSettings.TriggerList.Add(trigger);
                    }

                    // Inject the Conveyor into all of the triggers
                    foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
                    {
                        trigger.Conveyor = App.Conveyor;
                    }

                    // Show the user that the profile was successfully loaded.
                    Interp.EchoText("");
                    Interp.EchoText($"--> Imported {dialog.FileName} into the current profile.\r\n", AnsiColors.Cyan);

                }

            }
            catch (Exception ex)
            {
                Interp.EchoText("");
                Interp.EchoText($"--> An error occured: {ex.Message}.\r\n", AnsiColors.Red);
            }
        }

        /// <summary>
        /// Opens the app settings directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemOpenSettingsFolderAsync_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it should)
            if (!Directory.Exists(App.Settings.AppDataDirectory))
            {
                await MsgBox($"The settings folder was not found at:\r\n\r\n{App.Settings.AppDataDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AppDataDirectory);
            }
            catch (Exception ex)
            {
                await MsgBox(ex.Message, "Open Directory Error");
            }
        }

        /// <summary>
        /// Opens the profile save folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemOpenProfilesFolderAsync_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it mostly likely should)
            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                await MsgBox($"The profile folder was not found at:\r\n\r\n{App.Settings.AvalonSettings.SaveDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AvalonSettings.SaveDirectory);
            }
            catch (Exception ex)
            {
                await MsgBox(ex.Message, "Open Directory Error");
            }
        }

        private async void MenuItemOpenPluginsFolderAsync_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it mostly likely should)
            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                await MsgBox($"The plugins folder was not found at:\r\n\r\n{App.Settings.PluginDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.PluginDirectory);
            }
            catch (Exception ex)
            {
                await MsgBox(ex.Message, "Open Directory Error");
            }
        }

        /// <summary>
        /// Shows the client settings dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemClientProperties_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new ClientSettingsWindow();
            win.ShowDialog();
        }

        /// <summary>
        /// Shows the profile settings dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemProfileProperties_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new ProfileSettingsWindow();
            win.ShowDialog();
        }

    }
}