using System;
using System.Globalization;
using System.Windows.Data;

namespace WinRepo.Wpf.Converters
{
    public class MaxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(double.NaN) || (double)value < 1.0) return 1.0;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
#nullable restore
}