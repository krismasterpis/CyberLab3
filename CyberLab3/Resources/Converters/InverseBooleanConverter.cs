using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CyberLab3.Resources.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return true; // domyślnie włączone, jeśli wartość nie jest bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }
}