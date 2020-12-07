using Argus.Extensions;
using Avalon.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalon.Common.Models;

namespace Avalon.Common.Triggers
{
    /// <summary>
    /// A trigger is an action that is executed based off of a pattern that is sent from the game.
    /// </summary>
    public class Trigger : ITrigger, ICloneable, INotifyPropertyChanged
    {
        public Trigger()
        {
        }

        public Trigger(string pattern, string command, string character = "", bool isSilent = false, string identifier = "",
                        TerminalTarget moveTo = TerminalTarget.None, bool gag = false, string group = "", bool disableAfterTriggered = false,
                        bool enabled = true, bool highlightLine = false, bool isLua = false, bool variableReplacement = false, bool systemTrigger = false,
                        int priority = 10000, bool stopProcessing = false)
        {
            this.Pattern = pattern;
            this.Command = command;
            this.Character = character;
            this.IsSilent = isSilent;
            this.Identifier = identifier;
            this.MoveTo = moveTo;
            this.Gag = gag;
            this.Group = group;
            this.DisableAfterTriggered = disableAfterTriggered;
            this.Enabled = enabled;
            this.HighlightLine = highlightLine;
            this.IsLua = isLua;
            this.VariableReplacement = variableReplacement;
            this.SystemTrigger = systemTrigger;
            this.Priority = priority;
            this.StopProcessing = stopProcessing;
        }

        public Trigger(string pattern, string command, string character, bool isSilent, string identifier)
        {
            this.Pattern = pattern;
            this.Command = command;
            this.Character = character;
            this.Identifier = identifier;
            this.IsSilent = isSilent;
        }

        public Trigger(string pattern, string command, string character, bool isSilent, string identifier, TerminalTarget moveTo, bool gag)
        {
            this.Pattern = pattern;
            this.Command = command;
            this.Character = character;
            this.Identifier = identifier;
            this.IsSilent = isSilent;
            this.MoveTo = moveTo;
            this.Gag = gag;
        }

        /// <inheritdoc/>
        public bool IsMatch(string line, bool skipVariableSet = false)
        {
            Match match;

            // Does this trigger contain any variables?  If so, we'll need to special handle it.  We're also
            // going to require that the VariableReplacement value is set to true so the player has to
            // specifically opt into this.  Since the Gag triggers run -a lot- on the terminal rendering
            // the bool will much faster as a first check before the string contains check.  This is
            // a micro optimization that had real payoff in the performance profiler.  Also when profiling, IndexOf
            // a char without Ordinal consistently ran faster than Contains and IndexOf with Ordinal.
            if (this.VariableReplacement && Pattern.IndexOf('@') >= 0)
            {
                // Replace any variables with their literal values.
                string tempPattern = this.Conveyor.ReplaceVariablesWithValue(Pattern);
                var tempRegex = new Regex(tempPattern, RegexOptions.IgnoreCase);
                match = tempRegex.Match(line);
            }
            else
            {
                // Run the match normal Match, this will be most all cases.
                match = this.Regex?.Match(line);
            }

            // If it's not a match, get out.
            if (match == null || !match.Success)
            {
                return false;
            }

            // If it's supposed to auto disable itself after it fires then set that.
            if (this.DisableAfterTriggered)
            {
                this.Enabled = false;
            }

            // Save the match for CLR processing if needed.
            this.Match = match;

            // Do we skip the variable set (used mainly when this is called from gags)... also skips the TriggeringText and
            // ProcessedCommand sets because they're not needed in this case.  I haven't had ANY issues with this in a lot of
            // testing but since Trigger is a reference we need to be careful about race conditions that might arise from
            // using the below properties if anything that's async takes advantage of this (you could get some hard to track down
            // bugs in that case where one trigger set the ProcessedCommand and another set it to another value before the first
            // had finished its Command).
            if (skipVariableSet == false)
            {
                // Save the text that triggered this trigger so that it can be used if needed elsewhere like in
                // a CLR trigger.
                TriggeringText = line;

                // Setup the command that we may or may not process
                ProcessedCommand = this.Command ?? "";

                // Allow the user to have the content of the last trigger if they need it.
                if (this.IsLua == false)
                {
                    ProcessedCommand = ProcessedCommand.Replace("%0", TriggeringText);
                }
                else
                {
                    ProcessedCommand = ProcessedCommand.Replace("%0", TriggeringText.Replace("\"", "\\\""));
                }

                // Go through any groups backwards that came back in the trigger match.  Groups are matched in reverse
                // order so that %1 doesn't overwrite %12 and leave a trailing 2.
                for (int i = match.Groups.Count - 1; i >= 0; i--)
                {
                    // If it's a named match, we specifically named it in the trigger and thus we're going
                    // to automatically store it in a variable that can then be used later by aliases, triggers, etc.
                    // If there are variables that came back that aren't named, throw those into the more generic
                    // %1, %2, %3 values.
                    if (!string.IsNullOrWhiteSpace(match.Groups[i].Name) && !match.Groups[i].Name.IsNumeric() && !string.IsNullOrWhiteSpace(match.Groups[i].Value))
                    {
                        Conveyor.SetVariable(match.Groups[i].Name, match.Groups[i].Value);
                    }
                    else
                    {
                        // TODO - Consider StringBuilder
                        // TODO - Consider doing both the variables sets, and then ALSO the pattern matches.
                        // Replace %1, %2, etc. variables with their values from the pattern match.
                        if (this.IsLua == false)
                        {
                            ProcessedCommand = ProcessedCommand.Replace($"%{i}", match.Groups[i].Value);
                        }
                        else
                        {
                            ProcessedCommand = ProcessedCommand.Replace($"%{i}", match.Groups[i].Value.Replace("\"", "\\\""));
                        }
                    }
                }
            }

            // If the profile setting to track the last trigger date is set then set it.
            if (this.Conveyor?.ProfileSettings?.TrackTriggerLastMatched == true)
            {
                this.LastMatched = DateTime.Now;
            }

            return match.Success;
        }

