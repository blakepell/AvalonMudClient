using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System.Windows;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class CompassWindow : Window, ICompassWindow
    {
        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// The window type.
        /// </summary>
        public WindowType WindowType { get; set; } = WindowType.CompassWindow;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CompassWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompassWindow_Closed(object sender, System.EventArgs e)
        {
            // Remove this specific window from the shared Conveyor's window list.
            App.Conveyor.WindowList.Remove(this);
        }

        /// <summary>
        /// Sets the angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngle(double angle)
        {
            CompassControl.SetAngle(angle);
        }

        /// <summary>
        /// Sets the angle to a friendly direction (N, NE, E, SE, S, SW, W or NW).
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(string direction)
        {
            CompassControl.SetAngle(direction);
        }

        /// <summary>
        /// Activates the Window and brings it to the forefront and focused.
        /// </summary>
        void IWindow.Activate()
        {
            base.Activate();
        }
    }
}
