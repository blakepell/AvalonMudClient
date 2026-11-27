using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Argus.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf;
using Mosaic.UI.Wpf.Common;
using Mosaic.UI.Wpf.Controls;
using Mosaic.UI.Wpf.Interfaces;
using Mosaic.UI.Wpf.Json;
using System.Windows.Media;

namespace Avalon.Models
{
    /// <summary>
    /// AppSettings that are persisted to storage and reloaded on application launch.
    /// </summary>
    public partial class AppSettings : ObservableObject, IAppSettings
    {
        /// <summary>
        /// The application data folder which will be populated by this program when it loads.
        /// </summary>
        [property: Category("File System")]
        [property: DisplayName("Application Data Folder")]
        [property: Description("The application data folder.")]
        [property: ReadOnly(true)]
        [property: Browsable(true)]
        [ObservableProperty]
        private string? _applicationDataFolder;

        [property: Category("File System")]
        [property: DisplayName("Client Settings")]
        [property: Description("Settings specific to this workstation.")]
        [property: ReadOnly(false)]
        [property: Browsable(true)]
        [JsonIgnore]
        [ObservableProperty]
        private LocalSettings? _clientSettings = new();

        /// <summary>
        /// The applications current theme.
        /// </summary>
        [property: DisplayName("Theme")]
        [property: Category("UI")]
        [property: Browsable(false)]
        [ObservableProperty]
        private MosaicThemeMode _theme = MosaicThemeMode.Black;

        /// <summary>
        /// Gets or sets the ribbon layout.
        /// </summary>
        [ObservableProperty]
        public partial RibbonLayoutMode RibbonLayout { get; set; } = RibbonLayoutMode.Classic;

        /// <summary>
        /// Gets or sets the saved terminal and info bar brushes.
        /// </summary>
        [property: Browsable(false)]
        [ObservableProperty]
        public partial ColorTheme ColorTheme { get; set; } = new();

        /// <summary>
        /// Gets or sets a value that indicates whether sending a game command clears the editor.
        /// </summary>
        [property: Category("Terminal")]
        [property: DisplayName("Clear Input After Command")]
        [ObservableProperty]
        public partial bool InputBoxClearAfterCommand { get; set; }

        /// <summary>
        /// Gets or sets the font family used by the game terminals.
        /// </summary>
        [property: Category("Terminal")]
        [property: DisplayName("Terminal Font")]
        [ObservableProperty]
        public partial string TerminalFont { get; set; } = "Consolas";

        /// <summary>
        /// Gets or sets the font size used by the game terminals.
        /// </summary>
        [property: Category("Terminal")]
        [property: DisplayName("Terminal Font Size")]
        [property: Description("The font size used by the game terminals (8 to 24).")]
        [ObservableProperty]
        public partial double TerminalFontSize { get; set; } = 14.0;

        /// <summary>
        /// Gets or sets a value that indicates whether line numbers are shown in the game terminal.
        /// </summary>
        [property: Category("Terminal")]
        [property: DisplayName("Show Line Numbers In Game Terminal")]
        [ObservableProperty]
        public partial bool ShowLineNumbersInGameTerminal { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the game terminals wrap long lines.
        /// </summary>
        [property: Category("Terminal")]
        [property: DisplayName("Word Wrap Terminals")]
        [ObservableProperty]
        public partial bool WordWrapTerminals { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether the command box uses a block caret.
        /// </summary>
        [property: Category("Command Box")]
        [property: DisplayName("Use Block Caret")]
        [property: Description("If a block caret should be used in the command box.")]
        [ObservableProperty]
        public partial bool UseBlockCaret { get; set; } = true;

        /// <summary>
        /// Gets or sets the background color of the command box.
        /// </summary>
        [property: Category("Command Box")]
        [property: DisplayName("Background Color")]
        [property: Description("The background color of the terminal's command box.")]
        [property: JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush CommandBoxBackgroundColor { get; set; } = Brushes.Black;

        /// <summary>
        /// Gets or sets the foreground color of the command box.
        /// </summary>
        [property: Category("Command Box")]
        [property: DisplayName("Foreground Color")]
        [property: Description("The foreground color of the terminal's command box.")]
        [property: JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush CommandBoxForegroundColor { get; set; } = Brushes.White;

        /// <summary>
        /// Default font size for UI elements.
        /// </summary>
        [property: Category("UI")]
        [property: DisplayName("Font Size")]
        [property: Description("The default font size for UI elements.")]
        [ObservableProperty]
        [property: Browsable(true)]
        private double _fontSize = 12.0;

        /// <summary>
        /// The view state information for a given window.
        /// </summary>
        [property: Category("UI")]
        [property: DisplayName("Window View States")]
        [property: Description("The view state information for a given window.")]
        [property: Browsable(true)]
        [ObservableProperty]
        private ObservableCollection<WindowViewState> _windowViewStates = new();

        /// <summary>
        /// Gets or sets the notes kept in the Notes window.
        /// </summary>
        [property: Browsable(false)]
        [ObservableProperty]
        public partial string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the tool windows that were open when the client closed, or null for the default layout.
        /// </summary>
        [property: Browsable(false)]
        [ObservableProperty]
        public partial List<string>? OpenToolWindows { get; set; }

        /// <summary>
        /// Gets or sets the configured resource labels for each named vitals panel.
        /// </summary>
        [Browsable(false)]
        public Dictionary<string, List<string>>? VitalsPanelLabels { get; set; }

        /// <summary>
        /// Gets or sets whether the ribbon is collapsed.
        /// </summary>
        [property: Category("UI")]
        [property: DisplayName("Ribbon Minimized")]
        [ObservableProperty]
        public partial bool IsRibbonMinimized { get; set; }

        /// <summary>
        /// Game listings.
        /// </summary>
        [property: Browsable(false)]
        [ObservableProperty]
        public partial FullyObservableCollection<Game> Games { get; set; } = new();
    }
}
