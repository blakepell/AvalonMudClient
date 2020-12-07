using Avalon.Common.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Avalon.Common.Models
{
    /// <summary>
    /// A variable that is persisted with the profile that can be used to track
    /// data as well as be coupled with aliases/triggers and the script engine.
    /// </summary>
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

        /// <inheritdoc />
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        private string _value = "";

        /// <inheritdoc />
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));

                // Also notify that the object itself has changed for bindings to update.
                OnPropertyChanged("Self");
            }
        }

        private string _character = "";

        /// <inheritdoc />
        public string Character
        { 
            get => _character;
            set
            {
                _character = value;
                OnPropertyChanged(nameof(Character));
            }            
        }

        private bool _isVisible = false;

        /// <inheritdoc />
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        private int _displayOrder = 0;

        /// <inheritdoc />
        public int DisplayOrder
        {
            get => _displayOrder;
            set
            {
                if (value != _displayOrder)
                {
                    _displayOrder = value;
                    OnPropertyChanged(nameof(DisplayOrder));
                }
            }
        }

        private string _label = "";

        /// <inheritdoc />
        public string Label
        {
            get => _label;
            set
            {
                if (value != _label)
                {
                    // Update both the label, and send a notice that the formatted label has
                    // changed as well.
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                    OnPropertyChanged(nameof(FormattedLabel));
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

        /// <inheritdoc />
        public string ForegroundColor
        {
            get => _foregroundColor;
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
        public Variable Self => this;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
