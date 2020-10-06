using Avalon.Common.Models;
using Avalon.Common.Settings;
using System.Collections.Generic;
using Trigger = Avalon.Common.Triggers.Trigger;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Interface for a settings provider.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// The settings file that is the current loaded profile.
        /// </summary>
        ProfileSettings ProfileSettings { get; set; }

        /// <summary>
        /// The application data directory that is in the standard shared location for Windows.
        /// </summary>
        string AppDataDirectory { get; }
        
        /// <summary>
        /// The directory that the client should look in to load plugins.
        /// </summary>
        string PluginDirectory { get; }

        /// <summary>
        /// A staging directory where updates for plugins are downloaded to.
        /// </summary>
        string UpdateDirectory { get; }

        /// <summary>
        /// The core settings file that is not profile settings related.
        /// </summary>
        AvalonSettings AvalonSettings { get; set; } 

        /// <summary>
        /// This is the settings file in the AppDataDirectory folder.  It will hold a small settings
        /// file that have data like where the main save folder is if it's been changed.  Basically this
        /// loads and then finds out where to get the actual profiles from (if it's saved in a DropBox,
        /// OneDrive, etc. type of folder).
        /// </summary>
        string AvalonSettingsFile { get; }

        /// <summary>
        /// An implementation of the Conveyor so some settings can be discerned from the UI (such as last
        /// window position, etc.).
        /// </summary>
        IConveyor Conveyor { get; set; }

        /// <summary>
        /// Creates the default application data folder.  This is where the core settings file
        /// will be held and any temp files will be stored.  The profiles folder will also default
        /// here until it's changed by the user to be somewhere else.
        /// </summary>
        void InitializeApplicationDataDirectory();

        /// <summary>
        /// Loads the settings from the specified json file.
        /// </summary>
        /// <param name="settingsFile"></param>
        void LoadSettings(string settingsFile);

        /// <summary>
        /// Saves our settings to the local storage.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Imports a package of aliases, triggers and directions from a JSON string.
        /// </summary>
        /// <param name="json"></param>
        void ImportPackageFromJson(string json);

        /// <summary>
        /// Imports a list of triggers.
        /// </summary>
        /// <param name="list"></param>
        void ImportTriggers(IList<Trigger> list);

        /// <summary>
        /// Imports a single trigger.
        /// </summary>
        /// <param name="trigger"></param>
        void ImportTrigger(Trigger trigger);

        /// <summary>
        /// Imports a list of directions.
        /// </summary>
        /// <param name="list"></param>
        void ImportDirections(IList<Direction> list);

        /// <summary>
        /// Imports a single direction.
        /// </summary>
        /// <param name="direction"></param>
        void ImportDirection(Direction direction);

        /// <summary>
        /// Imports a list of aliases.
        /// </summary>
        /// <param name="list"></param>
        void ImportAliases(IList<Alias> list);

        /// <summary>
        /// Imports a single alias.
        /// </summary>
        /// <param name="alias"></param>
        void ImportAlias(Alias alias);
    }
}
