/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

global using Argus.Extensions;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
global using System.ComponentModel;
global using System.Collections;
global using System.Reflection;
global using Newtonsoft.Json;
using Avalon.Common;
using Avalon.Common.Interfaces;
using Avalon.Common.Plugins;
using Avalon.Common.Scripting;
using Avalon.Common.Settings;
using Avalon.Utilities;
using Avalon.Windows;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Design;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// A reference to the GameWindow
        /// </summary>
        internal static MainWindow MainWindow { get; set; }

        /// <summary>
        /// A reference to the main Conveyor which is the platform specific implementation of how the
        /// abstracted libraries can interact with the UI.
        /// </summary>
        internal static Conveyor Conveyor { get; set; }

        /// <summary>
        /// A reference to the Settings for the profile that is currently loaded.
        /// </summary>
        internal static ISettingsProvider Settings { get; set; }

        /// <summary>
        /// A list of plugins that were loaded via reflection on startup.
        /// </summary>
        internal static List<Plugin> Plugins { get; set; } = new();

        /// <summary>
        /// Ability to send a Windows OS toast message.
        /// </summary>
        public static Toast Toast { get; set; } = new();

        /// <summary>
        /// Global variables that are specific to this instance and not persisted across boots of
        /// the mud client.
        /// </summary>
        internal static InstanceGlobals InstanceGlobals { get; set; } = new();

        /// <summary>
        /// The SoundPlayer used to play the ANSI beep.
        /// </summary>
        internal static SoundPlayer Beep;

        /// <summary>
        /// Runs as the first thing in the programs pipeline.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var splashScreen = new SplashScreen("Media/SplashScreen.png");
                splashScreen.Show(true);
            }
            catch
            {
                // Eat error
            }

            try
            {
                // Done to support the Interface, we're going to go ahead and register any
                // singleton instances that we can create here.  Tier 1 services which can
                // be loaded without dependency.
                var conveyor = new Conveyor();
                var scriptHost = new ScriptHost();
                var settings = new SettingsProvider(conveyor);
                var mainWindow = new MainWindow();

                AppServices.Init((sc) =>
                {
                    sc.AddSingleton<Conveyor>(conveyor);
                    sc.AddSingleton<IConveyor>(conveyor);
                    sc.AddSingleton<ScriptHost>(scriptHost);
                    sc.AddSingleton<SettingsProvider>(settings);
                    sc.AddSingleton<ISettingsProvider>(settings);
                    sc.AddSingleton<MainWindow>(mainWindow);
                });

                // Setup the Conveyor for this instance of the mud client.  This can be passed to
                // the business logic layer via because it inherits the IConveyor interface.
                App.Conveyor = AppServices.GetService<Conveyor>();

                // First thing's first, setup the Settings.  This will at least initialize the client
                // settings (and if a profile has previously been loaded it will load that profile).
                App.Settings = AppServices.GetService<SettingsProvider>();

                // Set are reference in the App (backwards compatibility) and also set the current
                // main window for the Application since we're manually showing the window at the
                // end of this procedure.
                App.MainWindow = AppServices.GetService<MainWindow>();
                Application.Current.MainWindow = App.MainWindow;

                string alertFile = Utilities.Utilities.IsRunningAsUwp() ? "ms-appx:///Media/alert.wav" : @"Media\alert.wav";

                // We're going to try to load the wav file to play the ANSI beep when it's needed.
                if (File.Exists(alertFile))
                {
                    App.Beep = new SoundPlayer(alertFile);
                    App.Beep.Load();
                }

                // Adds the string editor to all strings.. but based on convention (or attribute) we'll 
                // determine which string editor opens.
                TypeDescriptor.AddAttributes(typeof(string), new EditorAttribute(typeof(StringPropertyEditor), typeof(UITypeEditor)));

                // Setup our global exception handling if the setting is set for it.
                if (App.Settings.AvalonSettings.GlobalExceptionHandlingEnabled)
                {
                    SetupExceptionHandling();
                }

                // This is not recommended but is provided in case the hardware acceleration is causing the app to crash
                // or not render correctly due to a video card driver.  This is an edge case but providing this setting
                // will allow for people to disable hardware acceleration and continue using the app.
                if (App.Settings.AvalonSettings.DisableHardwareAcceleration)
                {
                    RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
                }

                // Remove "Control+D" from the AvalonEdit input gestures so our "Control+D" hot key for directions
                // from the current room will work.
                AvalonEditCommands.DeleteLine.InputGestures.Clear();

                // Tier 2 services, might require something from tier 1 so loaded second.  The interpreter echos to
                // the main window, requires events wired up on the main window, requires the script host and needs
                // the settings so we'll add it in after the fact here.
                var interp = new Interpreter();

                AppServices.AddService((sc) =>
                {
                    sc.AddSingleton<Interpreter>(interp);
                    sc.AddSingleton<IInterpreter>(interp);
                });
            }
            catch (Exception ex)
            {
                // TODO - logging
                MessageBox.Show($"A startup error occurred: {ex.Message}");
            }

            // Showing the main window needs to be the last thing since it will require the settings to be
            // in place that are used in the MainWindow_Loaded event.
            try
            {
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A startup error occurred showing MainWindow: {ex.Message}");
                Environment.Exit(8);
            }
        }

        /// <summary>
        /// Tasks to perform when the application exits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save the users current profile on exit if the setting is set to do so, but also if the override
            // to skip it hasn't been flagged.
            if (Settings.ProfileSettings.SaveSettingsOnExit && !InstanceGlobals.SkipSaveOnExit)
            {
                // For saving of the grid layout into the settings object.
                App.MainWindow.SaveGridState();

                // Saves the last Lua script that the use had in the interactive editor.
                App.Settings.ProfileSettings.LastInteractiveLuaScript = App.MainWindow.LuaEditor.Editor.Text;

                // Actually save the settings.
                App.Settings.SaveSettings();
            }

            // Write any pending database transactions to the db, then dispose of the SqlTasks
            // object which will properly close the DB connection.  We're going to eat an error
            // here since the program is ending anyway.  We can log it in the future if need be.
            try
            {
                App.MainWindow?.SqlTasks?.Flush();
                App.MainWindow?.SqlTasks?.Dispose();
            }
            catch { }

            // Dispose of the Toast object which has a NotifyIcon which might potentially leave the
            // program in memory if not axed.
            Toast?.Dispose();

            // Try to gracefully close down any open windows that are being tracked in the
            // Conveyor.  In theory, these should clean themselves up because the main window
            // closing should trigger them to close (and when they close they should be removing
            // themselves from this list.
            if (Conveyor != null)
            {
                foreach (var item in Conveyor.WindowList.Where(x => x != null))
                {
                    item.Close();
                }
            }
        }

        /// <summary>
        /// Sets up global exception handling.
        /// </summary>
        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        /// <summary>
        /// Global exception handler.  Can be disabled via setting.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="source"></param>
        private void LogUnhandledException(Exception exception, string source)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Unhandled exception ({source})");
            string logFile = Path.Join(App.Settings.AppDataDirectory, "CrashLog.txt");

            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                sb.AppendFormat("Unhandled exception in {0} v{1}\r\n", assemblyName.Name, assemblyName.Version);
                sb.AppendLine(exception.ToFormattedString());
                File.WriteAllText(logFile, sb.ToString(), Encoding.UTF8);
            }
            catch { }

            Environment.Exit(8);
        }

    }
}
