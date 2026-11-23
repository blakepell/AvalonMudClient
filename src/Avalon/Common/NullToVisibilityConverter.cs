using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Avalon.Common;

/// <summary>
/// Shows an element when the bound value is not null and collapses it when it is.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
