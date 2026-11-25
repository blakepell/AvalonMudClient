using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Avalon.Common;

/// <summary>
/// Finds the writable JavaScript property represented by a settings grid row.
/// </summary>
public sealed class ScriptPropertyConverter : IMultiValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is null || values[1] is not string name)
        {
            return null;
        }

        var property = TypeDescriptor.GetProperties(values[0])[name];
        return property is { IsReadOnly: false, PropertyType: var type } && type == typeof(string)
            && property.Attributes[typeof(ModelEditorAttribute)] is ModelEditorAttribute { TextEditor: TextEditorKind.Script }
            ? property
            : null;
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
