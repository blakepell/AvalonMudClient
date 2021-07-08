/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Common.Scripting;
using Cysharp.Text;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Avalon.Common.Triggers
{
    /// <summary>
    /// A trigger is an action that is executed based off of a pattern that is sent from the game.
    /// </summary>
    public class Trigger : ITrigger, ICloneable, INotifyPropertyChanged, IModelInfo
    {
        public Trigger()
        {
            this.Identifier = Guid.NewGuid().ToString();
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
        public virtual bool IsMatch(string line)
        {
            Match match;
            var conveyor = AppServices.GetService<IConveyor>();

            // Does this trigger contain any variables?  If so, we'll need to special handle it.  We're also
            // going to require that the VariableReplacement value is set to true so the player has to
            // specifically opt into this.  Since the Gag triggers run -a lot- on the terminal rendering
            // the bool will much faster as a first check before the string contains check.  This is
            // a micro optimization that had real payoff in the performance profiler.  Also when profiling, IndexOf
            // a char without Ordinal consistently ran faster than Contains and IndexOf with Ordinal.
            if (this.VariableReplacement && Pattern.IndexOf('@') >= 0)
            {
                // Replace any variables with their literal values.
                string tempPattern = conveyor.ReplaceVariablesWithValue(Pattern);
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

            if (this.LineTransformer && this.ExecuteAs == ExecuteType.LuaMoonsharp)
            {
                var paramList = new string[match.Groups.Count];
                paramList[0] = line;

                for (int i = 1; i < match.Groups.Count; i++)
                {
                    paramList[i] = match.Groups[i].Value;
                }

                // We'll send the function we want to call but also the code, if the code has changed
                // it nothing will be reloaded thus saving memory and calls.  This is why replacing %1
                // variables is problematic here and why we are forcing the use of Lua varargs (...)
                try
                {
                    var sc = AppServices.GetService<ScriptHost>();
                    this.ProcessedCommand = sc.MoonSharp.ExecuteFunction<string>(this.FunctionName, paramList);
                }
                catch
                {
                    conveyor.EchoError($"The previous exception was from a line transformer trigger with the following pattern: {this.Pattern}");
                    return false;
                }
            }
            else if (this.LineTransformer && this.ExecuteAs == ExecuteType.Command)
            {
                // If this is the route then it will be a 1:1 replacement, no script needs to be run.
                this.ProcessedCommand = this.Command;
            }
            else
            {
                // This is the block that swaps matched groups into the processed command as the user
                // has requested (e.g. %0, %1, %2, %3, etc.)

                // Save the text that triggered this trigger so that it can be used if needed elsewhere like in
                // a CLR trigger.
                TriggeringText = line;

                using (var sb = ZString.CreateStringBuilder())
                {
                    // Set the command that we may or may not process.  Allow the user to have the content of
                    // the last trigger if they need it.
                    sb.Append(this.Command?.Replace("%0", TriggeringText) ?? "");

                    // Go through any groups backwards that came back in the trigger match.  Groups are matched in reverse
                    // order so that %1 doesn't overwrite %12 and leave a trailing 2.
                    for (int i = match.Groups.Count - 1; i >= 0; i--)
                    {
                        // If it's a named match, we specifically named it in the trigger and thus we're going
                        // to automatically store it in a variable that can then be used later by aliases, triggers, etc.
                        // If there are variables that came back that aren't named, throw those into the more generic
                        // %1, %2, %3 values.  TODO - Is this right.. seems like it should do both if needed?
                        if (!string.IsNullOrWhiteSpace(match.Groups[i].Name) && !match.Groups[i].Name.IsNumeric() && !string.IsNullOrWhiteSpace(match.Groups[i].Value))
                        {
                            conveyor.SetVariable(match.Groups[i].Name, match.Groups[i].Value);
                        }
                        else
                        {
                            // Replace %1, %2, etc. variables with their values from the pattern match.  ToString() was
                            // called to avoid a boxing allocation.  If it's Lua we're not going to swap these values
                            // in.  The reason for this is that Lua parameters are passed in via parameters so the script
                            // can be re-used which is MUCH more memory and CPU efficient.
                            sb.Replace($"%{i.ToString()}", match.Groups[i].Value);
                        }
                    }

                    ProcessedCommand = sb.ToString();
                }
            }

            // If the profile setting to track the last trigger date is set then set it.
            if (conveyor?.ProfileSettings?.TrackTriggerLastMatched == true)
            {
                this.LastMatched = DateTime.Now;
            }

            return match.Success;
        }

        public virtual void Execute()
        {

        }

        /// <summary>
        /// Updates the script environment with the contents of script for this trigger if it's set to execute as
        /// a script.  This will need to be called from the ExecuteAs property and the <see cref="Command"/> property to avoid
        /// cases where the lua is set before the <see cref="ExecuteAs"/>.
        /// </summary>
        public void UpdateScriptingEnvironment()
        {
            var sc = AppServices.GetService<ScriptHost>();

            // Load the scripts into the scripting environment.
            if (this.ExecuteAs == ExecuteType.LuaMoonsharp && !string.IsNullOrWhiteSpace(this.Command))
            {
                sc.AddFunction(new SourceCode(this.Command, this.FunctionName, ScriptType.MoonSharpLua));
            }
        }

        private string _identifier;

        /// <inheritdoc />
        public string Identifier
        {
            get
            {
                return _identifier;
            }
            set
            {
                if (value != null)
                {
                    this.FunctionName = ScriptHost.GetFunctionName(value, "tr");
                }

                _identifier = value;
                OnPropertyChanged(nameof(Identifier));
            }
        }

        private string _pattern = "";

        /// <inheritdoc/>
        public string Pattern
        {
            get => _pattern;
            set
            {
                try
                {
                    // Only set the pattern if it compiled.
                    this.Regex = new Regex(value, RegexOptions.Compiled);
                    _pattern = value;
                    OnPropertyChanged(nameof(Pattern));
                }
                catch (Exception ex)
                {
                    var conveyor = AppServices.GetService<IConveyor>();
                    conveyor.EchoLog($"Trigger creation error: {ex.Message}", LogType.Error);
                }
            }
        }

        private string _command = "";

        /// <inheritdoc/>
        public virtual string Command
        {
            get => _command;
            set
            {
                _command = value;
                OnPropertyChanged(nameof(Command));
                this.UpdateScriptingEnvironment();
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

        private ExecuteType _executeType = ExecuteType.Command;

        /// <inheritdoc/>
        public ExecuteType ExecuteAs
        {
            get => _executeType;
            set
            {
                _executeType = value;
                OnPropertyChanged(nameof(ExecuteAs));
                this.UpdateScriptingEnvironment();
            }
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Regex Regex { get; set; }

        /// <inheritdoc />
        public bool SystemTrigger { get; set; } = false;

        /// <inheritdoc />
        public string PackageId { get; set; } = "";

        /// <inheritdoc/>
        public DateTime LastMatched { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The name of the function for the OnMatchedEvent.
        /// </summary>
        [JsonIgnore]
        public string FunctionName { get; set; }

        private bool _temp = false;

        /// <summary>
        /// If the trigger is temporary and should not be saved with the profile.
        /// </summary>
        public bool Temp
        {
            get => _temp;
            set
            {
                _temp = value;
                OnPropertyChanged(nameof(Temp));
            }
        }

        /// <summary>
        /// Clones the trigger.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
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

        private bool _isSilent = false;

        /// <summary>
        /// Whether the triggers output should be silent (not echo to the main terminal).
        /// </summary>
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

        /// <summary>
        /// Whether the command should be executed as a Lua script.
        /// </summary>
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

        private bool _disableAfterTriggered = false;

        /// <summary>
        /// If set to true will disable the trigger after it fires.
        /// </summary>
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

        private bool _variableReplacement = false;

        /// <summary>
        /// Whether or not variables should be replaced in the pattern.  This is offered as
        /// a performance tweak so the player has to opt into it.
        /// </summary>
        public bool VariableReplacement
        {
            get => _variableReplacement;
            set
            {
                _variableReplacement = value;
                OnPropertyChanged(nameof(VariableReplacement));
            }
        }

        private bool _lineTransformer = false;

        /// <summary>
        /// Whether or not this trigger represents a line transformer.
        /// </summary>
        public bool LineTransformer
        {
            get => _lineTransformer;
            set
            {
                _lineTransformer = value;
                OnPropertyChanged(nameof(LineTransformer));
            }
        }

        private bool _gag = false;

        /// <summary>
        /// Whether or not the matching line should be gagged from terminal.  A gagged line is hidden from view
        /// as if it does not exist but does in fact still exist in the terminal.  If triggers are disabled you
        /// will see gagged lines re-appear.  Further, gagged lines will appear in clipboard actions such as copy.
        /// </summary>
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

        /// <summary>
        /// What terminal window to move the triggered line to.
        /// </summary>
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

        /// <summary>
        /// Whether or not the matching line should be highlighted.
        /// </summary>
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

        private bool _stopProcessing = false;

        /// <summary>
        /// If StopProcessing is true then the trigger processing function will stop processing any triggers after
        /// the trigger that fired here.  In order for that to happen, the trigger will need to match.  This will
        /// allow a player to allow for a very efficient trigger loop (but could also cause problems if use incorrectly
        /// in that it will stop trigger processing when this fires).  One thing to note, this is for general purpose
        /// triggers that the user executes but it does not apply to Gag triggers.  Gag triggers inherently work will
        /// gag an entire line and they stop processing as soon as one matches.
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
        public bool IsEmpty()
        {
            if (string.IsNullOrWhiteSpace(this.Pattern))
            {
                return true;
            }

            return false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}