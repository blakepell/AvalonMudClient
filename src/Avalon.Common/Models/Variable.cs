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

namespace Avalon.Common.Models
{
    /// <summary>
    /// A variable that is persisted with the profile that can be used to track
    /// data as well as be coupled with aliases/triggers and the script engine.
    /// </summary>
    public class Variable : INotifyPropertyChanged, IVariable, IModelInfo
    {
        public Variable()
        {

        }

        public Variable(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public Variable(string key, string value, string color)
        {
            this.Key = key;
            this.Value = value;
            this.ForegroundColor = color;
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
                OnPropertyChanged("Self");
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
                OnPropertyChanged("Self");
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
                    OnPropertyChanged("Self");
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
                    OnPropertyChanged("Self");
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
                    OnPropertyChanged("Self");
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
                    OnPropertyChanged("Self");
                }
            }
        }


        private string _onChangeEvent;

        /// <inheritdoc />
        public string OnChangeEvent
        {
            get => _onChangeEvent;
            set
            {
                if (value != _onChangeEvent)
                {
                    _onChangeEvent = value;
                    OnPropertyChanged(nameof(this.OnChangeEvent));
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

        /// <inheritdoc />
        public bool IsEmpty()
        {
            // A variable is considered empty if the Key is blank.
            return this.Key.IsNullOrEmptyOrWhiteSpace();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}