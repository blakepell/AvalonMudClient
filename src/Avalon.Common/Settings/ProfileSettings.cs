using Argus.ComponentModel;
using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

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

        [CategoryAttribute("Login")]
        [DescriptionAttribute("Whether the client should try to auto login with triggers.")]
        [Browsable(true)]
        public bool AutoLogin { get; set; } = false;

        [CategoryAttribute("Login")]
        [DescriptionAttribute("The username that auto login will use.")]
        [Browsable(true)]
        public string Username { get; set; } = "";

        [CategoryAttribute("Login")]
        [DescriptionAttribute("The password that auto login will use.")]
        [Browsable(true)]
        public string Password { get; set; } = "";

        [CategoryAttribute("Login")]
        [DescriptionAttribute("Whether the client should try to skip the MOTD.")]
        [Browsable(true)]
        public bool SkipMotd { get; set; } = false;

        private bool _aliasesEnabled = true;

        [Browsable(false)]
        public bool AliasesEnabled
        {
            get
            {
                return _aliasesEnabled;
            }
            set
            {
                _aliasesEnabled = value;
                OnPropertyChanged("AliasesEnabled");
            }
        }

        private bool _triggersEnabled = true;

        [Browsable(false)]
        public bool TriggersEnabled
        {
            get
            {
                return _triggersEnabled;
            }
            set
            {
                _triggersEnabled = value;
                OnPropertyChanged("TriggersEnabled");
            }
        }

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether the game should save the settings file when the client closes.")]
        [Browsable(true)]
        public bool SaveSettingsOnExit { get; set; } = true;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether the game should sending a warning 5 seconds before a tick.")]
        [Browsable(true)]
        public bool EchoTickWarning { get; set; } = false;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether the game will insert a command if the user enters a command more than 15 times.")]
        [Browsable(true)]
        public bool SpamGuard { get; set; } = true;


        [CategoryAttribute("Performance")]
        [DescriptionAttribute("Whether the game tracks the last date each trigger was fired.")]
        [Browsable(true)]
        public bool TrackTriggerLastMatched { get; set; } = true;


        [CategoryAttribute("Misc")]
        [DescriptionAttribute("A command or set of commands that will run as the last step in the mud client startup process.")]
        [Browsable(true)]
        public string AutoExecuteCommand { get; set; } = "";

        [Browsable(false)]
        public bool Debug { get; set; } = false;

        private bool _spellChecking = false;

        [CategoryAttribute("Misc")]
        [DescriptionAttribute("Whether spellchecking is enabled in the input box.")]
        [Browsable(true)]
        public bool SpellChecking
        {
            get
            {
                return _spellChecking;
            }
            set
            {
                _spellChecking = value;
                OnPropertyChanged("SpellChecking");
            }
        }

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
