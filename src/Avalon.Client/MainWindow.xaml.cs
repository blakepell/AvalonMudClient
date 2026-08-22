/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common;
using Avalon.Common.Models;
using Avalon.Common.Settings;
using Avalon.Controls;
using Avalon.Controls.AutoCompleteTextBox;
using Avalon.Timers;
using Avalon.Utilities;
using Microsoft.Win32;
using Mosaic.UI.Wpf.AvalonDock.Layout;
using Mosaic.UI.Wpf.Themes;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly AutoCompleteCommandProvider _autoCompleteCommandProvider = new();

        internal TerminalUnreadCounter CustomTab1Badge { get; } = new();

        internal TerminalUnreadCounter CustomTab2Badge { get; } = new();

        internal TerminalUnreadCounter CustomTab3Badge { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            TextInput.Provider = _autoCompleteCommandProvider;

            Terminal1Tool.IsActiveChanged += (_, _) => HandleTerminalToolActivation(Terminal1Tool, Terminal1, CustomTab1Badge);
            Terminal2Tool.IsActiveChanged += (_, _) => HandleTerminalToolActivation(Terminal2Tool, Terminal2, CustomTab2Badge);
            Terminal3Tool.IsActiveChanged += (_, _) => HandleTerminalToolActivation(Terminal3Tool, Terminal3, CustomTab3Badge);
            ThemeManager.ThemeChanged += (_, _) =>
                DockingManager.Theme = new Mosaic.UI.Wpf.AvalonDock.Themes.MosaicTheme();

            // Set the global reference.
            App.MainWindow = this;
        }

        /// <summary>
        /// Toggles the active Mosaic color theme.
        /// </summary>
        private void ButtonToggleTheme_OnClick(object sender, RoutedEventArgs e)
        {
            App.ToggleMosaicTheme();
        }

        /// <summary>
        /// Gets whether the single active game session is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// The input history from the user so we can scroll back through it, etc.
        /// </summary>
        public Interpreter Interp;

        /// <summary>
        /// A view model to house various data elements that should be displayed on the main game window.
        /// </summary>
        public MainWindowViewModel ViewModel = new();

        /// <summary>
        /// Timer to time ticks with.
        /// </summary>
        public TickTimer TickTimer;

        /// <summary>
        /// A queue of commands that can be executed as a batch in the order they are added.
        /// </summary>
        public BatchTasks BatchTasks;

        /// <summary>
        /// A queue of SQL commands to execute in buffered batches.
        /// </summary>
        public SqlTasks SqlTasks;

        /// <summary>
        /// A queue of commands that will run after a specified time.
        /// </summary>
        public ScheduledTasks ScheduledTasks { get; set; }

        /// <summary>
        /// The Loaded event for the Window where we will execute code that should run when the Window
        /// is first put into place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Keep this HWND alive while the connection manager is open. Mosaic's chrome behavior
            // installs native window hooks during SourceInitialized; hiding and showing the window
            // again from inside Loaded can tear down and re-enter those callbacks while layout is
            // changing. The window starts transparent instead and is revealed after selection.

            if (Utilities.Utilities.IsRunningAsUwp())
            {
                // The settings for the app load in the app startup, they will then try to load the last profile that was used.
                App.Conveyor.EchoInfo($"{{GA{{gvalon {{GM{{gud {{GC{{glient{{x: Version {Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "Unknown"} (UWP)");
                MenuItemUpdateClient.Visibility = Visibility.Collapsed;
            }
            else
            {
                // The settings for the app load in the app startup, they will then try to load the last profile that was used.
                App.Conveyor.EchoInfo($"{{GA{{gvalon {{GM{{gud {{GC{{glient{{x: Version {Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "Unknown"}");
            }

            try
            {
                int count = Utilities.Utilities.UpdatePlugins();

                if (count == 1)
                {
                    App.Conveyor.EchoSuccess($"{count.ToString()} plugin was updated.");
                }
                else if (count > 1)
                {
                    App.Conveyor.EchoSuccess($"{count.ToString()} plugins were updated.");
                }
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError("An error occurred copying updated plugins.");
                App.Conveyor.EchoError(ex.Message);
            }

            try
            {
                int count = Utilities.Utilities.CleanupUpdatesFolder();

                if (count > 0)
                {
                    App.Conveyor.EchoInfo($"{count.ToString()} files(s) deleted from the updates folder.");
                }
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError("An error occurred removing old updates from the updates folder.");
                App.Conveyor.EchoError(ex.Message);
            }

            try
            {
                this.DataContext = this.ViewModel;

                // Wire up any events that have to be wired up through code.
                TextInput.Editor.PreviewKeyDown += this.Editor_PreviewKeyDown;

                // Pass the necessary reference from this page to the Interpreter.  Add the interpreter
                // references.
                this.Interp = AppServices.GetService<Interpreter>();

                // Setup the handler so when it wants to write to the main window it can by raising the echo event.
                Interp.Echo += this.InterpreterEcho;

                // Setup the tick timer.
                TickTimer = new TickTimer(App.Conveyor);

                // Setup the scheduled and batch tasks.
                this.ScheduledTasks = new ScheduledTasks(this.Interp);
                this.BatchTasks = new BatchTasks(this.Interp);

                // Load any plugin classes from the plugins folder.  They will be "activated" when a mud who matches
                // the plugin IP is connected to.
                this.LoadPlugins();

                // Settings and SQLite are available on demand from the regular menu.
                SettingsTool.Hide();

            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError("A critical error on startup occurred.");
                App.Conveyor.EchoError(ex.Message);
                App.Conveyor.EchoError(ex?.StackTrace ?? "No stack trace available.");
            }

            // Show the connection manager, if they don't select something, exit the entire program.
            var cm = new ConnectionManagerWindow();
            var result = cm.ShowDialog();

            if (!result.GetValueOrDefault(false))
            {
                App.InstanceGlobals.SkipSaveOnExit = true;
                Application.Current.Shutdown(0);
            }

            // We're at the end of load, reveal the already-created window without recreating or
            // re-entering its native chrome lifecycle.
            ShowInTaskbar = true;
            Opacity = 1;
            Activate();
        }

        /// <summary>
        /// Loads the settings for the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadSettings(string fileName)
        {
            try
            {
                // Setup the view model binding to the client settings.
                this.ViewModel.AvalonSettings = App.Settings.AvalonSettings;

                // Load the profile settings from the requested file.
                App.Settings.LoadSettings(fileName);

                // Setup the view model binding to the profile settings.
                this.ViewModel.ProfileSettings = App.Settings.ProfileSettings;

                // Any manually references that should occur.
                VariableRepeater.Bind();

                // The real title bar that Windows will show in the task bar.
                if (string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.WindowTitle))
                {
                    App.Settings.ProfileSettings.WindowTitle = "Avalon Mud Client";
                }

                App.Conveyor.EchoLog($"Settings Loaded: {fileName}", LogType.Success);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, LogType.Error);
            }
        }

        /// <summary>
        /// Updates any properties that need to be updated after the settings are updated.  This deals primarily
        /// with properties that aren't bound through dependency properties.
        /// </summary>
        public void UpdateUISettings()
        {
            // Terminal font
            FontFamily font;

            switch (App.Settings.AvalonSettings.TerminalFont)
            {
                case AvalonSettings.TerminalFonts.Consolas:
                    this.ViewModel.TerminalFontFamily = new FontFamily("Consolas");
                    break;
                case AvalonSettings.TerminalFonts.CourierNew:
                    this.ViewModel.TerminalFontFamily = new FontFamily("Courier New");
                    break;
                default:
                    this.ViewModel.TerminalFontFamily = new FontFamily("Consolas");
                    break;
            }

            this.ViewModel.SpellCheckEnabled = App.Settings.ProfileSettings.SpellChecking;

            // Scroll everything to the last line in case heights/widths/wrapping has changed.
            GameTerminal.ScrollToLastLine();
            Terminal1.ScrollToLastLine();
            Terminal2.ScrollToLastLine();
            Terminal3.ScrollToLastLine();

            // This will allow the main window to go maximize and not cover the task bar on the main window
            // but will maximize over the task bar on 2nd monitors.
            // TODO - Make this work on all monitors
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            // The number of seconds in between batch writes for the the database wrapper.
            SqlTasks.SetInterval(App.Settings.AvalonSettings.DatabaseWriteInterval);

            // Apply the border settings by trigger the SizeChanged event.
            this.MainPage_SizeChanged(null, null);

            SetToolVisibility(Terminal1Tool, App.Settings.AvalonSettings.CustomTab1Visible);
            SetToolVisibility(Terminal2Tool, App.Settings.AvalonSettings.CustomTab2Visible);
            SetToolVisibility(Terminal3Tool, App.Settings.AvalonSettings.CustomTab3Visible);
            SetToolVisibility(VariablesTool, App.Settings.AvalonSettings.VariableRepeaterVisible);

            // Grid Layout
            LoadGridState();
        }

        /// <summary>
        /// Refreshes the auto complete values.
        /// </summary>
        public void RefreshAutoCompleteEntries()
        {
            _autoCompleteCommandProvider.RefreshAutoCompleteEntries();
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
            App.InstanceGlobals.SkipSaveOnExit = true;
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
                App.Settings.ProfileSettings.LastInteractiveLuaScript = LuaEditor.Editor.Text;
                App.MainWindow.SaveGridState();
                App.Settings.SaveSettings();
                App.Conveyor.EchoText("\r\n");
                App.Conveyor.EchoLog("Settings Saved", LogType.Success);
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
            var dialog = new OpenFileDialog
            {
                InitialDirectory = App.Settings.AvalonSettings.SaveDirectory,
                Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                await this.OpenProfile(dialog.FileName);
            }
        }

        /// <summary>
        /// Opens a profile and handles any errors that occur.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public async Task OpenProfile(string filePath, string ipAddress = "", int port = 0)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    this.Interp.Conveyor.EchoError($"File not found: {filePath}");
                    return;
                }

                // Load the settings for the file that was selected.
                this.LoadSettings(filePath);

                // Restore the last script the user was working on if they were.
                LuaEditor.Editor.Text = App.Settings.ProfileSettings.LastInteractiveLuaScript;

                // Inject the Conveyor/ScriptHost into the Triggers and aliases including
                // initial loading of scripts.
                Utilities.Utilities.SetupTriggers();
                Utilities.Utilities.SetupAliases();

                // We have a new profile, refresh the auto complete command list.
                this.RefreshAutoCompleteEntries();

                // Update static variables from places that might be expensive to populate from (like Environment.Username).  Normally
                // something like that isn't expensive but when run on variable replacement it can be more noticeable.
                Utilities.Utilities.UpdateCommonVariables();

                // Loads the initial list of items for the navigation slide out.
                this.ViewModel.NavManager.Load();

                // Close and open a connection to the database for the new profile.
                if (this.SqlTasks == null)
                {
                    this.SqlTasks = new SqlTasks($"Data Source={App.Settings.ProfileSettings.SqliteDatabase}");
                }

                await SqlTasks.OpenAsync($"Data Source={App.Settings.ProfileSettings.SqliteDatabase}");
                SqliteQuery.DatabaseFilePath = App.Settings.ProfileSettings.SqliteDatabase;

                // Update any UI settings
                this.UpdateUISettings();
                GameDocument.IsSelected = true;
                GameDocument.IsActive = true;

                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    App.Settings.ProfileSettings.IpAddress = ipAddress;
                }

                if (port > 0)
                {
                    App.Settings.ProfileSettings.Port = port;
                }

                // Auto connect if it's setup to do so (this will disconnect from the previous server if it was connected.
                if (App.Settings.ProfileSettings.AutoConnect)
                {
                    this.Disconnect();
                    await this.Connect();
                }

                // Is there an auto execute command or set of commands to run?
                if (!string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.AutoExecuteCommand))
                {
                    // Send the auto execute command if the user is connected.
                    if (this?.Interp?.Telnet != null && this.Interp.Telnet.IsConnected())
                    {
                        await Interp.Send(App.Settings.ProfileSettings.AutoExecuteCommand, true, false);
                    }
                    else
                    {
                        App.Conveyor.EchoWarning("Auto Execute Command Skipped: Connection to server was closed.");
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
                        this.WindowState = (WindowState)App.Settings.AvalonSettings.LastWindowPosition.WindowState;
                        this.Left = App.Settings.AvalonSettings.LastWindowPosition.Left;
                        this.Top = App.Settings.AvalonSettings.LastWindowPosition.Top;
                        this.Height = App.Settings.AvalonSettings.LastWindowPosition.Height;
                        this.Width = App.Settings.AvalonSettings.LastWindowPosition.Width;
                        break;
                }

                // Finally, all is done, set the focus to the command box.
                TextInput.Focus();
            }
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoError($"{ex.Message}.\r\n");
                this.Interp.Conveyor.EchoError($"{ex.StackTrace}\r\n");
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
            try
            {
                var dialog = new OpenFileDialog
                {
                    InitialDirectory = App.Settings.AvalonSettings.SaveDirectory,
                    Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    var result = await WindowManager.InputBox($"Are you sure you want to import from: {Argus.IO.FileSystemUtilities.ExtractFileName(dialog.FileName)}?", "Are you sure?");

                    if (!result)
                    {
                        Interp.EchoText("");
                        Interp.Conveyor.EchoLog("Cancelled Import\r\n", LogType.Warning);
                        return;
                    }

                    // Load the file, then set it as the last loaded file -if- it existed.
                    string json = await File.ReadAllTextAsync(dialog.FileName);

                    // This will update this profile with the items from the json package.
                    App.Settings.ImportPackageFromJson(json, this.Interp.ScriptHost);

                    // Show the user that the profile was successfully loaded.
                    Interp.EchoText("");
                    Interp.Conveyor.EchoLog($"Imported the package {dialog.FileName} into the current profile.\r\n", LogType.Success);
                }

            }
            catch (Exception ex)
            {
                Interp.EchoText("");
                Interp.Conveyor.EchoLog($"An error occurred: {ex.Message}.\r\n", LogType.Error);
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
                await WindowManager.MsgBox($"The settings folder was not found at:\r\n\r\n{App.Settings.AppDataDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AppDataDirectory);
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Open Directory Error");
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
                await WindowManager.MsgBox($"The profile folder was not found at:\r\n\r\n{App.Settings.AvalonSettings.SaveDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.AvalonSettings.SaveDirectory);
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Open Directory Error");
            }
        }

        private async void MenuItemOpenPluginsFolderAsync_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the directory exists (it mostly likely should)
            if (!Directory.Exists(App.Settings.AvalonSettings.SaveDirectory))
            {
                await WindowManager.MsgBox($"The plugins folder was not found at:\r\n\r\n{App.Settings.PluginDirectory}", "Directory not found");
                return;
            }

            try
            {
                Process.Start("explorer.exe", App.Settings.PluginDirectory);
            }
            catch (Exception ex)
            {
                await WindowManager.MsgBox(ex.Message, "Open Directory Error");
            }
        }


        /// <summary>
        /// Shows the profile settings dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(SettingsTool);
        }

        private void ButtonApplySettings_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateUISettings();
            App.Settings.SaveSettings();
        }

        private void MenuItemDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            ShowDatabaseToolWindow();
        }

        private async void MenuItemExternalTool_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: string title })
            {
                return;
            }

            var item = ViewModel.NavManager.NavMenuItems.FirstOrDefault(x =>
                x.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                await item.ExecuteAsync();
            }
            else
            {
                App.Conveyor.EchoWarning($"{title} is not configured in the active profile.");
            }
        }

        private void MenuItemToolWindow_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: string toolName })
            {
                return;
            }

            switch (toolName)
            {
                case "Lua":
                    ActivateToolWindow(LuaTool);
                    break;
                case "SQLite":
                    ActivateToolWindow(DatabaseTool);
                    break;
                case "Terminal1":
                    ActivateToolWindow(Terminal1Tool);
                    break;
                case "Terminal2":
                    ActivateToolWindow(Terminal2Tool);
                    break;
                case "Terminal3":
                    ActivateToolWindow(Terminal3Tool);
                    break;
                case "Bars":
                    ActivateToolWindow(BarsTool);
                    break;
            }
        }

        private void ActivateToolWindow(LayoutAnchorable tool)
        {
            if (!tool.IsVisible)
            {
                if (tool.IsHidden)
                {
                    tool.Show();
                }
                else
                {
                    tool.AddToLayout(DockingManager, AnchorableShowStrategy.Right);
                }
            }

            if (tool.IsFloating)
            {
                tool.Dock();
            }

            tool.IsSelected = true;
            tool.IsActive = true;
        }

        /// <summary>
        /// Preserves a closable tool window in AvalonDock's hidden collection so it can be reopened.
        /// </summary>
        private void PersistentToolWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (sender is not LayoutAnchorable tool)
            {
                return;
            }

            e.Cancel = true;
            Dispatcher.BeginInvoke(new Action(tool.Hide));
        }

        private static void SetToolVisibility(LayoutAnchorable tool, bool isVisible)
        {
            if (isVisible && !tool.IsVisible)
            {
                tool.Show();
            }
            else if (!isVisible && tool.IsVisible)
            {
                tool.Hide();
            }
        }

        /// <summary>
        /// Shows or hides an auxiliary terminal tool window for scripting APIs.
        /// </summary>
        public void SetTerminalToolVisibility(CustomTab tab, bool isVisible)
        {
            switch (tab)
            {
                case CustomTab.Tab1:
                    SetToolVisibility(Terminal1Tool, isVisible);
                    App.Settings.AvalonSettings.CustomTab1Visible = isVisible;
                    break;
                case CustomTab.Tab2:
                    SetToolVisibility(Terminal2Tool, isVisible);
                    App.Settings.AvalonSettings.CustomTab2Visible = isVisible;
                    break;
                case CustomTab.Tab3:
                    SetToolVisibility(Terminal3Tool, isVisible);
                    App.Settings.AvalonSettings.CustomTab3Visible = isVisible;
                    break;
            }
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
            var win = new StringEditor
            {
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ActionButtonText = "Send"
            };

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                foreach (string line in win.Text.Split(Environment.NewLine))
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
            var win = new StringEditor
            {
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "Simulate Incoming Text",
                ActionButtonText = "Simulate",
                StatusText = "Although a simulation of incoming text, triggers and aliases will actually fire."
            };

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                foreach (string line in win.Text.Split(Environment.NewLine))
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
            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            var win = new StringEditor
            {
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ActionButtonText = "Save",
                EditorMode = StringEditor.EditorType.Lua,
                StatusText = "Global Lua File",
                Text = App.Settings?.ProfileSettings?.LuaGlobalScript ?? ""
            };

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

            await confirmDialog.ShowAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Shows the update client dialog for only the plugins.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemUpdatePlugins_Click(object sender, RoutedEventArgs e)
        {
            await WindowManager.ShellWindowAsync("UpdateDLLPlugin");
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
                if (TextInput.Editor != null)
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
                this.Interp.Conveyor.EchoLog("An error occurred setting the focus on the input box when the window received focus.", LogType.Error);
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
            var win = new RegexTestWindow
            {
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CancelButtonText = "Close",
                SaveButtonVisible = false
            };

            win.Show();
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
            Row4.Height = new GridLength(0.0, GridUnitType.Pixel);
            Row5.Height = new GridLength(30.0, GridUnitType.Pixel);
            Row6.Height = new GridLength(32.0, GridUnitType.Pixel);
            Col1.Width = new GridLength(55.0, GridUnitType.Star);
            Col2.Width = new GridLength(45.0, GridUnitType.Star);

            // Make the variable repeater visible.
            ButtonVariableRepeaterVisible.IsChecked = true;
            ButtonVariableRepeaterVisible_Click(null, null);

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

        private void HandleTerminalToolActivation(LayoutAnchorable tool, AvalonTerminal terminal, TerminalUnreadCounter badge)
        {
            if (!tool.IsActive || TextInput.Editor == null)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                badge.Value = 0;
                UpdateLayout();
                terminal.ScrollToEnd();
                TextInput.Editor.CaretIndex = TextInput.Editor.Text?.Length ?? 0;
                TextInput.Editor.Focus();
            }));
        }

        /// <summary>
        /// Show the editor to enable/disable and change the commands to send on a tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemTickTimerCommands_Click(object sender, RoutedEventArgs e)
        {
            var win = new TickCommandEditor
            {
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            _ = win.ShowDialog();
        }

        /// <summary>
        /// Attempts to open the editor last edited alias or trigger.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemOpenLastEditedAliasOrTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (App.InstanceGlobals.LastEdited == InstanceGlobals.EditItem.None)
            {
                await WindowManager.MsgBox("No triggers or aliases have been edited this session.", "Edit Last Alias or Trigger");
                return;
            }

            if (App.InstanceGlobals.LastEdited == InstanceGlobals.EditItem.Alias)
            {
                // Get the alias from the current line.
                var alias = App.Settings.ProfileSettings.AliasList.FirstOrDefault(x => x.AliasExpression.Equals(App.InstanceGlobals.LastEditedId, StringComparison.OrdinalIgnoreCase));

                // Hmm, no alias.. gracefully exit.
                if (alias == null)
                {
                    await WindowManager.MsgBox("No alias you wish to edit could not be found or no longer exists.", "Edit Last Alias or Trigger");
                    return;
                }

                // Set the initial text for the editor.
                var win = new StringEditor
                {
                    Text = alias.Command
                };

                // Set the initial type for highlighting.
                if (alias.ExecuteAs == ExecuteType.LuaMoonsharp)
                {
                    win.EditorMode = StringEditor.EditorType.Lua;
                }

                // Show what alias is being edited in the status bar of the string editor window.
                win.StatusText = $"Alias: {alias.AliasExpression}";

                // Startup position of the dialog should be in the center of the parent window.  The
                // owner has to be set for this to work.
                win.Owner = App.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Show the Lua dialog.
                var result = win.ShowDialog();

                // If the result
                if (result != null && result.Value)
                {
                    alias.Command = win.Text;
                }

            }
            else if (App.InstanceGlobals.LastEdited == InstanceGlobals.EditItem.Trigger)
            {
                // Get the Trigger from the current line.
                var trigger = App.Settings.ProfileSettings.TriggerList.FirstOrDefault(x => x.Identifier.Equals(App.InstanceGlobals.LastEditedId, StringComparison.OrdinalIgnoreCase));

                // Hmm, no Trigger.. gracefully exit.
                if (trigger == null)
                {
                    await WindowManager.MsgBox("No trigger you wish to edit could not be found or no longer exists.", "Edit Last Alias or Trigger");
                    return;
                }

                var win = new TriggerEditorWindow(trigger)
                {
                    StatusText = $"This trigger has fired {trigger.Count.ToString().FormatIfNumber(0)} times."
                };

                // Save the last item and type so the Control+Alt+L alias can re-open it.
                App.InstanceGlobals.LastEditedId = trigger.Identifier;
                App.InstanceGlobals.LastEdited = InstanceGlobals.EditItem.Trigger;

                // Startup position of the dialog should be in the center of the parent window.  The
                // owner has to be set for this to work.
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                // Show the Trigger editor window.
                win.Show();
            }
        }

        /// <summary>
        /// Allows the user to change their current character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemChangeCharacter_Click(object sender, RoutedEventArgs e)
        {
            this.Interp.Conveyor.InputBoxToVariable("What is your current character name that is logged in?", "Set Character Name", "Character");
        }

        /// <summary>
        /// If the variable bar is suppressed update the layout of other row/columns.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonVariableRepeaterVisible_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonVariableRepeaterVisible.IsChecked == true)
            {
                ActivateToolWindow(VariablesTool);
            }
            else
            {
                VariablesTool.Hide();
            }
        }

        /// <summary>
        /// Opens and activates the SQLite tool window hosted by the main docking manager.
        /// </summary>
        public void ShowDatabaseToolWindow()
        {
            ActivateToolWindow(DatabaseTool);
        }

        /// <summary>
        /// Event used from menus to shell any number of Shell based UserControls into windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuShellWindow_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;
            string requestedWindow = menuItem.Tag.ToString();
            await WindowManager.ShellWindowAsync(requestedWindow);
        }

        /// <summary>
        /// NavBar menu item clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NavBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NavMenuItem navMenuItem)
            {
                await navMenuItem.ExecuteAsync();
            }
        }

        /// <summary>
        /// Echos information from the #lua-debug to the main terminal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlStatus_OnClick(object sender, RoutedEventArgs e)
        {
            this.Interp.Send("#sql-debug", true, false);
            TextInput.Editor.Focus();
        }

        /// <summary>
        /// Echos information from the #lua-debug to the main terminal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LuaStatus_OnClick(object sender, RoutedEventArgs e)
        {
            this.Interp.Send("#lua-debug", true, false);
            TextInput.Editor.Focus();
        }

        /// <summary>
        /// Echos information from the #task-list to the main terminal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduledTaskStatus_OnClick(object sender, RoutedEventArgs e)
        {
            this.Interp.Send("#task-list", true, false);
            TextInput.Editor.Focus();
        }

        /// <summary>
        /// Logic to handle when the UI changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// With a no border window WPF will maximize the window 8 pixels (4 on each side) wider than the actual
        /// screen.  The result is that the window extends slightly off the edges of the monitor.  As a result, we're
        /// going to apply a thickness of the border depending on whether the Window is maximized or not.
        /// </remarks>
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (App.Settings.AvalonSettings.ShowMainWindowBorder)
            {
                this.BorderThickness = this.WindowState == WindowState.Maximized ? new Thickness(8) : new Thickness(1);
            }
            else
            {
                this.BorderThickness = this.WindowState == WindowState.Maximized ? new Thickness(6) : new Thickness(0);
            }
        }

        /// <summary>
        /// Shows the connection manager window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemConnectionManager_OnClick(object sender, RoutedEventArgs e)
        {
            var cm = new ConnectionManagerWindow();
            cm.ShowDialog();
        }
    }

    /// <summary>
    /// Tracks unread output for an auxiliary terminal without creating a hidden visual control.
    /// </summary>
    internal sealed class TerminalUnreadCounter
    {
        public int Value { get; set; }

        public void IncrementOrReset(bool shouldIncrement)
        {
            Value = shouldIncrement ? Value + 1 : 0;
        }
    }
}
