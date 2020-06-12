using Argus.Extensions;
using Avalon.Common.Interfaces;
using Newtonsoft.Json;
using System;
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

                // Also notify that the object itself has changed for bindings to update.
                OnPropertyChanged("Self");
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
                    // Update both the label, and send a notice that the formatted label has
                    // changed as well.
                    _label = value;
                    OnPropertyChanged(nameof(this.Label));
                    OnPropertyChanged(nameof(this.FormattedLabel));
                }
            }
        }

        /// <summary>
        /// Used to properly format the label, choosing the label property or if it doesn't exist the key.
        /// </summary>
        public string FormattedLabel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.Label))
                {
                    return $"{this.Label}:";
                }
                else
                {
                    return $"{this.Key}:";
                }
            }
        }

        private string _foregroundColor;

        /// <summary>
        /// The foreground color that the variable should be displayed in.
        /// </summary>
        public string ForegroundColor
        {
            get
            {
                return _foregroundColor;
            }

            set
            {
                if (value != _foregroundColor)
                {
                    _foregroundColor = value;
                    OnPropertyChanged(nameof(this.ForegroundColor));
                }
            }
        }

        /// <summary>
        /// A self reference used to ease binding scenario.  This property will be marked as
        /// changed when the value of the object changes.  This must be ignored in serialization
        /// scenarios or it will endlessly loop.
        /// </summary>
        [JsonIgnore]
        public Variable Self
        {
            get
            {
                return this;
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
