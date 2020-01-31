using System.Collections.Generic;
using System.Windows;
using Avalon.Common.Settings;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Utilities;

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
        /// Runs as the first thing in the programs pipeline.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // First thing's first, setup the Settings.  This will at least initialize the client
            // settings (and if a profile has previously been loaded it will load that profile).
            App.Settings = new SettingsProvider();
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
