using Argus.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for BarRepeater.xaml
    /// </summary>
    public partial class BarRepeater : UserControl
    {

        public ObservableCollection<Bar> BarItems;

        /// <summary>
        /// Constructor
        /// </summary>
        public BarRepeater()
        {
            InitializeComponent();
            BarItems = new ObservableCollection<Bar>();
            repeater.ItemsSource = BarItems;
            this.StatusBarVisible = false;
        }

        /// <summary>
        /// Clears all items in the list.
        /// </summary>
        public void Clear()
        {
            this.BarItems.Clear();
        }

        /// <summary>
        /// Adds an item into the progress bar repeater list.  If a key exists it's value
        /// and text will be updated.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        /// <param name="text"></param>
        /// <param name="key"></param>
        public void Add(int value, int maximum, string text, string key)
        {
            // Let's try an add or update.
            var bar = this.BarItems.FirstOrDefault(x => x.Key == key);

            if (bar == null)
            {
                // New
                this.BarItems.Add(new Bar(value, maximum, text, key));
            }
            else
            {
                // Update
                bar.Text = text;
                bar.Value = value;
            }

            this.scrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Removes at item from the BarRepeater.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            for (int i = this.BarItems.Count - 1; i >= 0; i--)
            {
                if (this.BarItems[i].Key.Equals(key, StringComparison.Ordinal))
                {
                    this.BarItems.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Whether a specific entry exists or not.
        /// </summary>
        /// <param name="key"></param>
        public bool Exists(string key)
        {
            return this.BarItems.Any(x => x.Key == key);
        }

        /// <summary>
        /// The status text to display on the warning bar.
        /// </summary>
        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set 
            {
                SetValue(StatusTextProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for WarningText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(BarRepeater), new PropertyMetadata(""));

        /// <summary>
        /// Whether or not the status bar should show.
        /// </summary>
        public bool StatusBarVisible
        {
            get { return (bool)GetValue(StatusBarVisibleProperty); }
            set
            { 
                SetValue(StatusBarVisibleProperty, value);
            }
        }

        public static readonly DependencyProperty StatusBarVisibleProperty =
            DependencyProperty.Register("StatusBarVisible", typeof(bool), typeof(BarRepeater), new PropertyMetadata(false));

        /// <summary>
        /// A colored progress bar.
        /// </summary>
        public class Bar : INotifyPropertyChanged
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Bar()
            {

            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="value"></param>
            /// <param name="maximum"></param>
            /// <param name="text"></param>
            /// <param name="key"></param>
            public Bar(int value, int maximum, string text, string key)
            {
                this.Value = value;
                this.Text = text;
                this.Maximum = maximum;
                this.Key = key;
            }

            /// <summary>
            /// They key that represents this bar.
            /// </summary>
            public string Key { get; set; } = "";

            private int _value = 0;

            /// <summary>
            /// The value of the progress bar.  This can be over the maximum (and will display at a full bar if so).
            /// </summary>
            public int Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                    OnPropertyChanged("Value");

                    var convert = new BrushConverter();

                    if (this.Value <= 2)
                    {
                        this.Background = Brushes.Red;
                    }
                    else if (this.Value == 3 || this.Value == 4 || this.Value == 5)
                    {
                        this.Background = (SolidColorBrush)convert.ConvertFrom("#ae9818");
                    }
                    else
                    {
                        this.Background = (SolidColorBrush)convert.ConvertFrom("#0078D7");
                    }
                }
            }

            private int _maximum = 0;

            /// <summary>
            /// The maximum for the progress bar.
            /// </summary>
            public int Maximum
            {
                get
                {
                    return _maximum;
                }
                set
                {
                    _maximum = value;
                    OnPropertyChanged("Maximum");
                }
            }

            private string _text = "";

            /// <summary>
            /// The text to display on the progress bar.
            /// </summary>
            public string Text
            {
                get
                {
                    return _text;
                }
                set
                {
                    _text = value;
                    OnPropertyChanged("Text");
                }
            }

            private Brush _background;

            /// <summary>
            /// The fill color for the progress bar.  This will be dynamically set from the value property.
            /// </summary>
            public Brush Background
            {
                get
                {
                    return _background;
                }
                set
                {
                    _background = value;
                    OnPropertyChanged("Background");
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
}
