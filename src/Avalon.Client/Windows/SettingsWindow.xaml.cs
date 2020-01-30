using System.Windows;
using System.IO;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
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
            propertyGrid.SelectedObject = App.Settings.ProfileSettings;
            TextSettingsFilename.Text = Path.Join(App.Settings.AvalonSettings.SaveDirectory, App.Settings.ProfileSettings.FileName);
        }

        /// <summary>
        /// Closes the settings form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
