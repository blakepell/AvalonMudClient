using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for BarRepeater.xaml
    /// </summary>
    public partial class BarRepeater : UserControl
    {
        public ObservableCollection<Bar> BarItems;

        public BarRepeater()
        {
            InitializeComponent();
            BarItems = new ObservableCollection<Bar>();
            repeater.ItemsSource = BarItems;            
        }

        public void Clear()
        {
            this.BarItems.Clear();
        }

        public void Add(int value, int maximum, string text, string key)
        {
            //this.BarItems.Add(new Bar(value, maximum, text));
            //this.scrollViewer.ScrollToEnd();

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

        public class Bar : INotifyPropertyChanged
        {
            public Bar()
            {

            }

            public Bar(int value, int maximum, string text, string key)
            {
                this.Value = value;
                this.Text = text;
                this.Maximum = maximum;
                this.Key = key;
            }

            public string Key { get; set; } = "";

            private int _value = 0;

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
                        // (SolidColorBrush)convert.ConvertFrom("#6c2020")
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

            public int Maximum { get; set; } = 0;

            private string _text = "";

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
