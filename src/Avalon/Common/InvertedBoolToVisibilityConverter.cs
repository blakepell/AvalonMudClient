using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Avalon.Common;

/// <summary>
/// Collapses an element when the bound value is true and shows it otherwise, including when the value
/// is null or not a boolean.
/// </summary>
public sealed class InvertedBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
