using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void Add(int value, int maximum, string text)
        {
            this.BarItems.Add(new Bar(value, maximum, text));
            this.scrollViewer.ScrollToEnd();
        }

        public class Bar
        {
            public Bar()
            {

            }

            public Bar(int value, int maximum, string text)
            {
                this.Value = value;
                this.Text = text;
                this.Maximum = maximum;

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

            public int Value { get; set; } = 0;

            public int Maximum { get; set; } = 0;

            public string Text { get; set; } = "";

            public Brush Background { get; set; }

        }

    }
}
