/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Collections.Generic;
using System.ComponentModel;
using Avalon.Common.Models;

namespace Avalon.Common.Settings
{
    /// <summary>
    /// The global settings for the Avalon Mud Client.  These are NOT profile specific like many of
    /// the other settings.  We're going to try to keep things simple and have no overlap or cascading
    /// properties.
    /// </summary>
    public class AvalonSettings : INotifyPropertyChanged
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
        public WindowPosition LastWindowPosition { get; set; } = new();

        /// <summary>
        /// Serialized state of the grid rows.
        /// </summary>
        [Browsable(false)]
        public List<GridLengthState> GameGridState = new();

        /// <summary>
        /// Whether the back buffer terminal should be populated when data arrives.  This also determines whether PageUp() and PageDown() triggers
        /// it's visibility.
        /// </summary>
        [CategoryAttribute("Performance")]
        [DescriptionAttribute("Whether the back buffer is enabled allowing for scrolling up through history simultaneously while game data is still arriving.")]
        [Browsable(true)]
        public bool BackBufferEnabled { get; set; } = true;

        /// <summary>
        /// The ability to disable hardware acceleration (not recommended).
        /// </summary>
        /// <remarks>
        /// This is not recommended but is provided in case the hardware acceleration is causing the app to crash
        /// or not render correctly due to a video card driver.  This is an edge case but providing this setting
        /// will allow for people to disable hardware acceleration and continue using the app.
        /// </remarks>
        [CategoryAttribute("Performance")]
        [DescriptionAttribute("Whether disable hardware acceleration or not.  This should always be true unless the UI is crashing due to a video card issue with this app.  Hardware acceleration will almost always be faster than the alternative.")]
        [Browsable(true)]
        public bool DisableHardwareAcceleration { get; set; } = false;

        private bool _developerMode = false;

        [CategoryAttribute("Security")]
        [DescriptionAttribute("Whether features that might implicate security are turned on.  These generally allow an expert to run commands a normal player shouldn't run.")]
        [Browsable(true)]
        public bool DeveloperMode
        {
            get => _developerMode;

            set
            {
                if (value != _developerMode)
                {
                    _developerMode = value;
                    OnPropertyChanged(nameof(this.DeveloperMode));
                }
            }
        }

        [CategoryAttribute("Security")]
        [DescriptionAttribute("Whether advanced debugging messages will be echoed to the console.")]
        [Browsable(true)]
        public bool Debug { get; set; } = false;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether the main window should have a border.")]
        [Browsable(true)]
        public bool ShowMainWindowBorder { get; set; } = false;

        private int _terminalFontSize = 13;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The font size that should be used in the terminal panels.")]
        [Browsable(true)]
        public int TerminalFontSize
        {
            get => _terminalFontSize;
            set
            {
                _terminalFontSize = value;
                OnPropertyChanged(nameof(this.TerminalFontSize));
            }
        }

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

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether the input box should clear after you enter a command.")]
        [Browsable(true)]
        public bool InputBoxClearAfterCommand { get; set; } = false;

