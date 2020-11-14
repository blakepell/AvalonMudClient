using Argus.ComponentModel;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Argus.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trigger = Avalon.Common.Triggers.Trigger;

namespace Avalon.Common.Settings
{
    /// <summary>
    /// The default implementation for settings.
    /// </summary>
    public class SettingsProvider : ISettingsProvider
    {
        /// <summary>
        /// The settings file that is the current loaded profile.
        /// </summary>
        public ProfileSettings ProfileSettings { get; set; } = new ProfileSettings();

        /// <summary>
        /// The core settings file that is not profile settings related.
        /// </summary>
        public AvalonSettings AvalonSettings { get; set; } = new AvalonSettings();

        /// <summary>
        /// The application data directory that is in the standard shared location for Windows.
        /// </summary>
        public string AppDataDirectory { get; private set; }

        /// <summary>
        /// The folder that plugins are stored in.
        /// </summary>
        public string PluginDirectory { get; private set; }

        /// <summary>
        /// A staging directory where updates for plugins are downloaded to.
        /// </summary>
        public string UpdateDirectory { get; private set; }

        /// <summary>
        /// This is the settings file in the AppDataDirectory folder.  It will hold a small settings
        /// file that have data like where the main save folder is if it's been changed.  Basically this
        /// loads and then finds out where to get the actual profiles from (if it's saved in a DropBox,
        /// OneDrive, etc. type of folder).
        /// </summary>
        public string AvalonSettingsFile { get; private set; }

