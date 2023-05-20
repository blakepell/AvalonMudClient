/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Scripting;

namespace Avalon.Common.Models
{
    /// <summary>
    /// An alias that invokes another command, a series of commands or a script by a
    /// provided scripting engine.
    /// </summary>
    public class Alias : INotifyPropertyChanged, ICloneable, IAlias
    {
        public Alias()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public Alias(string aliasExpression, string command)
        {
            this.AliasExpression = aliasExpression;
            this.Command = command;
            this.Id = Guid.NewGuid().ToString();
        }

        public Alias(string aliasExpression, string command, string group)
        {
            this.AliasExpression = aliasExpression;
            this.Command = command;
            this.Group = group;
            this.Id = Guid.NewGuid().ToString();
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

        /// <inheritdoc />
        public string AliasExpression { get; set; } = "";

        private string _command = "";

        /// <inheritdoc />
        public string Command
        {
            get => _command;
            set
            {
                _command = value;
                OnPropertyChanged(nameof(this.Command));
                this.UpdateScriptingEnvironment();
            }
        }

        private bool _enabled = true;

        /// <inheritdoc />
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(this.Enabled));
            }
        }

        /// <inheritdoc />
        public string Character { get; set; } = "";

        /// <inheritdoc />
        public string Group { get; set; } = "";

        /// <inheritdoc />
        public bool IsLua { get; set; } = false;

        private string _id;

        /// <inheritdoc />
        public string Id
        {
            get => _id;
            set
            {
                if (value != null)
                {
                    this.FunctionName = ScriptHost.GetFunctionName(value, "a");
                }

                _id = value;
                OnPropertyChanged(nameof(this.Id));
            }
        }

        private string _functionName;

        /// <summary>
        /// The name of the function for the OnMatchedEvent.
        /// </summary>
        [JsonIgnore]
        public string FunctionName
        {
            get => _functionName; 
            set
            {
                if (value == null)
                {
                    return;
                }

                _functionName = value;
            }
        }

        private ExecuteType _executeType = ExecuteType.Command;

        /// <inheritdoc />
        public ExecuteType ExecuteAs
        {
            get => _executeType;
            set
            {
                _executeType = value;
                OnPropertyChanged(nameof(this.ExecuteAs));
                this.UpdateScriptingEnvironment();
            }
        }

        /// <inheritdoc />
        public bool Lock { get; set; } = false;

        private int _count = 0;

        /// <inheritdoc />
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged(nameof(this.Count));
            }
        }

        /// <inheritdoc />
        public string PackageId { get; set; } = "";

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Clones the alias.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}