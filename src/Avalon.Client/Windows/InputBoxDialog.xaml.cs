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
            get
            {
                return this.TextBlockCaption.Text;
            }
            set
            {
                this.TextBlockCaption.Text = value;
            }
        }

        /// <summary>
        /// The text that was entered in the text box.
        /// </summary>
        public string Text
        {
            get
            {
                return TextBoxInput.Text;
            }
        }

    }
}
