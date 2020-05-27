using Avalon.Common.Models;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Represents an abstracted window that can be called from plugins.
    /// </summary>
    public interface ITerminalWindow
    {
        /// <summary>
        /// Appends text to the terminal.
        /// </summary>
        /// <param name="text"></param>
        void AppendText(string text);

        /// <summary>
        /// Appends a line to the terminal.
        /// </summary>
        /// <param name="line"></param>
        void AppendText(Line line);

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();

        /// <summary>
        /// The name of the window.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The text for the status bar of the terminal window.
        /// </summary>
        string StatusText { get; set; }

        /// <summary>
        /// The full text in the terminal window.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The title of the window.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The topmost screen pixel the form should be placed at.
        /// </summary>
        double Top { get; set; }

        /// <summary>
        /// The leftmost screen pixel the form should be placed at.
        /// </summary>
        double Left { get; set; }

        /// <summary>
        /// The width in pixels of the form.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// The height in pixels of the form.
        /// </summary>
        double Height { get; set; }

    }

}
