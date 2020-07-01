namespace Avalon.Common.Models
{
    public enum WindowType
    {
        /// <summary>
        /// A blank window.
        /// </summary>
        Blank,
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
