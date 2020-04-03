using System.Windows;
using System.IO;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for global client settings.
    /// </summary>
    public partial class ClientSettingsWindow : Window
    {
        public ClientSettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Code that runs when the settings form is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {            
            propertyGrid.SelectedObject = App.Settings.AvalonSettings;
            TextSettingsFilename.Text = App.Settings.AvalonSettingsFile;
        }

        /// <summary>
        /// Closes the settings form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            App.MainWindow.UpdateUISettings();
            this.Close();
        }
    }
}
