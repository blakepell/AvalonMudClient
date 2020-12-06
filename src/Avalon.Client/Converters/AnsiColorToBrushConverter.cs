using Avalon.Colors;
using Avalon.Common.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Avalon.Converters
{
    /// <summary>
    /// Converts a string color to a SolidColorBrush as represented by our supported AnsiColors.
    /// </summary>
    public class AnsiColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as Variable;

            // If it's null or there's no value then it's the default color.
            if (item == null || string.IsNullOrWhiteSpace(item.Value) || item.Value.Equals("n/a", StringComparison.Ordinal))
            {
                return Colorizer.ColorMapByName("Cyan").Brush;
            }

            // Try to find the color.
            var color = Colorizer.ColorMapByName(item.ForegroundColor)?.Brush;

            if (color == null)
            {
                color = Colorizer.ColorMapByName("Cyan").Brush;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