        /// <summary>
        /// The supported time stamp formats.
        /// </summary>
        public enum TimestampFormats
        {
            HoursMinutes = 0,
            HoursMinutesSeconds = 1,
            TwentyFourHour = 2,
            OSDefault = 3
        }

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The format of the timestamp when a line is written to a communication panel.")]
        [Browsable(true)]
        public TimestampFormats TimestampFormat { get; set; } = TimestampFormats.HoursMinutesSeconds;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("If escape is pressed all terminal windows will scroll to the bottom.")]
        [Browsable(true)]
        public bool EscapeScrollsAllTerminalsToBottom { get; set; } = true;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("If the mouse wheel is used in the game terminal it will open the back buffer and re-route the scroll there.")]
        [Browsable(true)]
        public bool MouseWheelScrollReroutesToBackBuffer { get; set; } = true;


        private bool _showLineNumbersInGameTerminal = false;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether or not line numbers should be shown in the game terminal.")]
        [Browsable(true)]
        public bool ShowLineNumbersInGameTerminal
        {
            get => _showLineNumbersInGameTerminal;
            set
            {
                _showLineNumbersInGameTerminal = value;
                OnPropertyChanged(nameof(this.ShowLineNumbersInGameTerminal));
            }
        }

        private bool _wordWrapTerminals = true;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Word wrap the terminals.")]
        [Browsable(true)]
        public bool WordWrapTerminals
        {
            get => _wordWrapTerminals;
            set
            {
                _wordWrapTerminals = value;
                OnPropertyChanged(nameof(this.WordWrapTerminals));
            }
        }

        private bool _variableRepeaterVisible = true;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("If the variables repeater is visible on the main game tab.  The variable repeater will show variables that are defined as visible in the order their defined on in the variables tab.")]
        [Browsable(true)]
        public bool VariableRepeaterVisible
        {
            get => _variableRepeaterVisible;
            set
            {
                if (value != _variableRepeaterVisible)
                {
                    _variableRepeaterVisible = value;
                    OnPropertyChanged(nameof(this.VariableRepeaterVisible));
                }
            }
        }

        private string _customTab1Label = "Untitled 1";

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The header name of custom tab 1.")]
        [Browsable(true)]
        public string CustomTab1Label
        {
            get => _customTab1Label;
            set
            {
                if (value != _customTab1Label)
                {
                    _customTab1Label = value;
                    OnPropertyChanged(nameof(this.CustomTab1Label));
                }
            }
        }

        private bool _customTab1Visible = true;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether custom tab 1 is visible or not.")]
        [Browsable(true)]
        public bool CustomTab1Visible
        {
            get => _customTab1Visible;
            set
            {
                if (value != _customTab1Visible)
                {
                    _customTab1Visible = value;
                    OnPropertyChanged(nameof(this.CustomTab1Visible));
                }
            }
        }

        private string _customTab2Label = "Untitled 2";

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The header name of custom tab 2.")]
        [Browsable(true)]
        public string CustomTab2Label
        {
            get => _customTab2Label;
            set
            {
                if (value != _customTab2Label)
                {
                    _customTab2Label = value;
                    OnPropertyChanged(nameof(this.CustomTab2Label));
                }
            }
        }

        private bool _customTab2Visible = true;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether custom tab 2 is visible or not.")]
        [Browsable(true)]
        public bool CustomTab2Visible
        {
            get => _customTab2Visible;
            set
            {
                if (value != _customTab2Visible)
                {
                    _customTab2Visible = value;
                    OnPropertyChanged(nameof(this.CustomTab2Visible));
                }
            }
        }

        private string _customTab3Label = "Untitled 3";

        [CategoryAttribute("UI")]
        [DescriptionAttribute("The header name of custom tab 3.")]
        [Browsable(true)]
        public string CustomTab3Label
        {
            get => _customTab3Label;
            set
            {
                if (value != _customTab3Label)
                {
                    _customTab3Label = value;
                    OnPropertyChanged(nameof(this.CustomTab3Label));
                }
            }
        }

        private bool _customTab3Visible = false;

        [CategoryAttribute("UI")]
        [DescriptionAttribute("Whether custom tab 3 is visible or not.")]
        [Browsable(true)]
        public bool CustomTab3Visible
        {
            get => _customTab3Visible;
            set
            {
                if (value != _customTab3Visible)
                {
                    _customTab3Visible = value;
                    OnPropertyChanged(nameof(this.CustomTab3Visible));
                }
            }
        }

        [CategoryAttribute("Security")]
        [DescriptionAttribute("The ability to start programs on the computer via #shell or lua:shell.")]
        [Browsable(true)]
        public bool AllowShell { get; set; } = false;

        [CategoryAttribute("Updates")]
        [DescriptionAttribute("The URL that the mud client to reference to determine if there is an update.")]
        [Browsable(true)]
        public string ReleaseUrl { get; } = "https://api.github.com/repos/blakepell/AvalonMudClient/releases/latest";

        [CategoryAttribute("Updates")]
        [DescriptionAttribute("The URL to the Package Manager API.  Although this is pre-filled by default it can be changed by the user to point to any package manager they desire that implements the same API calls.")]
        [Browsable(true)]
        public string PackageManagerApiUrl { get; set; } = "https://avalon-mud-client.azurewebsites.net/api/package";

        [CategoryAttribute("Error Handling and Reporting")]
        [DescriptionAttribute("Whether or not global exceptions should be handled.  Note: This requires a reboot of the mud client to take effect.")]
        [Browsable(true)]
        public bool GlobalExceptionHandlingEnabled { get; set; } = false;


        [CategoryAttribute("Lua")]
        [DescriptionAttribute("Whether the Lua editor should attempt to validate Lua for syntax errors on save and report those to the user.")]
        [Browsable(true)]
        public bool ValidateLua { get; set; } = true;

        [CategoryAttribute("Database")]
        [DescriptionAttribute("The number of seconds between each database batch write.")]
        [Browsable(true)]
        public int DatabaseWriteInterval { get; set; } = 10;

        [CategoryAttribute("Input")]
        [DescriptionAttribute("The character used to split commands.")]
        [Browsable(true)]
        public char CommandSplitCharacter { get; set; } = ';';

        [CategoryAttribute("Input")]
        [DescriptionAttribute("If the shift+tab hot key is enabled to auto complete words from past input.")]
        [Browsable(true)]
        public bool AutoCompleteWord { get; set; } = false;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
