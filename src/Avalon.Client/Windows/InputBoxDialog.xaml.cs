using ModernWpf.Controls;
using System.Threading.Tasks;

namespace Avalon
{
    /// <summary>
    /// An input box that can ask for string data.
    /// </summary>
    public partial class InputBoxDialog : ContentDialog
    {
        public InputBoxDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Code to execute when the dialog loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ContentDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // This is a hack, delay half a second so that the Focus sticks.
            await Task.Delay(500);
            TextBoxInput.Focus();            
        }

        /// <summary>
        /// The caption of the text box.
        /// </summary>
        public string Caption
        {
            get => this.TextBlockCaption.Text;
            set => this.TextBlockCaption.Text = value;
        }

        /// <summary>
        /// The text that was entered in the text box.
        /// </summary>
        public string Text
        {
            get => TextBoxInput.Text;
            set => TextBoxInput.Text = value;
        }

        /// <summary>
        /// Cancels the input box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            TextBoxInput.Text = "";
        }

        /// <summary>
        /// Cancels the input box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentDialog_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                TextBoxInput.Text = "";
            }
            else
            {
                e.Handled = false;
            }
        }
    }
}
