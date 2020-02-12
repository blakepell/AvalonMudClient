using System.ComponentModel;
using Newtonsoft.Json;

namespace Avalon.Common.Settings
{
    /// <summary>
    /// The global settings for the Avalon Mud Client.  These are NOT profile specific like many of
    /// the other settings.  We're going to try to keep things simple and have no overlap or cascading
    /// properties.
    /// </summary>
    public class AvalonSettings
    {
        /// <summary>
        /// The overridden save directory if one exists.
        /// </summary>
        [CategoryAttribute("FileSystem")]
        [DescriptionAttribute("The default directory save profiles and other game related file data in.")]
        [Browsable(true)]
        public string SaveDirectory { get; set; } = "";

        /// <summary>
        /// The path to the last loaded profile file.
        /// </summary>
        [CategoryAttribute("FileSystem")]
        [DescriptionAttribute("The path to the last loaded profile file.  This is the profile that will be loaded by default on the clients next startup.")]
        [Browsable(true)]
        public string LastLoadedProfilePath { get; set; } = "";

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The position of the mud client window when it starts up.")]
        [Browsable(true)]
        public WindowStartupPosition WindowStartupPosition { get; set; } = WindowStartupPosition.OperatingSystemDefault;


        [Browsable(true)]
        public WindowPosition LastWindowPosition { get; set; } = new WindowPosition();
        
    }
}
