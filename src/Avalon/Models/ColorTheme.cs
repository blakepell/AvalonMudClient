using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mosaic.UI.Wpf.Cache;
using Mosaic.UI.Wpf.Json;

namespace Avalon.Models
{
    /// <summary>
    /// Stores the brushes for the terminal palette and info bar.
    /// </summary>
    public partial class ColorTheme : ObservableObject
    {
        /// <summary>
        /// Gets or sets the background color for the main terminal.
        /// </summary>
        [Category("Terminal Colors")]
        [Description("The background color for the main terminal.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Background { get; set; } = Brushes.Black;

        /// <summary>
        /// Gets or sets the default foreground color for the main terminal.
        /// </summary>
        [Category("Terminal Colors")]
        [Description("The default foreground color for the main terminal.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Default { get; set; } = Brushes.LightGray;

        /// <summary>
        /// Gets or sets the foreground color set when a clear code is sent to the main terminal.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [Description("The foreground color set when a clear code is sent to the main terminal.")]
        [ObservableProperty]
        public partial SolidColorBrush Clear { get; set; } = Brushes.LightGray;

        /// <summary>
        /// Gets or sets the green terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Green { get; set; } = Brushes.Lime;

        /// <summary>
        /// Gets or sets the dark green terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Green")]
        [ObservableProperty]
        public partial SolidColorBrush DarkGreen { get; set; } = Brushes.Green;

        /// <summary>
        /// Gets or sets the white terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush White { get; set; } = Brushes.White;

        /// <summary>
        /// Gets or sets the light gray terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Light Gray")]
        [ObservableProperty]
        public partial SolidColorBrush LightGray { get; set; } = Brushes.LightGray;

        /// <summary>
        /// Gets or sets the dark gray terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Gray")]
        [ObservableProperty]
        public partial SolidColorBrush DarkGray { get; set; } = Brushes.DarkGray;

        /// <summary>
        /// Gets or sets the black terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Black { get; set; } = Brushes.DarkGray;

        /// <summary>
        /// Gets or sets the red terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Red { get; set; } = Brushes.Red;

        /// <summary>
        /// Gets or sets the dark red terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Red")]
        [ObservableProperty]
        public partial SolidColorBrush DarkRed { get; set; } = Brushes.DarkRed;

        /// <summary>
        /// Gets or sets the blue terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Blue { get; set; } = Brushes.Blue;

        /// <summary>
        /// Gets or sets the dark blue terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Blue")]
        [ObservableProperty]
        public partial SolidColorBrush DarkBlue { get; set; } = Brushes.DarkBlue;

        /// <summary>
        /// Gets or sets the yellow terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Yellow { get; set; } = Brushes.Yellow;

        /// <summary>
        /// Gets or sets the dark yellow terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Yellow")]
        [ObservableProperty]
        public partial SolidColorBrush DarkYellow { get; set; } = Brushes.Gold;

        /// <summary>
        /// Gets or sets the cyan terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Cyan { get; set; } = Brushes.Cyan;

        /// <summary>
        /// Gets or sets the dark cyan terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Cyan")]
        [ObservableProperty]
        public partial SolidColorBrush DarkCyan { get; set; } = Brushes.DarkCyan;

        /// <summary>
        /// Gets or sets the purple terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Purple { get; set; } = Brushes.Magenta;

        /// <summary>
        /// Gets or sets the dark purple terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Dark Purple")]
        [ObservableProperty]
        public partial SolidColorBrush DarkPurple { get; set; } = Brushes.DarkMagenta;

        /// <summary>
        /// Gets or sets the pink terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Pink { get; set; } = Brushes.HotPink;

        /// <summary>
        /// Gets or sets the brown terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Brown { get; set; } = Brushes.Brown;

        /// <summary>
        /// Gets or sets the magenta terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Magenta { get; set; } = Brushes.MediumPurple;

        /// <summary>
        /// Gets or sets the orange terminal color.
        /// </summary>
        [Category("Terminal Colors")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [ObservableProperty]
        public partial SolidColorBrush Orange { get; set; } = Brushes.Orange;

        /// <summary>
        /// Gets or sets the background color for the info bar.
        /// </summary>
        [Category("Info Bar")]
        [Description("The background color for the info bar.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Background")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarBackground { get; set; } = ColorPaletteCache.GetBrush("#171717");

        /// <summary>
        /// Gets or sets the foreground color for the info bar.
        /// </summary>
        [Category("Info Bar")]
        [Description("The foreground color for the info bar.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Foreground")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarForeground { get; set; } = ColorPaletteCache.GetBrush("#FFFFFF");

        /// <summary>
        /// Gets or sets the warning color for the info bar (when stats are low or the tick timer is close to 0).
        /// </summary>
        [Category("Info Bar")]
        [Description("The warning color for the info bar (when stats are low or the tick timer is close to 0).")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Warning")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarWarning { get; set; } = Brushes.Yellow;

        /// <summary>
        /// Gets or sets the danger color for the info bar (when stats are low or the tick timer is close to 0).
        /// </summary>
        [Category("Info Bar")]
        [Description("The danger color for the info bar (when stats are low or the tick timer is close to 0).")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Danger")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarDanger { get; set; } = Brushes.Red;

        /// <summary>
        /// Gets or sets the dark text which is used for a label of a value like the room name.
        /// </summary>
        [Category("Info Bar")]
        [Description("The dark text which is used for a label of a value like the room name.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Text Dark")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarTextDark { get; set; } = Brushes.DarkCyan;

        /// <summary>
        /// Gets or sets the light text which is used as the value of something like a room name.
        /// </summary>
        [Category("Info Bar")]
        [Description("The light text which is used as the value of something like a room name.")]
        [JsonConverter(typeof(SolidColorBrushSerializer))]
        [DisplayName("Text Light")]
        [ObservableProperty]
        public partial SolidColorBrush InfoBarTextLight { get; set; } = Brushes.Cyan;
    }
}
