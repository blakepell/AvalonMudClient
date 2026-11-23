using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Mosaic.UI.Wpf.Cache;

namespace Avalon.Common;

/// <summary>
/// Adapts a solid color brush to a color editor's two-way binding.
/// </summary>
public sealed class SolidColorBrushColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is SolidColorBrush brush ? brush.Color : DependencyProperty.UnsetValue;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
        {
            return Binding.DoNothing;
        }

        var brush = ColorPaletteCache.GetBrush(color);
        brush.Freeze();
        return brush;
    }
}
