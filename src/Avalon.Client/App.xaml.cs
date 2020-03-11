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

namespace Avalon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
        /// Ability to send a Windows OS toast message.
        /// </summary>
        public static Toast Toast { get; set; } = new Toast();

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
                App.Settings.SaveSettings();
            }

            // Dispose of the Toast object which has a NotifyIcon which might potentially leave the
            // program in memory if not axed.
            Toast?.Dispose();
        }

    }
}
