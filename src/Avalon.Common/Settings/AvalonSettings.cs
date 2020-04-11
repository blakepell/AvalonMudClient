﻿using System.Collections.Generic;
using System.ComponentModel;
using Avalon.Common.Models;
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

        /// <summary>
        /// Where the window should try to position itself when the game starts up.
        /// </summary>
        [CategoryAttribute("UI")]
        [DescriptionAttribute("The position of the mud client window when it starts up.")]
        [Browsable(true)]
        public WindowStartupPosition WindowStartupPosition { get; set; } = WindowStartupPosition.OperatingSystemDefault;

        /// <summary>
        /// The actual last position of the window when the settings are saved that will be used to reposition
        /// the window on startup.
        /// </summary>
        [Browsable(true)]
        public WindowPosition LastWindowPosition { get; set; } = new WindowPosition();

        /// <summary>
        /// Serialized state of the grid rows.
        /// </summary>
        [Browsable(false)]
        public List<GridLengthState> GameGridState = new List<GridLengthState>();

        /// <summary>
        /// Whether the back buffer terminal should be populated when data arrives.  This also determines whether PageUp() and PageDown() triggers
        /// it's visibility.
        /// </summary>
        [CategoryAttribute("Performance")]
        [DescriptionAttribute("Whether the back buffer is enabled allowing for scrolling up through history simultaneously while game data is still arriving.")]
        [Browsable(true)]
        public bool BackBufferEnabled { get; set; } = true;

        [CategoryAttribute("Security")]
        [DescriptionAttribute("Whether features that might implicate security are turned on.  These generally allow an expert to run commands a normal player shouldn't run.")]
        [Browsable(true)]
        public bool DeveloperMode { get; set; } = false;


        [CategoryAttribute("UI")]
        [DescriptionAttribute("The font size that should be used in the terminal panels.")]
        [Browsable(true)]
        public int TerminalFontSize { get; set; } = 13;

        /// <summary>
        /// Supported fonts.
        /// </summary>
        public enum TerminalFonts
        {
            Consolas,
            CourierNew
        }

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether features that might implicate security are turned on.  These generally allow an expert to run commands a normal player shouldn't run.")]
        [Browsable(true)]
        public TerminalFonts TerminalFont { get; set; } = TerminalFonts.Consolas;

        [CategoryAttribute("Updates")]
        [DescriptionAttribute("The URL that the mud client to reference to determine if there is an update.")]
        [Browsable(true)]
        public string ReleaseUrl { get; } = "https://api.github.com/repos/blakepell/AvalonMudClient/releases/latest";

    }
}
