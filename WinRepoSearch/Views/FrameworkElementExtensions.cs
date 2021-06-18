using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinRepoSearch.Views
{
    public class UserControlBase : UserControl
    {
        public Visibility GetVisibility(object? value)
        {
            if (value is null) return Visibility.Collapsed;

            if (value is string strValue)
            {
                return string.IsNullOrWhiteSpace(strValue)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            return Visibility.Visible;
        }
    }
}
