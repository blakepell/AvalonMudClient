using Avalon.Colors;
using Avalon.Common.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Avalon.Converters
{
    /// <summary>
    /// Converts a string color to a SolidColorBrush as represented by our supported AnsiColors.
    /// </summary>
    public class AnsiColorToBrushConverter : IValueConverter
    {
        //public static SolidColorBrush ForegroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#76F0FF"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (!ForegroundColor.IsFrozen && ForegroundColor.CanFreeze)
            //{
            //    ForegroundColor.Freeze();
            //}

            var item = value as Variable;

            // If it's null or there's no value then it's the default color.
            if (item == null || string.IsNullOrWhiteSpace(item.Value) || item.Value.Equals("n/a", StringComparison.Ordinal))
            {
                return Brushes.Cyan;
            }

            // Try to find the color.
            var color = Colorizer.ColorMapByName(item.ForegroundColor)?.Brush;

            if (color == null)
            {
                color = Brushes.Cyan;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}