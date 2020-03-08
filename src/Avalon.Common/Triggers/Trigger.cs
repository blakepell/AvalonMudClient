using Argus.Extensions;
using Avalon.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalon.Common.Models;

namespace Avalon.Common.Triggers
{
    public class Trigger : ITrigger, ICloneable, INotifyPropertyChanged
    {
        public Trigger()
        {
        }

        public Trigger(string pattern, string command)
        {
            this.Pattern = pattern;
            this.Command = command;
        }

        public Trigger(string pattern, string command, string character, bool silent, string identifier)
        {
            this.Pattern = pattern;
            this.Command = command;
            this.Character = character;
            this.Identifier = identifier;
            this.IsSilent = silent;
        }

        public Trigger(string pattern, string command, string character, bool silent, string identifier, TerminalTarget moveTo, bool gag)
        {
            this.Pattern = pattern;
            this.Command = command;
            this.Character = character;
            this.Identifier = identifier;
            this.IsSilent = silent;
            this.MoveTo = moveTo;
            this.Gag = gag;
        }

        /// <summary>
        /// Matches a trigger.  Also allows for the variable replacement triggers to be explicitly ignored by the caller
        /// regardless of how they're setup by the user.  This is important because the screen rendering code from AvalonEdit
        /// will hit those triggers over and over as each line comes in.  Variable replace should be ignored in those cases
        /// because they've already been processed (and in some cases it will cause them to re-process out of order).  If you're
        /// reading variables in from a prompt and getting say, a room name, you want that to process once, when you're there
        /// and not out of order.  To be clear, this isn't replace @ variables in the pattern, it's the part that sets the
        /// the variable later down the line.  The first "variable replacement" has to happen in both cases.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="skipVariableSet">Default false: Whether to explicitly skip variable setting (not replacing).</param>
        public bool IsMatch(string line, bool skipVariableSet = false)
        {
            Match match;

            // Does this trigger contain any variables?  If so, we'll need to special handle it.  We're also
            // going to require that the VariableReplacement value is set to true so the player has to
            // specifically opt into this.  Since the Gag triggers run -a lot- on the terminal rendering
            // the bool will be more performant as a first check before the string contains check.  This is
            // a micro optimization that had real payoff in the performance profiler.
            if (this.VariableReplacement && Pattern.Contains('@', StringComparison.Ordinal))
            {
                // Replace any variables with their literal values.
                string tempPattern = this.Conveyor.ReplaceVariablesWithValue(Pattern);
                var tempRegex = new Regex(tempPattern, RegexOptions.IgnoreCase);
                match = tempRegex.Match(line);
            }
            else
            {
                // Run the match normal Match, this will be most all cases.
                match = _regex?.Match(line);
            }

            // If it's not a match, get out.
            if (match == null || !match.Success)
            {
                return false;
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
                ProcessedCommand = this.Command;

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
            if (this.Conveyor.ProfileSettings.TrackTriggerLastMatched)
            {
                this.LastMatched = DateTime.Now;
            }

            return match.Success;
        }

        /// <summary>
        /// The command after it's been processed.  This is what should get sent to the game.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// The command as entered by the user before it is processed in anyway.
        /// </summary>
        public virtual string Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
                OnPropertyChanged("Command");
            }
        }

        private string _pattern = "";

        /// <summary>
        /// The regular expression pattern to match the trigger on.
        /// </summary>
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                _pattern = value;

                try
                {
                    _regex = new Regex(_pattern, RegexOptions.Compiled);
                }
                catch
                {
                    // TODO
                    // They might have been updating the trigger and the pattern failed, consider logging this
                    // under developer mode
                }
            }
        }

        /// <summary>
        /// Execute command that is overridable.
        /// </summary>
        public virtual void Execute()
        {
            return;
        }

        /// <summary>
        /// The character who the trigger should be isolated to (if any).
        /// </summary>
        public string Character { get; set; } = "";

        /// <summary>
        /// The group the trigger is in.  This can be used to toggle all triggers on or off.
        /// </summary>
        public string Group { get; set; } = "";

        /// <summary>
        /// Whether the triggers output should be silent (not echo to the main terminal).
        /// </summary>
        public bool IsSilent { get; set; } = false;

        /// <summary>
        /// Whether the command should be executed as a Lua script.
        /// </summary>
        public bool IsLua { get; set; } = false;

        /// <summary>
        /// Indicates whether a trigger was loaded from a plugin or not.
        /// </summary>
        public bool Plugin { get; set; } = false;

        /// <summary>
        /// The date/time the trigger last fired successfully.  This can be useful in tracking down
        /// errant triggers that are running (when you have -a lot- of them).  This can be toggled not
        /// to set via the TrackTriggerLastMatched profile setting.
        /// </summary>
        public DateTime LastMatched { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Whether or not variables should be replaced in the pattern.  This is offered as
        /// a performance tweak so the player has to opt into it.
        /// </summary>
        public bool VariableReplacement { get; set; } = false;

        private bool _enabled = true;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public bool Gag { get; set; } = false;

        /// <summary>
        /// What terminal window to move the triggered line to.
        /// </summary>
        public TerminalTarget MoveTo { get; set; } = TerminalTarget.None;

        public bool HighlightLine { get; set; } = false;

        /// <summary>
        /// A Conveyor so the trigger can interact with the UI if it's a CLR trigger.
        /// </summary>
        [JsonIgnore]
        public IConveyor Conveyor { get; set; }

        private int _count = 0;

        /// <summary>
        /// The number of a times a trigger has fired.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        public string Identifier { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Clones the trigger.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private Regex _regex;

        protected virtual async void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);

        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
