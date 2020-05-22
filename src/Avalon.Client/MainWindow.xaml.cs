using Avalon.Common.Colors;
using Avalon.Common.Settings;
using Avalon.Controls;
using Avalon.Controls.AutoCompleteTextBox;
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
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
using Avalon.Extensions;
using ModernWpf;

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
        /// A queue of commands that will run after a specified time.
        /// </summary>
        public ScheduledTasks ScheduledTasks;

        /// <summary>
        /// A queue of commands that can be executed as a batch in the order they are added.
        /// </summary>
        public BatchTasks BatchTasks;


        /// <summary>
        /// Whether spell checking is currently enabled.
        /// </summary>
        public bool SpellCheckEnabled
        {
            get { return (bool)GetValue(SpellCheckEnabledProperty); }
            set { SetValue(SpellCheckEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpellCheckEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpellCheckEnabledProperty =
            DependencyProperty.Register("SpellCheckEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


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
            // The settings for the app load in the app startup, they will then try to load the last profile that was used.
            App.Conveyor.EchoLog($"Avalon Mud Client Version {Assembly.GetExecutingAssembly()?.GetName()?.Version.ToString() ?? "Unknown"}", LogType.Information);

            try
            {
                int count = Utilities.Utilities.UpdatePlugins();

                if (count == 1)
                {
                    App.Conveyor.EchoLog($"{count} plugin was updated.", LogType.Information);
                }
                else if (count > 1)
                {
                    App.Conveyor.EchoLog($"{count} plugins were updated.", LogType.Information);
                }
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog("An error occured copying updated plugins.", LogType.Error);
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }

            try
            {
                int count = Utilities.Utilities.CleanupUpdatesFolder();

                if (count > 0)
                {
                    App.Conveyor.EchoLog($"{count} files(s) deleted from the updates folder.", LogType.Information);
                }
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog("An error occured removing old updates from the updates folder.", LogType.Error);
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }

            try
            {
                if (App.Settings.AvalonSettings.DeveloperMode)
                {
                    App.Conveyor.EchoLog($"Global Settings Folder: {App.Settings.AppDataDirectory}", LogType.Information);
                    App.Conveyor.EchoLog($"Global Settings File:   {App.Settings.AvalonSettingsFile}", LogType.Information);
                    App.Conveyor.EchoLog($"Profiles Folder: {App.Settings.AvalonSettings.SaveDirectory}", LogType.Information);
                }

                // Parse the command line arguments to see if a profile was specified.
                var args = Environment.GetCommandLineArgs();

                // Try to load the last profile loaded, if not found create a new profile.
                if (File.Exists(App.Settings.AvalonSettings.LastLoadedProfilePath))
                {
                    await LoadSettings(App.Settings.AvalonSettings.LastLoadedProfilePath);
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

                // TODO - Setting to disable and command to view these tasks.
                // Setup the scheduled and batch tasks.
                ScheduledTasks = new ScheduledTasks(this.Interp);
                BatchTasks = new BatchTasks(this.Interp);

                // Setup the auto complete commands.  If they're found refresh them, if they're not
                // report it to the terminal window.  It should -always be found-.
                RefreshAutoCompleteEntries();

                // Update static variables from places that might be expensive to populate from (like Environment.Username).  Normally
                // something like that isn't expensive but when run on variable replacement it can be more noticable.
                Utilities.Utilities.UpdateCommonVariables();

                // Load any plugin classes from the plugins folder.  They will be "activated" when a mud who matches
                // the plugin IP is connected to.
                LoadPlugins();

                // Update any UI settings
                UpdateUISettings();

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
            catch (Exception ex)
            {
                App.Conveyor.EchoLog("A critical error on startup occured.", LogType.Error);
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
                App.Conveyor.EchoText(ex?.StackTrace?.ToString() ?? "No stack trace available.");
                return;
            }
        }

        /// <summary>
        /// Loads the settings for the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        private async Task LoadSettings(string fileName)
        {
            try
            {
                App.Settings.LoadSettings(fileName);

                this.Title = string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.WindowTitle)
                    ? "Avalon Mud Client"
                    : App.Settings.ProfileSettings.WindowTitle;

                App.Conveyor.EchoLog($"Settings Loaded: {fileName}", LogType.Success);

                // Setup the database control.            
                try
                {
                    if (App.Settings.AvalonSettings.DeveloperMode)
                    {
                        App.Conveyor.EchoLog($"Initializing SQLite Database: {App.Settings.ProfileSettings.SqliteDatabase}", LogType.Information);
                    }

                    SqliteQueryControl.ConnectionString = $"Data Source={App.Settings.ProfileSettings.SqliteDatabase}";
                    await SqliteQueryControl.RefreshSchema();
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoLog(ex.Message, LogType.Error);
                }

            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }
        }

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
                        App.Conveyor.EchoLog($"   => Loaded For: {pluginInstance.IpAddress}", LogType.Success);
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
                        App.SystemTriggers.Add(trigger);
                    }

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
                        if (Interp.HashCommands.Count(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)) == 0)
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

                    App.Conveyor.EchoLog($"Plugins Loaded For: {plugin.IpAddress}", LogType.Success);
                    App.Conveyor.EchoLog($"   => {plugin.Triggers.Count()} Triggers Loaded", LogType.Success);
                }

            }

        }

        /// <summary>
        /// Updates any dependency properties with the value from the settings.  Notably this will be called
        /// from the settings dialog when it closes.
        /// </summary>
        public void UpdateUISettings()
        {
            // Terminal font size
            this.GameTerminal.FontSize = App.Settings.AvalonSettings.TerminalFontSize;
            this.Terminal1.FontSize = App.Settings.AvalonSettings.TerminalFontSize;
            this.Terminal2.FontSize = App.Settings.AvalonSettings.TerminalFontSize;
            this.Terminal3.FontSize = App.Settings.AvalonSettings.TerminalFontSize;
            this.GameBackBufferTerminal.FontSize = App.Settings.AvalonSettings.TerminalFontSize;

            // Terminal font
            FontFamily font;

            switch (App.Settings.AvalonSettings.TerminalFont)
            {
                case AvalonSettings.TerminalFonts.Consolas:
                    font = new FontFamily("Consolas");
                    break;
                case AvalonSettings.TerminalFonts.CourierNew:
                    font = new FontFamily("Courier New");
                    break;
                default:
                    font = new FontFamily("Consolas");
                    break;
            }

            this.GameTerminal.FontFamily = font;
            this.Terminal1.FontFamily = font;
            this.Terminal2.FontFamily = font;
            this.Terminal3.FontFamily = font;
            this.GameBackBufferTerminal.FontFamily = font;

            this.SpellCheckEnabled = App.Settings.ProfileSettings.SpellChecking;

            if (App.Settings.AvalonSettings.DeveloperMode)
            {
                this.MenuItemDeveloperTools.Visibility = Visibility.Visible;
            }
            else
            {
                this.MenuItemDeveloperTools.Visibility = Visibility.Collapsed;
            }

            // Line numbers
            GameTerminal.ShowLineNumbers = App.Settings.AvalonSettings.ShowLineNumbersInGameTerminal;
            GameBackBufferTerminal.ShowLineNumbers = App.Settings.AvalonSettings.ShowLineNumbersInGameTerminal;

            // Word wrap
            GameTerminal.WordWrap = App.Settings.AvalonSettings.WordWrapTerminals;
            Terminal1.WordWrap = GameTerminal.WordWrap = App.Settings.AvalonSettings.WordWrapTerminals;
            Terminal2.WordWrap = GameTerminal.WordWrap = App.Settings.AvalonSettings.WordWrapTerminals;
            Terminal3.WordWrap = GameTerminal.WordWrap = App.Settings.AvalonSettings.WordWrapTerminals;

            // Scroll everything to the last line in case heights/widths/wrapping has changed.
            GameTerminal.ScrollToLastLine();
            Terminal1.ScrollToLastLine();
            Terminal2.ScrollToLastLine();
            Terminal3.ScrollToLastLine();

            // Custom tabs (Might be overriden by plugins)
            CustomTab1Label.Content = App.Settings.AvalonSettings.CustomTab1Label;
            CustomTab1.Visibility = App.Settings.AvalonSettings.CustomTab1Visible.ToVisibleOrHidden();
            CustomTab2Label.Content = App.Settings.AvalonSettings.CustomTab2Label;
            CustomTab2.Visibility = App.Settings.AvalonSettings.CustomTab2Visible.ToVisibleOrHidden();
            CustomTab3Label.Content = App.Settings.AvalonSettings.CustomTab3Label;
            CustomTab3.Visibility = App.Settings.AvalonSettings.CustomTab3Visible.ToVisibleOrHidden();

            // Grid Layout
            LoadGridState();
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
            var win = new SettingsWindow();
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

                    // This is a little bit of a hack.. terminals stop auto scrolling for some reason when
                    // another tab is active.
                    ScrollAllToBottom();

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
                App.MainWindow.SaveGridState();
                App.Settings.SaveSettings();
                App.Conveyor.EchoText("\r\n");
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
        private async void ButtonOpenProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = App.Settings.AvalonSettings.SaveDirectory;
                dialog.Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*";

                if (dialog.ShowDialog() == true)
                {
                    // Load the settings for the file that was selected.
                    await LoadSettings(dialog.FileName);

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
        private async void ButtonImportPackage_OnClickAsync(object sender, RoutedEventArgs e)
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
                        Interp.Conveyor.EchoLog("Cancelled Import\r\n", LogType.Warning);
                        return;
                    }

                    // Load the file, then set it as the last loaded file -if- it existed.
                    string json = File.ReadAllText(dialog.FileName);

                    // This will update this profile with the items from the json package.
                    App.Settings.ImportPackageFromJson(json);

                    // Show the user that the profile was successfully loaded.
                    Interp.EchoText("");
                    Interp.Conveyor.EchoLog($"Imported the package {dialog.FileName} into the current profile.\r\n", LogType.Success);
                }

            }
            catch (Exception ex)
            {
                Interp.EchoText("");
                Interp.Conveyor.EchoLog($"An error occured: {ex.Message}.\r\n", LogType.Error);
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
        /// Shows the profile settings dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow();
            this.ShowDialog(win);
        }

        /// <summary>
        /// Shows the window to create JSON packages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCreatePackage_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreatePackageWindow();
            this.ShowDialog(win);
        }

        /// <summary>
        /// Sends text to the game with a specified delay between each line.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemSendTextToGameAsync_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new StringEditor();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ActionButtonText = "Send";

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                var lines = win.Text.Split(Environment.NewLine);

                foreach (string line in lines)
                {
                    await this.Interp.Send(line, false, false);
                    await Task.Delay(500);
                }
            }
        }

        /// <summary>
        /// Simulates text as if it were sent by the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemSimulateIncomingTextAsync_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new StringEditor();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.Title = "Simulate Incoming Text";
            win.ActionButtonText = "Simulate";
            win.StatusText = "Although a simulation of incoming text, triggers and aliases will actually fire.";

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                var lines = win.Text.Split(Environment.NewLine);

                foreach (string line in lines)
                {
                    // Like it really came in, send it to the places it should go to display
                    // it and then process it.
                    HandleDataReceived(sender, $"{line}\r\n");
                    HandleLineReceived(sender, line.Trim());
                    await Task.Delay(50);
                }
            }

        }

        /// <summary>
        /// Edits the global Lua file that is loaded into all Lua scripts to allow for shared
        /// Lua logic between all Lua instances.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemEditGlobalLuaFile_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new StringEditor();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ActionButtonText = "Save";
            win.EditorMode = StringEditor.EditorType.Lua;
            win.StatusText = "Global Lua File";
            win.Text = App.Settings?.ProfileSettings?.LuaGlobalScript ?? "";

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                App.Settings.ProfileSettings.LuaGlobalScript = win.Text;
            }
        }

        /// <summary>
        /// Link to the git documentation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDocumentation_Click(object sender, RoutedEventArgs e)
        {
            Utilities.Utilities.ShellLink("https://github.com/blakepell/AvalonMudClient/blob/master/README.md");
        }

        /// <summary>
        /// Link to the git hash commands documentation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemHashCommands_Click(object sender, RoutedEventArgs e)
        {
            Utilities.Utilities.ShellLink("https://github.com/blakepell/AvalonMudClient/blob/master/doc/HashCommands.md");
        }

        /// <summary>
        /// Link to the git Lua documentation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemLua_Click(object sender, RoutedEventArgs e)
        {
            Utilities.Utilities.ShellLink("https://github.com/blakepell/AvalonMudClient/blob/master/doc/Lua.md");
        }

        /// <summary>
        /// A link to the git source code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSourceCode_Click(object sender, RoutedEventArgs e)
        {
            Utilities.Utilities.ShellLink("https://github.com/blakepell/AvalonMudClient");
        }

        /// <summary>
        /// Shows the update client dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemUpdateClient_Click(object sender, RoutedEventArgs e)
        {
            // Save first.
            App.MainWindow.SaveGridState();
            App.Settings.SaveSettings();

            var confirmDialog = new UpdateDialog()
            {
                PluginsOnly = false
            };

            var result = await confirmDialog.ShowAsync();
        }

        /// <summary>
        /// Shows the update client dialog for only the plugins.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemUpdatePlugins_Click(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new UpdateDialog()
            {
                PluginsOnly = true
            };

            var result = await confirmDialog.ShowAsync();

            this.Interp.Conveyor.EchoText("\r\n");
            this.Interp.Conveyor.EchoLog("In order for the plugin updates to take effect you will need to close and then restart this application.", LogType.Warning);
        }

        /// <summary>
        /// Event for when the window gets focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            try
            {
                // When the game tab gets the focus always put the focus into the input box.
                if (TabGame.IsSelected && TextInput.Editor != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // In order to get the focus in this instance an UpdateLayout() call has to be called first.
                        UpdateLayout();

                        // This is a little bit of a hack.. terminals stop auto scrolling for some reason when
                        // another tab is active.
                        ScrollAllToBottom();

                        // When the app is first loaded the Editor was coming up null so we'll just check the nulls
                        // and then default the caret position to 0 if that's the case.
                        TextInput.Editor.CaretIndex = TextInput?.Editor?.Text?.Length ?? 0;
                        TextInput.Editor.Focus();
                    }));
                }
            }
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog("An error occured setting the focus on the input box when the window received focus.", LogType.Error);
                this.Interp.Conveyor.EchoLog(ex.Message, LogType.Error);
            }
        }

        /// <summary>
        /// Shows the regular expression test window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemRegexTester_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new RegexTestWindow();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.CancelButtonText = "Close";
            win.SaveButtonVisible = false;

            // Show the Lua dialog.
            win.ShowDialog();
        }

        /// <summary>
        /// Resets the layout to the default.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemResetLayout_Click(object sender, RoutedEventArgs e)
        {
            ResetGridState();
            SaveGridState();
        }

        /// <summary>
        /// Resets the grid state to the default.
        /// </summary>
        public void ResetGridState()
        {
            Row1.Height = new GridLength(0, GridUnitType.Auto);
            Row2.Height = new GridLength(7.0, GridUnitType.Star);
            Row3.Height = new GridLength(3.0, GridUnitType.Star);
            Row4.Height = new GridLength(1.0, GridUnitType.Auto);
            Row5.Height = new GridLength(1.0, GridUnitType.Auto);
            Col1.Width = new GridLength(55.0, GridUnitType.Star);
            Col2.Width = new GridLength(45.0, GridUnitType.Star);
        }

        /// <summary>
        /// Saves the grids current state into the settings.
        /// </summary>
        public void SaveGridState()
        {
            App.Settings.AvalonSettings.GameGridState.Clear();

            // Save rows.
            for (int i = 0; i < GridGame.RowDefinitions.Count; i++)
            {
                var row = new GridLengthState
                {
                    Length = GridGame.RowDefinitions[i].Height.Value,
                    ElementType = GridLengthState.GridElementType.Row,
                    Index = i
                };

                switch (GridGame.RowDefinitions[i].Height.GridUnitType)
                {
                    case GridUnitType.Star:
                        row.UnitType = GridLengthState.GridUnitType.Star;
                        break;
                    case GridUnitType.Pixel:
                        row.UnitType = GridLengthState.GridUnitType.Pixel;
                        break;
                    case GridUnitType.Auto:
                        row.UnitType = GridLengthState.GridUnitType.Auto;
                        break;
                }

                App.Settings.AvalonSettings.GameGridState.Add(row);
            }

            // Save columns.
            for (int i = 0; i < GridGame.ColumnDefinitions.Count; i++)
            {
                var column = new GridLengthState
                {
                    Length = GridGame.ColumnDefinitions[i].Width.Value,
                    ElementType = GridLengthState.GridElementType.Column,
                    Index = i
                };

                switch (GridGame.ColumnDefinitions[i].Width.GridUnitType)
                {
                    case GridUnitType.Star:
                        column.UnitType = GridLengthState.GridUnitType.Star;
                        break;
                    case GridUnitType.Pixel:
                        column.UnitType = GridLengthState.GridUnitType.Pixel;
                        break;
                    case GridUnitType.Auto:
                        column.UnitType = GridLengthState.GridUnitType.Auto;
                        break;
                }

                App.Settings.AvalonSettings.GameGridState.Add(column);
            }
        }

        /// <summary>
        /// Loads the grids state from the settings.
        /// </summary>
        public void LoadGridState()
        {
            if (App.Settings.AvalonSettings.GameGridState.Count == 0)
            {
                ResetGridState();
                SaveGridState();
                return;
            }

            try
            {
                foreach (var item in App.Settings.AvalonSettings.GameGridState)
                {
                    if (item.ElementType == GridLengthState.GridElementType.Row)
                    {
                        switch (item.UnitType)
                        {
                            case GridLengthState.GridUnitType.Auto:
                                GridGame.RowDefinitions[item.Index].Height = new GridLength(item.Length, GridUnitType.Auto);
                                break;
                            case GridLengthState.GridUnitType.Pixel:
                                GridGame.RowDefinitions[item.Index].Height = new GridLength(item.Length, GridUnitType.Pixel);
                                break;
                            case GridLengthState.GridUnitType.Star:
                                GridGame.RowDefinitions[item.Index].Height = new GridLength(item.Length, GridUnitType.Star);
                                break;
                        }

                    }
                    else if (item.ElementType == GridLengthState.GridElementType.Column)
                    {
                        switch (item.UnitType)
                        {
                            case GridLengthState.GridUnitType.Auto:
                                GridGame.ColumnDefinitions[item.Index].Width = new GridLength(item.Length, GridUnitType.Auto);
                                break;
                            case GridLengthState.GridUnitType.Pixel:
                                GridGame.ColumnDefinitions[item.Index].Width = new GridLength(item.Length, GridUnitType.Pixel);
                                break;
                            case GridLengthState.GridUnitType.Star:
                                GridGame.ColumnDefinitions[item.Index].Width = new GridLength(item.Length, GridUnitType.Star);
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog("The grid layout was saved did not match the current grid layout (possibly because of a client update).  The grid has been reset to the default.", LogType.Error);
                this.Interp.Conveyor.EchoLog(ex.Message, LogType.Error);
                ResetGridState();
                SaveGridState();
            }

        }

        /// <summary>
        /// Manually activate plugins for a specified IP address.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemManuallyLoadPlugin_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = await this.Interp.Conveyor.InputBox("What is the IP Address associated with the plugin you want to load?", "Load Plugin", App.Settings.ProfileSettings.IpAddress);

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return;
            }

            ActivatePlugins(ipAddress);
        }

        /// <summary>
        /// Handles the event when the custom tab selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabCustom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When the game tab gets the focus always put the focus into the input box.
            if (CustomTab1.IsSelected && TextInput.Editor != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Reset the value to 0, it's now been shown.
                    CustomTab1Badge.Value = 0;

                    // In order to get the focus in this instance an UpdateLayout() call has to be called first.
                    UpdateLayout();

                    Terminal1.ScrollToEnd();

                    // When the app is first loaded the Editor was coming up null so we'll just check the nulls
                    // and then default the caret position to 0 if that's the case.
                    TextInput.Editor.CaretIndex = TextInput?.Editor?.Text?.Length ?? 0;
                    TextInput.Editor.Focus();
                }));
            }
            else if (CustomTab2.IsSelected && TextInput.Editor != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Reset the value to 0, it's now been shown.
                    CustomTab2Badge.Value = 0;

                    // In order to get the focus in this instance an UpdateLayout() call has to be called first.
                    UpdateLayout();

                    Terminal2.ScrollToEnd();

                    // When the app is first loaded the Editor was coming up null so we'll just check the nulls
                    // and then default the caret position to 0 if that's the case.
                    TextInput.Editor.CaretIndex = TextInput?.Editor?.Text?.Length ?? 0;
                    TextInput.Editor.Focus();
                }));
            }
            else if (CustomTab3.IsSelected && TextInput.Editor != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Reset the value to 0, it's now been shown.
                    CustomTab3Badge.Value = 0;

                    // In order to get the focus in this instance an UpdateLayout() call has to be called first.
                    UpdateLayout();

                    Terminal3.ScrollToEnd();

                    // When the app is first loaded the Editor was coming up null so we'll just check the nulls
                    // and then default the caret position to 0 if that's the case.
                    TextInput.Editor.CaretIndex = TextInput?.Editor?.Text?.Length ?? 0;
                    TextInput.Editor.Focus();
                }));
            }
        }

        /// <summary>
        /// Show the editor to enable/disable and change the commands to send on a tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemTickTimerCommands_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new TickCommandEditor();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Show the Lua dialog.
            var result = win.ShowDialog();
        }

    }
}