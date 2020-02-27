using System;
using System.ComponentModel;
using Avalon.Common.Interfaces;

namespace Avalon.Common.Models
{
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

        public string AliasExpression { get; set; }

        private string _command;
        public string Command
        {
            get => _command;
            set
            {
                _command = value;
                OnPropertyChanged("Command");
            }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// The character who the trigger should be isolated to (if any).
        /// </summary>
        public string Character { get; set; }

        public string Group { get; set; }

        public bool IsLua { get; set; } = false;

        private int _count = 0;
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        protected virtual async void OnPropertyChanged(string propertyName)
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
