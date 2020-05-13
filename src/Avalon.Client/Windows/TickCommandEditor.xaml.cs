using System.Windows;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class TickCommandEditor : Window
    {
        /// <summary>
        /// The value of the Lua text editor.
        /// </summary>
        public string Text
        {
            get => AvalonCommandEditor.Text;
            set => AvalonCommandEditor.Text = value;
        }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TickCommandEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TickCommandEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the initial values
            CheckBoxEnabled.IsChecked = App.Settings.ProfileSettings.EnableCommandsOnTick;
            AvalonCommandEditor.Text = App.Settings.ProfileSettings.ExecuteCommandsOnTick;

            // Set the focus onto the text editor.
            AvalonCommandEditor.Focus();
        }

        /// <summary>
        /// Closes the window and does not save changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Closes the window and saves changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            App.Settings.ProfileSettings.EnableCommandsOnTick = CheckBoxEnabled.IsChecked ?? false;
            App.Settings.ProfileSettings.ExecuteCommandsOnTick = AvalonCommandEditor.Text;
            this.DialogResult = true;
            this.Close();
        }

    }
}
