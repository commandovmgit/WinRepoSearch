using System.Windows;
using System.Windows.Controls;

namespace WinRepo.Wpf.Controls
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
