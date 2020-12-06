using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for BarRepeater.xaml
    /// </summary>
    public partial class BarRepeater
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
        /// Adds an item into the progress bar repeater list.  If a key exists it's value
        /// and text will be updated.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <param name="command"></param>
        public void Add(int value, int maximum, string text, string key, string command)
        {
            // Let's try an add or update.
            var bar = this.BarItems.FirstOrDefault(x => x.Key == key);

            if (bar == null)
            {
                // New
                this.BarItems.Add(new Bar(value, maximum, text, key, command));
            }
            else
            {
                // Update
                bar.Text = text;
                bar.Value = value;
                bar.Command = command;
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
        /// If this is enabled and a command has been set in the Tag then send it to the mud.  This will allow
        /// people to execute commands from progress bar items.  In the case of a Diku/ROM style mud this might
        /// manifest as recasting a spell that was close to wearing off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.EnableMouseClick)
            {
                return;
            }

            try
            {
                var tb = e.OriginalSource as TextBlock;

                if (tb != null && tb.Tag == null)
                {
                    return;
                }

                string cmd = (string)tb.Tag;

                App.MainWindow.Interp.Send(cmd);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);
                App.Conveyor.EchoLog(ex?.StackTrace ?? "Stack trace was empty.", Common.Models.LogType.Error);
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
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for WarningText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(BarRepeater), new PropertyMetadata(""));

        /// <summary>
        /// Whether or not the status bar should show.
        /// </summary>
        public bool StatusBarVisible
        {
            get => (bool)GetValue(StatusBarVisibleProperty);
            set => SetValue(StatusBarVisibleProperty, value);
        }

        public static readonly DependencyProperty StatusBarVisibleProperty =
            DependencyProperty.Register(nameof(StatusBarVisible), typeof(bool), typeof(BarRepeater), new PropertyMetadata(false));

        /// <summary>
        /// Whether handling the mouse click event is enabled.
        /// </summary>
        public bool EnableMouseClick
        {
            get => (bool)GetValue(EnableMouseClickProperty);
            set => SetValue(EnableMouseClickProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableMouseClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMouseClickProperty =
            DependencyProperty.Register(nameof(EnableMouseClick), typeof(bool), typeof(BarRepeater), new PropertyMetadata(false));

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
            /// Constructor
            /// </summary>
            /// <param name="value"></param>
            /// <param name="maximum"></param>
            /// <param name="text"></param>
            /// <param name="key"></param>
            /// <param name="command"></param>
            public Bar(int value, int maximum, string text, string key, string command)
            {
                this.Value = value;
                this.Text = text;
                this.Maximum = maximum;
                this.Key = key;
                this.Command = command;
            }

            /// <summary>
            /// They key that represents this bar.
            /// </summary>
            public string Key { get; set; } = "";

            private int _value;

            /// <summary>
            /// The value of the progress bar.  This can be over the maximum (and will display at a full bar if so).
            /// </summary>
            public int Value
            {
                get => _value;
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

            private int _maximum;

            /// <summary>
            /// The maximum for the progress bar.
            /// </summary>
            public int Maximum
            {
                get => _maximum;
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
                get => _text;
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
                get => _background;
                set
                {
                    _background = value;
                    OnPropertyChanged("Background");
                }
            }

            private string _command = "";

            /// <summary>
            /// Command to send to the game if the control supports the mouse clicks being enabled.
            /// </summary>
            public string Command
            { 
                get => _command;
                set
                {
                    _command = value;
                    OnPropertyChanged("Command");
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
