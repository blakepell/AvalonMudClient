using System.Collections.Generic;
using System.Windows;
using Avalon.Common.Settings;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Utilities;
using System.Media;
using System.IO;
using System.ComponentModel;
using Avalon.Windows;
using System.Drawing.Design;
using Avalon.Common.Plugins;
using System;
using System.Threading.Tasks;
using Argus.Extensions;
using System.Text;
using System.Linq;
using ICSharpCode.AvalonEdit;
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
        /// These are immutable triggers that are set by the mud client that will be isolated from
        /// the user defined triggers.  (TODO: Factor into abstract layer)
        /// </summary>
        internal static List<ITrigger> SystemTriggers { get; set; } = new List<ITrigger>();

        /// <summary>
        /// A list of plugins that were loaded via reflection on startup.
        /// </summary>
        internal static List<Plugin> Plugins { get; set; } = new List<Plugin>();

        /// <summary>
        /// Ability to send a Windows OS toast message.
        /// </summary>
        public static Toast Toast { get; set; } = new Toast();

        /// <summary>
        /// Global variables that are specific to this instance and not persisted across boots of
        /// the mud client.
        /// </summary>
        internal static InstanceGlobals InstanceGlobals { get; set; } = new InstanceGlobals();

        /// <summary>
        /// An override to force skipping a save on exit in case someone borked their settings up.
        /// </summary>
        internal static bool SkipSaveOnExit { get; set; } = false;

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
                // Setup the Conveyor for this instance of the mud client.  This can be passed to
                // the business logic layer via because it inherits the IConveyor interface.
                App.Conveyor = new Conveyor();

                // First thing's first, setup the Settings.  This will at least initialize the client
                // settings (and if a profile has previously been loaded it will load that profile).
                App.Settings = new SettingsProvider(App.Conveyor);

                // We're going to try to load the wav file to play the ANSI beep when it's needed.
                if (File.Exists(@"Media\alert.wav"))
                {
                    App.Beep = new SoundPlayer(@"Media\alert.wav");
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

            }
            catch (Exception ex)
            {
                // TODO - logging
                MessageBox.Show($"A startup error occurred: {ex.Message}");
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
            if (Settings.ProfileSettings.SaveSettingsOnExit && SkipSaveOnExit == false)
            {
                // For saving of the grid layout into the settings object.
                App.MainWindow.SaveGridState();

                // Actually save the settings.
                App.Settings.SaveSettings();
            }

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
