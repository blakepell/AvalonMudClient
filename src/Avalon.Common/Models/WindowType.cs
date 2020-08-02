namespace Avalon.Common.Models
{
    public enum WindowType
    {
        /// <summary>
        /// A default window which represents a normal window for the application (e.g. a WPF window if
        /// running WPF on Windows).
        /// </summary>
        Default,
        /// <summary>
        /// A window that inherits from ITerminalWindow
        /// </summary>
        TerminalWindow,
        /// <summary>
        /// A window that implements ICompassWindow
        /// </summary>
        CompassWindow
    }
}
