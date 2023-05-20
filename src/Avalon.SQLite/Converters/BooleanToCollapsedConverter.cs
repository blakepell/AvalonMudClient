/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Sqlite.Converters
{
    /// <summary>
    /// Converts a bool to a Visibility.  True returns Visible and False return Collapsed.  This is the
    /// opposite of <see cref="InvertingBooleanToCollapsedConverter"/>.
    /// </summary>
    public class BooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = bool.Parse(value.ToString());
            return vis ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            return vis == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a bool to a Visibility.  True returns Collapsed and False returns Visible.  This is the
    /// opposite of <see cref="BooleanToCollapsedConverter"/>.
    /// </summary>
    public class InvertingBooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = bool.Parse(value.ToString());
            return vis ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }

            Visibility vis = (Visibility)value;
            return vis == Visibility.Collapsed;
        }
    }
}