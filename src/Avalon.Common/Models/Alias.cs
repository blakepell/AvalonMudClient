using System;
using System.ComponentModel;
using Avalon.Common.Interfaces;

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

        }

        public Alias(string aliasExpression, string command)
        {
            this.AliasExpression = aliasExpression;
            this.Command = command;
        }

        public Alias(string aliasExpression, string command, string group)
        {
            this.AliasExpression = aliasExpression;
            this.Command = command;
            this.Group = group;
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
                OnPropertyChanged(nameof(Command));
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
                OnPropertyChanged(nameof(Enabled));
            }
        }

        /// <inheritdoc />
        public string Character { get; set; } = "";

        /// <inheritdoc />
        public string Group { get; set; } = "";

        /// <inheritdoc />
        public bool IsLua { get; set; } = false;

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
                OnPropertyChanged(nameof(Count));
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