        /// <summary>
        /// The command after it's been processed.  This is what should get sent to the game.
        /// </summary>
        [JsonIgnore]
        public string ProcessedCommand { get; private set; } = "";

        /// <summary>
        /// The text that triggered the trigger.
        /// </summary>
        [JsonIgnore]
        public string TriggeringText { get; private set; } = "";

        [JsonIgnore]
        public Match Match { get; set; }

        private string _command = "";

        /// <inheritdoc/>
        public virtual string Command
        {
            get => _command;
            set
            {
                _command = value;
                OnPropertyChanged(nameof(Command));
            }
        }

        private string _pattern = "";

        /// <inheritdoc/>
        public string Pattern
        {
            get => _pattern;
            set
            {
                _pattern = value;
                OnPropertyChanged(nameof(Pattern));

                try
                {
                    this.Regex = new Regex(_pattern, RegexOptions.Compiled);
                }
                catch (Exception ex)
                {
                    this.Conveyor?.EchoLog($"Trigger creation error: {ex.Message}", LogType.Error);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Execute()
        {
        }

        private string _character = "";

        /// <inheritdoc/>
        public string Character
        {
            get => _character;
            set
            {
                _character = value;
                OnPropertyChanged(nameof(Character));
            }
        }

        private string _group = "";

        /// <inheritdoc/>
        public string Group
        {
            get => _group;
            set
            {
                _group = value;
                OnPropertyChanged(nameof(Group));
            }
        }

        private bool _isSilent = false;

        /// <inheritdoc/>
        public bool IsSilent
        {
            get => _isSilent;

            set
            {
                if (value != _isSilent)
                {
                    _isSilent = value;
                    OnPropertyChanged(nameof(IsSilent));
                }
            }
        }

        private bool _isLua = false;

        /// <inheritdoc/>
        public bool IsLua
        {
            get => _isLua;

            set
            {
                if (value != _isLua)
                {
                    _isLua = value;
                    OnPropertyChanged(nameof(IsLua));
                }
            }
        }

        private bool _plugin = false;

        /// <inheritdoc/>
        public bool Plugin
        {
            get => _plugin;

            set
            {
                if (value != _plugin)
                {
                    _plugin = value;
                    OnPropertyChanged(nameof(Plugin));
                }
            }
        }

        private bool _disableAfterTriggered = false;

        /// <inheritdoc/>
        public bool DisableAfterTriggered
        {
            get => _disableAfterTriggered;

            set
            {
                if (value != _disableAfterTriggered)
                {
                    _disableAfterTriggered = value;
                    OnPropertyChanged(nameof(DisableAfterTriggered));
                }
            }
        }

        private bool _lock = false;

        /// <inheritdoc/>
        public bool Lock
        {
            get => _lock;

            set
            {
                if (value != _lock)
                {
                    _lock = value;
                    OnPropertyChanged(nameof(Lock));
                }
            }
        }

        /// <inheritdoc/>
        public DateTime LastMatched { get; set; } = DateTime.MinValue;

        private bool _variableReplacement = false;

        /// <inheritdoc/>
        public bool VariableReplacement
        {
            get => _variableReplacement;
            set
            {
                _variableReplacement = value;
                OnPropertyChanged(nameof(VariableReplacement));
            }
        }

        private bool _enabled = true;

        /// <inheritdoc/>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        private bool _gag = false;

        /// <inheritdoc/>
        public bool Gag
        {
            get => _gag;
            set
            {
                if (value != _gag)
                {
                    _gag = value;
                    OnPropertyChanged(nameof(Gag));
                }
            }
        }

        private TerminalTarget _moveTo = TerminalTarget.None;

        /// <inheritdoc/>
        public TerminalTarget MoveTo
        {
            get => _moveTo;
            set
            {
                if (value != _moveTo)
                {
                    _moveTo = value;
                    OnPropertyChanged(nameof(MoveTo));
                }
            }
        }

        private bool _highlightLine = false;

        /// <inheritdoc />
        public bool HighlightLine
        {
            get => _highlightLine;
            set
            {
                if (value != _highlightLine)
                {
                    _highlightLine = value;
                    OnPropertyChanged(nameof(HighlightLine));
                }
            }
        }

        /// <inheritdoc />
        [JsonIgnore]
        public IConveyor Conveyor { get; set; }

        private int _count = 0;

        /// <inheritdoc />
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        private int _priority = 10000;

        /// <inheritdoc />
        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        private bool _stopProcessing = false;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool StopProcessing
        {
            get => _stopProcessing;
            set
            {
                _stopProcessing = value;
                OnPropertyChanged(nameof(StopProcessing));
            }
        }

        /// <inheritdoc />
        public string Identifier { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public string PackageId { get; set; } = "";

        /// <inheritdoc />
        public bool SystemTrigger { get; set; } = false;

        /// <inheritdoc />
        [JsonIgnore]
        public Regex Regex { get; set; }

        /// <summary>
        /// Clones the trigger.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);

        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}