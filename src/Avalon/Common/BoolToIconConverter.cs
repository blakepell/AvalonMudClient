using System.Globalization;
using System.Windows.Data;

namespace Avalon.Common;

/// <summary>
/// Converts a boolean to a green check mark circle when true or a red x circle otherwise.
/// </summary>
public sealed class BoolToIconConverter : IValueConverter
{
    private const string Check = "pack://application:,,,/Assets/circle-check-24.png";
    private const string Cross = "pack://application:,,,/Assets/circle-x-24.png";

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Check : Cross;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
