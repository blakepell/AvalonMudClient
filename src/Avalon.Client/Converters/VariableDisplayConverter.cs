using System;
using System.Globalization;
using System.Windows.Data;

namespace Avalon.Converters
{
    public class VariableDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "n/a";
            }

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return "n/a";
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
