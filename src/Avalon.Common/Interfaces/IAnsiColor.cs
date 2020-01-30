namespace Avalon.Common.Interfaces
{ 
    /// <summary>
    /// Represents an ANSI color and mapping information to other formats.  Framework specific
    /// implementations should start from the base class that inherits this and then provide
    /// the actual Brush or Color for the platform selected (e.g. SolidColorBrush for WPF).
    /// </summary>
    public interface IAnsiColor
    {
        /// <summary>
        /// The color code as a player would use it on a game if this exists.
        /// </summary>
        string MudColorCode { get; }

        /// <summary>
        /// The friendly name of the color.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ToString() implementation which should be the actual ANSI value.
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}