        /// <summary>
        /// The implementation of the Conveyor so that the settings can interact with the UI.
        /// </summary>
        public IConveyor Conveyor { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsProvider(IConveyor ic)
        {
            this.Conveyor = ic;
            this.AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvalonMudClient");
            this.PluginDirectory = Path.Combine(this.AppDataDirectory, "Plugins");
            this.UpdateDirectory = Path.Combine(this.AppDataDirectory, "Updates");
            this.AvalonSettingsFile = Path.Combine(this.AppDataDirectory, "avalon.json");
            this.InitializeApplicationDataDirectory();
        }

        /// <summary>
        /// Creates the default application data folder.  This is where the core settings file
        /// will be held and any temp files will be stored.  The profiles folder will also default
        /// here until it's changed by the user to be somewhere else.
        /// </summary>
        public void InitializeApplicationDataDirectory()
        {
            // Create the core app data folder if it doesn't exist.
            if (!Directory.Exists(this.AppDataDirectory))
            {
                Argus.IO.FileSystemUtilities.CreateDirectory(this.AppDataDirectory);
            }

            // Create the plugin folder if it doesn't exist.
            if (!Directory.Exists(this.PluginDirectory))
            {
                Argus.IO.FileSystemUtilities.CreateDirectory(this.PluginDirectory);
            }

            // Create the updates folder if it doesn't exist.
            if (!Directory.Exists(this.UpdateDirectory))
            {
                Argus.IO.FileSystemUtilities.CreateDirectory(this.UpdateDirectory);
            }
            
            // Create the core (minimal) settings file if it doesn't exist.
            if (!File.Exists(this.AvalonSettingsFile))
            {
                // The default save folder is the AppDataDirectory, this will be configurable.
                this.AvalonSettings.SaveDirectory = this.AppDataDirectory;
                File.WriteAllText(this.AvalonSettingsFile, JsonConvert.SerializeObject(this.AvalonSettings, Formatting.Indented));
            }

            // Now, read in the file.
            if (File.Exists(this.AvalonSettingsFile))
            {
                try
                {
                    string json = File.ReadAllText(this.AvalonSettingsFile);
                    this.AvalonSettings = JsonConvert.DeserializeObject<AvalonSettings>(json);
                    
                    // If the save directory is empty or doesn't exist then use the app data directory.
                    if (string.IsNullOrWhiteSpace(this.AvalonSettings.SaveDirectory) || !Directory.Exists((this.AvalonSettings.SaveDirectory)))
                    {
                        this.AvalonSettings.SaveDirectory = this.AppDataDirectory;
                    }

                }
                catch (JsonSerializationException)
                {
                    // Reset the file
                    File.WriteAllText(this.AvalonSettingsFile, JsonConvert.SerializeObject(this.AvalonSettings, Formatting.Indented));
                }
                catch
                {
                    // TODO - Error handling
                    // Something else went wrong so ya know.. guess we'll hang with the default settings like it's a brand new install.
                }
            }

        }

        /// <summary>
        /// Loads the settings from the specified json file.
        /// </summary>
        /// <param name="settingsFile"></param>
        public void LoadSettings(string settingsFile)
        {
            // If the storage folder hasn't been set, default to the save location.
            if (!File.Exists(settingsFile))
            {
                // Get the name of the profile they wanted
                this.ProfileSettings = new ProfileSettings
                {
                    FileName = Path.GetFileName(settingsFile)
                };

                // Use it to make this profile.
                this.AvalonSettings.LastLoadedProfilePath = Path.Combine(this.AvalonSettings.SaveDirectory, this.ProfileSettings.FileName);
            }
            else
            {
                // File existed, let's try to read it in.
                try
                {
                    // Load the file, then set it as the last loaded file -if- it existed.
                    string json = File.ReadAllText(settingsFile);
                    this.ProfileSettings = JsonConvert.DeserializeObject<ProfileSettings>(json);
                    this.ProfileSettings.FileName = Path.GetFileName(settingsFile);
                    this.AvalonSettings.LastLoadedProfilePath = settingsFile;
                }
                catch
                {
                    // No file or an error, default back to the safe storage location.
                    this.ProfileSettings = new ProfileSettings();

                    // Get the name of the profile they wanted
                    this.ProfileSettings = new ProfileSettings
                    {
                        FileName = Path.GetFileName(settingsFile)
                    };

                    // Use it to make this profile.
                    this.AvalonSettings.LastLoadedProfilePath = Path.Combine(this.AvalonSettings.SaveDirectory, this.ProfileSettings.FileName);
                }
            }

            // Set the database that corresponds to this profile (we will not save this out, it will be constructed and be saved in
            // the same folder as the profile.
            if (string.IsNullOrWhiteSpace(this.ProfileSettings.SqliteDatabase))
            {
                this.ProfileSettings.SqliteDatabase = Path.Combine(this.AvalonSettings.SaveDirectory, $"{this.ProfileSettings.FileName.Replace(".json", "")}.db");
            }

            // There are no macros set, initialize our default ones.
            if (this.ProfileSettings.MacroList.Count == 0)
            {
                this.ProfileSettings.MacroList.Add(new Macro(74, "NumPad0", "d"));   // Key.NumPad0
                this.ProfileSettings.MacroList.Add(new Macro(75, "NumPad1", "sw"));  // Key.NumPad1
                this.ProfileSettings.MacroList.Add(new Macro(76, "NumPad2", "s"));   // Key.NumPad2
                this.ProfileSettings.MacroList.Add(new Macro(77, "NumPad3", "se"));  // Key.NumPad3
                this.ProfileSettings.MacroList.Add(new Macro(78, "NumPad4", "w"));   // Key.NumPad4
                this.ProfileSettings.MacroList.Add(new Macro(79, "NumPad5", "u"));   // Key.NumPad5
                this.ProfileSettings.MacroList.Add(new Macro(80, "NumPad6", "e"));   // Key.NumPad6
                this.ProfileSettings.MacroList.Add(new Macro(81, "NumPad7", "nw"));  // Key.NumPad7
                this.ProfileSettings.MacroList.Add(new Macro(82, "NumPad8", "n"));   // Key.NumPad8
                this.ProfileSettings.MacroList.Add(new Macro(83, "NumPad9", "ne"));  // Key.NumPad9
                this.ProfileSettings.MacroList.Add(new Macro(90, "F1", ""));         // Key.F1
                this.ProfileSettings.MacroList.Add(new Macro(91, "F2",""));          // Key.F2
                this.ProfileSettings.MacroList.Add(new Macro(92, "F3", ""));         // Key.F3
                this.ProfileSettings.MacroList.Add(new Macro(93, "F4", ""));         // Key.F4
                this.ProfileSettings.MacroList.Add(new Macro(94, "F5", ""));         // Key.F5
                this.ProfileSettings.MacroList.Add(new Macro(95, "F6", ""));         // Key.F6
                this.ProfileSettings.MacroList.Add(new Macro(96, "F7", ""));         // Key.F7
                this.ProfileSettings.MacroList.Add(new Macro(97, "F8", ""));         // Key.F8
                this.ProfileSettings.MacroList.Add(new Macro(98, "F9", ""));         // Key.F9
                this.ProfileSettings.MacroList.Add(new Macro(99, "F10", ""));        // Key.F10
                this.ProfileSettings.MacroList.Add(new Macro(100, "F11", ""));       // Key.F11
                this.ProfileSettings.MacroList.Add(new Macro(101, "F12", ""));       // Key.F12
            }

        }

        /// <summary>
        /// Saves our settings to the local storage.
        /// </summary>
        public void SaveSettings()
        {
            // Don't save the settings if they're null
            if (this.ProfileSettings == null)
            {
                return;
            }

            // If there is no last loaded profile set it then save it.
            if (string.IsNullOrWhiteSpace(this.AvalonSettings.LastLoadedProfilePath))
            {
                this.AvalonSettings.LastLoadedProfilePath = Path.Join(this.AvalonSettings.SaveDirectory, "default.json");
            }

            this.AvalonSettings.LastWindowPosition = this.Conveyor.GetWindowPosition;

            // Write the profile settings file.
            File.WriteAllText(this.AvalonSettings.LastLoadedProfilePath, JsonConvert.SerializeObject(this.ProfileSettings, Formatting.Indented));

            // Write the Avalon settings file.
            File.WriteAllText(this.AvalonSettingsFile, JsonConvert.SerializeObject(this.AvalonSettings, Formatting.Indented));
        }

        /// <summary>
        /// Imports a package of aliases, triggers and directions into the current profile.  Values that already
        /// exist are updated preserving various information such as count..
        /// </summary>
        /// <param name="json"></param>
        public void ImportPackageFromJson(string json)
        {
            // Deserialize the JSON file that was provided to us.
            var settings = JsonConvert.DeserializeObject<ProfileSettings>(json);

            // Imports the aliases, triggers and directions that are applicable using the shared import methods.
            this.ImportAliases(settings.AliasList);
            this.ImportTriggers(settings.TriggerList);
            this.ImportDirections(settings.DirectionList);
        }

        /// <summary>
        /// Imports a <see cref="Package"/> object.
        /// </summary>
        /// <param name="package"></param>
        public void ImportPackage(Package package)
        {
            this.ProfileSettings.InstalledPackages.AddIfDoesntExist(package.Id);

            this.ImportAliases(package.AliasList);
            this.ImportTriggers(package.TriggerList);
            this.ImportDirections(package.DirectionList);

            if (!string.IsNullOrWhiteSpace(package.SetupCommand))
            {
                this.Conveyor.ExecuteCommand(package.SetupCommand);
            }

            if (!string.IsNullOrWhiteSpace(package.SetupLuaScript))
            {
                this.Conveyor.ExecuteLuaAsync(package.SetupLuaScript);
            }
        }

        /// <inheritdoc />
        public void ImportTriggers(IList<Trigger> list)
        {
            foreach (var trigger in list)
            {
                ImportTrigger(trigger);
            }
        }

        /// <inheritdoc />
        public void ImportTrigger(Trigger trigger)
        {
            // Skip any locked items that exist AND are locked.
            if (this.ProfileSettings.TriggerList.Any(profileTrigger => profileTrigger.Identifier.Equals(trigger.Identifier, StringComparison.OrdinalIgnoreCase) && profileTrigger.Lock))
            {
                return;
            }

            int count = 0;

            // Go through all of the triggers and see if this one already exists.
            for (int i = this.ProfileSettings.TriggerList.Count - 1; i >= 0; i--)
            {
                if (this.ProfileSettings.TriggerList[i].Identifier == trigger.Identifier)
                {
                    // Save the count we're going to preserve it.
                    count = this.ProfileSettings.TriggerList[i].Count;

                    // Remove the trigger, we're going to re-add the new copy.
                    this.ProfileSettings.TriggerList.RemoveAt(i);

                    break;
                }
            }

            trigger.Count = count;

            // Inject the Conveyor into all of the triggers so they're ready to roll.
            trigger.Conveyor = this.Conveyor;

            this.ProfileSettings.TriggerList.Add(trigger);
        }

        /// <inheritdoc />
        public void ImportDirections(IList<Direction> list)
        {
            // A direction must be unique by it's name and starting room.
            foreach (var direction in list)
            {
                ImportDirection(direction);
            }
        }

        /// <inheritdoc />
        public void ImportDirection(Direction direction)
        {
            // Skip any locked items that exist AND are locked.
            if (this.ProfileSettings.DirectionList.Any(profileDirection => profileDirection.Name.Equals(direction.Name, StringComparison.OrdinalIgnoreCase)
                                                                            && profileDirection.StartingRoom.Equals(direction.StartingRoom, StringComparison.OrdinalIgnoreCase)
                                                                            && profileDirection.Lock))
            {
                return;
            }

            // Go through all of the directions and see if this one already exists.
            for (int i = this.ProfileSettings.DirectionList.Count - 1; i >= 0; i--)
            {
                if (string.Equals(this.ProfileSettings.DirectionList[i].Name, direction.Name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(this.ProfileSettings.DirectionList[i].StartingRoom, direction.StartingRoom, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the trigger, we're going to re-add the new copy.
                    this.ProfileSettings.DirectionList.RemoveAt(i);
                }
            }

            this.ProfileSettings.DirectionList.Add(direction);
        }

        /// <inheritdoc />
        public void ImportAliases(IList<Alias> list)
        {
            // An alias must be unique by it's expression.
            foreach (var alias in list)
            {
                ImportAlias(alias);
            }
        }

        /// <inheritdoc />
        public void ImportAlias(Alias alias)
        {
            // Skip any locked items that exist AND are locked.
            if (this.ProfileSettings.AliasList.Any(profileAlias => profileAlias.AliasExpression.Equals(alias.AliasExpression, StringComparison.OrdinalIgnoreCase) && profileAlias.Lock))
            {
                return;
            }

            int count = 0;

            // Go through all of the aliases and see if this one already exists.
            for (int i = this.ProfileSettings.AliasList.Count - 1; i >= 0; i--)
            {
                if (this.ProfileSettings.AliasList[i].AliasExpression.Equals(alias.AliasExpression, StringComparison.OrdinalIgnoreCase))
                {
                    // Save the count we're going to preserve it.
                    count = this.ProfileSettings.AliasList[i].Count;

                    // Remove the alias, we're going to re-add the new copy.
                    this.ProfileSettings.AliasList.RemoveAt(i);

                    break;
                }
            }

            alias.Count = count;
            this.ProfileSettings.AliasList.Add(alias);
        }
    }
}
