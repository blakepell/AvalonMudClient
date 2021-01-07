using Argus.ComponentModel;
using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Avalon.Common.Attributes;
using System.Collections.Generic;

namespace Avalon.Common.Settings
{
    public class ProfileSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// The location from which these settings were loaded or last saved.  This will not get written out to
        /// file or surfaced on the UI, it's for internal use in knowing where to save the file back to.  This is
        /// just the filename.  The folder will be set either by the user's preferred save folder or the default
        /// save folder for the game.
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public string FileName { get; set; } = "default.json";

        [CategoryAttribute("Network")]
        [DescriptionAttribute("IP address of the game server.")]
        [Browsable(true)]
        public string IpAddress { get; set; } = "dsl-mud.org";

        [CategoryAttribute("Network")]
        [DescriptionAttribute("Port to connect to on the game server.")]
        [Browsable(true)]
        public int Port { get; set; } = 4000;

        [CategoryAttribute("Network")]
        [DescriptionAttribute("Whether the game should automatically connect to the last profile used.")]
        [Browsable(true)]
        public bool AutoConnect { get; set; } = true;

        [CategoryAttribute("Network")]
        [DescriptionAttribute("A command or set of commands that will execute when the network connection is first established.")]
        [Browsable(true)]
        public string OnConnect { get; set; } = "";

        [CategoryAttribute("Network")]
        [DescriptionAttribute("The number of milliseconds to wait after a connect attempt to send the OnConnect commands if they exist.  1 second = 1000 milliseconds.")]
        [Browsable(true)]
        public int OnConnectDelayMilliseconds { get; set; } = 1000;

        private bool _aliasesEnabled = true;

        [Browsable(false)]
        public bool AliasesEnabled
        {
            get => _aliasesEnabled;
            set
            {
                _aliasesEnabled = value;
                OnPropertyChanged(nameof(AliasesEnabled));
            }
        }

        private bool _triggersEnabled = true;

        [Browsable(false)]
        public bool TriggersEnabled
        {
            get => _triggersEnabled;
            set
            {
                _triggersEnabled = value;
                OnPropertyChanged(nameof(TriggersEnabled));
            }
        }

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The title that should be displayed client window for this profile.  A blank setting will default to 'Avalon Mud Client'.")]
        [Browsable(true)]
        public string WindowTitle { get; set; } = "Avalon Mud Client";


        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether the game should save the settings file when the client closes.")]
        [Browsable(true)]
        public bool SaveSettingsOnExit { get; set; } = true;

        [CategoryAttribute("TickTimer")]
        [DescriptionAttribute("Whether the game should sending a warning 5 seconds before a tick.")]
        [Browsable(true)]
        public bool EchoTickWarning { get; set; } = false;

        [CategoryAttribute("TickTimer")]
        [DescriptionAttribute("Commands to execute on tick.")]
        [Browsable(true)]
        public string ExecuteCommandsOnTick { get; set; } = "";

        [CategoryAttribute("TickTimer")]
        [DescriptionAttribute("If the send command on tick is enabled.")]
        [Browsable(true)]
        public bool EnableCommandsOnTick { get; set; } = false;

        [CategoryAttribute("TickTimer")]
        [DescriptionAttribute("The duration of a tick in seconds.  The default is 40.")]
        [Browsable(true)]
        public int TickDurationInSeconds { get; set; } = 40;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether the game will insert a command if the user enters a command more than 15 times.")]
        [Browsable(true)]
        public bool SpamGuard { get; set; } = true;

        [CategoryAttribute("Performance")]
        [DescriptionAttribute("Whether the game tracks the last date each trigger was fired.")]
        [Browsable(true)]
        public bool TrackTriggerLastMatched { get; set; } = true;

        [CategoryAttribute("Audio")]
        [DescriptionAttribute("Whether the mud client should make a beep when the ANSI beep code is sent from the mud.")]
        [Browsable(true)]
        public bool AnsiBeep { get; set; } = false;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("A command or set of commands that will run as the last step in the mud client startup process.")]
        [Browsable(true)]
        public string AutoExecuteCommand { get; set; } = "";

        private bool _spellChecking = false;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether spellchecking is enabled in the input box.")]
        [Browsable(true)]
        public bool SpellChecking
        {
            get => _spellChecking;
            set
            {
                _spellChecking = value;
                OnPropertyChanged(nameof(SpellChecking));
            }
        }

        private string _searchBarCommand = "";

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The command the search bar should execute.")]
        [Browsable(true)]
        public string SearchBarCommand
        {
            get => _searchBarCommand;
            set
            {
                _searchBarCommand = value;
                OnPropertyChanged(nameof(SearchBarCommand));
            }
        }

        /// <summary>
        /// The location to the SQLite database for this profile.  Note that if the file is changed it will try
        /// to create a new database in the location specified.
        /// </summary>
        [JsonIgnore]
        [CategoryAttribute("Database")]
        [DescriptionAttribute("Location to the SQLite database for this profile.")]
        [Browsable(true)]
        public string SqliteDatabase { get; set; } = "";

        /// <summary>
        /// A global Lua script that can be used to share functions and code with all Lua from the UI.
        /// </summary>
        [Lua]
        [CategoryAttribute("Lua")]
        [DescriptionAttribute("A global Lua script that can be used to share functions and code with all Lua from the UI.")]
        [Browsable(true)]
        public string LuaGlobalScript { get; set; } = "";

        /// <summary>
        /// This is provided in the profile even if a client doesn't implement it.  The SSH program shelled might be specific
        /// to a profile if a user has multiple profiles.
        /// </summary>
        [CategoryAttribute("NavigationBar")]
        [DescriptionAttribute("The path to shell an SSH program that is associated with this profile.  If this value is blank the option won't be shown on the Nav Bar.")]
        [Browsable(true)]
        public string SshAppPath { get; set; } = "";

        /// <summary>
        /// This is provided in the profile even if a client doesn't implement it.  The terminal program (not SSH) that should be
        /// shelled for use on the local system.
        /// </summary>
        [CategoryAttribute("NavigationBar")]
        [DescriptionAttribute("The path or execute name of a terminal that can be shelled from the navigation bar.  E.g. cmd, bash, wt.  If this value is blank the option won't be shown on the Nav Bar.")]
        [Browsable(true)]
        public string TerminalAppPath { get; set; } = "";

        /// <summary>
        /// A list of any installed package ID's.  The package ID a trigger, alias, etc. belongs to will be stored there
        /// as well but this will give us a place to track installed packages for things like behaviors that don't have
        /// aliases/triggers/directions etc. but might change something about the UI.
        /// </summary>
        [Browsable(false)]
        public List<InstalledPackage> InstalledPackages { get; set; } = new List<InstalledPackage>();

        [Browsable(false)]
        public ObservableCollection<Macro> MacroList { get; set; } = new ObservableCollection<Macro>();

        [Browsable(false)]
        public ObservableCollection<Alias> AliasList { get; set; } = new ObservableCollection<Alias>();

        [Browsable(false)]
        public SpecialObservableCollection<Direction> DirectionList { get; set; } = new SpecialObservableCollection<Direction>();

        [Browsable(false)]
        public SpecialObservableCollection<Trigger> TriggerList { get; set; } = new SpecialObservableCollection<Trigger>();

        [Browsable(false)]
        public SpecialObservableCollection<Variable> Variables { get; set; } = new SpecialObservableCollection<Variable>();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
