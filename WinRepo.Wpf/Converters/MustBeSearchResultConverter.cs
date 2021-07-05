using System;
using System.Globalization;
using System.Windows.Data;

using WinRepoSearch.Core.Models;

namespace WinRepo.Wpf.Converters
{
#nullable disable
    public class MustBeSearchResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as SearchResult;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
#nullable restore
}