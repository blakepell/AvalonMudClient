using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Avalon.Windows
{
    /// <summary>
    /// Interaction logic for DirectionsSelectWindow.xaml
    /// </summary>
    public partial class DirectionsSelectWindow : Window
    {
        public DirectionsSelectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
