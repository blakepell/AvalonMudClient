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
using CommandLine;
using Microsoft.Win32;
using System.Reflection;
using System.Threading.Tasks;
using Avalon.Common.Interfaces;
using Avalon.Common.Plugins;
using Newtonsoft.Json;

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
        /// The Lua Environment
        /// </summary>
        public Script Lua;

        /// <summary>
        /// Lua control token to pause, stop and resume scripts.
        /// </summary>
        public ExecutionControlToken LuaControl;

        /// <summary>
        /// Timer to time ticks with.
        /// </summary>
        public TickTimer TickTimer;

        /// <summary>
        /// A copy of the conveyor used by hash commands and other abstracted classes.
        /// </summary>
        public Conveyor Conveyor;

        /// <summary>
        /// Window initialization.  This occurs before the Loaded event.  We'll set the initial
        /// window positioning here before the UI is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            // TODO - This causes some issue with Aero snap initially, research.
            // Initialize Screen Position
            this.WindowSize();
        }

        /// <summary>
        /// The Loaded event for the Window where we will execute code that should run when the Window
        /// is first put into place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // The Conveyor will be passed around to other objects so that they can interact with the UI.  This Conveyor may have
            // state so it's important to re-use this object unless sandboxing is needed.
            Conveyor = new Conveyor();

            // The settings for the app load in the app startup, they will then try to load the last profile
            // that was used.
            GameTerminal.Append($"Default Settings Folder: {App.Settings.AppDataDirectory}\r\n", AnsiColors.Cyan);
            GameTerminal.Append($"Core Settings File:      {App.Settings.AvalonSettingsFile}\r\n", AnsiColors.Cyan);
            GameTerminal.Append($"Current Profiles Folder: {App.Settings.AvalonSettings.SaveDirectory}\r\n", AnsiColors.Cyan);

            // Parse the command line arguments to see if a profile was specified.
            var args = Environment.GetCommandLineArgs();

            // TODO - This settings loading needs to be streamlined, it's too convoluted.
            Parser.Default.ParseArguments<CommandLineArguments>(args)
                  .WithParsed<CommandLineArguments>(o =>
                  {
                      if (!string.IsNullOrWhiteSpace(o.Profile))
                      {
                          // A profile was specified, try to use that.
                          string profilePath = Path.Join(App.Settings.AvalonSettings.SaveDirectory, o.Profile);

                          if (File.Exists(profilePath))
                          {
                              App.Settings.LoadSettings(profilePath);
                              GameTerminal.Append($"Settings Loaded:         {App.Settings.AvalonSettings.LastLoadedProfilePath}\r\n", AnsiColors.Cyan);
                          }
                          else
                          {
                              GameTerminal.Append($"Settings Not Found:      {profilePath}\r\n", AnsiColors.Red);
                              App.Settings.LoadSettings(App.Settings.AvalonSettings.LastLoadedProfilePath);
                              GameTerminal.Append($"Settings Loaded:         {App.Settings.AvalonSettings.LastLoadedProfilePath}\r\n", AnsiColors.Cyan);
                          }
                      }
                      else
                      {
                          // No profile was specified, try to use the last one if there was no last one then load up
                          // a new default profile.
                          if (File.Exists(App.Settings.AvalonSettings.LastLoadedProfilePath))
                          {
                              App.Settings.LoadSettings(App.Settings.AvalonSettings.LastLoadedProfilePath);
                              GameTerminal.Append($"Settings Loaded:         {App.Settings.AvalonSettings.LastLoadedProfilePath}\r\n", AnsiColors.Cyan);
                          }
                          else
                          {
                              GameTerminal.Append($"Last Profile Loaded Not Found: {App.Settings.AvalonSettings.LastLoadedProfilePath}\r\n", AnsiColors.Cyan);
                          }
                      }
                  });

            // TODO - Figure out a better way to inject a single Conveyor, maybe static in App?
            // Inject the Conveyor into the Triggers.
            foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
            {
                trigger.Conveyor = Conveyor;
            }

            // Wire up any events that have to be wired up through code.
            TextInput.Editor.PreviewKeyDown += this.Editor_PreviewKeyDown;
            AddHandler(TabControlEx.NetworkButtonClickEvent, new RoutedEventHandler(NetworkButton_Click));
            AddHandler(TabControlEx.SettingsButtonClickEvent, new RoutedEventHandler(SettingsButton_Click));

            // Pass the necessary reference from this page to the Interpreter.
            Interp = new Interpreter(this.Conveyor);

            // Setup the handler so when it wants to write to the main window it can by raising the echo event.
            Interp.Echo += this.InterpreterEcho;

            // Setup Lua
            Lua = new Script();
            Lua.Options.CheckThreadAccess = false;
            UserData.RegisterType<LuaCommands>();

            // create a userdata, again, explicitly.
            var luaCmd = UserData.Create(new LuaCommands(Interp));
            Lua.Globals.Set("Cmd", luaCmd);
            LuaControl = new ExecutionControlToken();

            // Setup the tick timer.
            TickTimer = new TickTimer(Conveyor);

            // Setup the auto complete commands.  If they're found refresh them, if they're not
            // report it to the terminal window.  It should -always be found-.
            var ac = this.Resources["AutoCompleteCommandProvider"] as AutoCompleteCommandProvider;

            if (ac != null)
            {
                ac.RefreshAutoCompleteEntries();
            }
            else
            {
                this.GameTerminal.Append("Warning: AutoCompleteCommandProvider was not found.", AnsiColors.Yellow);
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

                // Get the plugins
                var plugins = assembly.GetTypes().Where(x => pluginType.IsAssignableFrom(x));

                foreach (var plugin in plugins)
                {
                    var pluginInstance = (Plugin)Activator.CreateInstance(plugin);

                    // The IP address specified in the plugin matches the IP address we're connecting to.
                    if (pluginInstance.IpAddress.ToUpper() == App.Settings.ProfileSettings.IpAddress.ToUpper())
                    {
                        // Let it run it's own Initialize.
                        pluginInstance.Initialize();

                        // Go through all of the triggers and put them into our system triggers.
                        foreach (var trigger in pluginInstance.Triggers)
                        {
                            trigger.Plugin = true;
                            trigger.Conveyor = this.Conveyor;
                            App.SystemTriggers.Add(trigger);
                        }

                        this.Conveyor.EchoText($"Plugin File: {Argus.IO.FileSystemUtilities.ExtractFileName(file)}\r\n");
                        this.Conveyor.EchoText($"   => Plugins For: {pluginInstance.IpAddress}\r\n");
                        this.Conveyor.EchoText($"   => {pluginInstance.Triggers.Count()} Triggers Loaded\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// Initially sizes the window.
        /// </summary>
        private void WindowSize()
        {
            // The windows forms namespaces give us some properties to easily get screen
            // and window information so we're going to leverage those for now.
            this.Width = SystemParameters.WorkArea.Width / 2;
            this.Height = SystemParameters.WorkArea.Height;
            this.Left = 0;
            this.Top = 0;
            this.WindowState = WindowState.Normal;
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
        private async void ButtonExitAndSkipProfileSave_OnClickAsync(object sender, RoutedEventArgs e)
        {
            App.SkipSaveOnExit = true;
            Interp.EchoText("--> Exiting WITHOUT saving the current profile.", AnsiColors.Yellow);
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
            App.Settings.SaveSettings();
            Interp.EchoText("");
            Interp.EchoText($"--> Settings Saved\r\n", AnsiColors.Cyan);
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
                    App.Settings.LoadSettings(dialog.FileName);

                    // Inject the Conveyor into the Triggers.
                    foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
                    {
                        trigger.Conveyor = Conveyor;
                    }

                    // Show the user that the profile was successfully loaded.
                    Interp.EchoText("");
                    Interp.EchoText($"--> Loaded {dialog.FileName}.\r\n", AnsiColors.Cyan);

                    // Auto connect if it's setup to do so (this will disconnect from the previous server if it
                    // was connected.
                    if (App.Settings.ProfileSettings.AutoLogin)
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
        private void ButtonImportProfile_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO - Instead of a straight import everything have this do an upsert if there are matching guids so that packages can be updated.
            try
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = App.Settings.AvalonSettings.SaveDirectory;
                dialog.Filter = "JSON files (*.json)|*.json|Text Files (*.txt)|*.txt|All files (*.*)|*.*";

                if (dialog.ShowDialog() == true)
                {
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
                        trigger.Conveyor = Conveyor;
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

    }
}