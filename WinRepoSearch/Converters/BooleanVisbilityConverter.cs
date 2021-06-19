using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using WinRepoSearch.Core.Models;

namespace WinRepoSearch.Converters
{
    public class BooleanVisbilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool bValue 
                ? bValue 
                    ? Visibility.Visible 
                    : Visibility.Collapsed 
                : (object)Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

#nullable disable
    public class MustBeSearchResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as SearchResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MaxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.Equals(double.NaN) || (double)value < 1.0) return 1.0;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
#nullable restore
}