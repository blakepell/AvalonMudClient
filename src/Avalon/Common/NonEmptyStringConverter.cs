using System.Globalization;
using System.Windows.Data;

namespace Avalon.Common;

/// <summary>
/// Passes strings through unchanged but never writes a blank value back to the source, so an editor
/// that clears its selection while the user is typing doesn't wipe out the setting.
/// </summary>
public sealed class NonEmptyStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && !string.IsNullOrWhiteSpace(text) ? text : Binding.DoNothing;
}
