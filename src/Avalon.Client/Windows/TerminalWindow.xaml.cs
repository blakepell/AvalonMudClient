using Avalon.Common.Models;
using System.Windows;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class TerminalWindow : Window
    {
        /// <summary>
        /// The value of the Lua text editor.
        /// </summary>
        public string Text
        {
            get => Terminal.Text;
            set => Terminal.Text = value;
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
        public TerminalWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminalWindowWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Appends text to the terminal.
        /// </summary>
        /// <param name="text"></param>
        public void AppendText(string text)
        {
            Terminal.Append(text, true);
        }

        /// <summary>
        /// Appends a line to the terminal.
        /// </summary>
        /// <param name="line"></param>
        public void AppendText(Line line)
        {
            Terminal.Append(line);
        }

    }
}
