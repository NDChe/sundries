using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SteganographyNetCore
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if ((bool)value)
                return Visibility.Visible;

            if (!(parameter is bool))
                return Visibility.Collapsed;

            var isHidden = (bool)parameter;
            if (isHidden)
                return Visibility.Hidden;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
