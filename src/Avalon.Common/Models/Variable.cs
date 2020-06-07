using Avalon.Common.Interfaces;
using System.ComponentModel;

namespace Avalon.Common.Models
{
    public class Variable : INotifyPropertyChanged, IVariable
    {
        public Variable()
        {

        }

        public Variable(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public Variable(string key, string value, string character)
        {
            this.Key = key;
            this.Value = value;
            this.Character = character;
        }

        private string _key = "";
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        private string _value = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        private string _character = "";
        public string Character
        { 
            get
            {
                return _character;
            }
            set
            {
                _character = value;
                OnPropertyChanged("Character");
            }            
        }

        private bool _isVisible = false;

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(this.IsVisible));
                }
            }
        }

        private int _displayOrder = 0;

        public int DisplayOrder
        {
            get
            {
                return _displayOrder;
            }

            set
            {
                if (value != _displayOrder)
                {
                    _displayOrder = value;
                    OnPropertyChanged(nameof(this.DisplayOrder));
                }
            }
        }

        private bool _displayLabel = true;

        public bool DisplayLabel
        {
            get
            {
                return _displayLabel;
            }

            set
            {
                if (value != _displayLabel)
                {
                    _displayLabel = value;
                    OnPropertyChanged(nameof(this.DisplayLabel));
                }
            }
        }

        private string _label = "";

        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                if (value != _label)
                {
                    _label = value;
                    OnPropertyChanged(nameof(this.Label));
                }
            }
        }

        protected virtual async void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
