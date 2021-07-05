using System.Windows;
using System.Windows.Navigation;

using CommunityToolkit.Mvvm.DependencyInjection;

using WinRepo.Wpf.Views;

namespace WinRepo.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            shellFrame.Navigating += ShellFrame_Navigating;
            shellFrame.NavigationStopped += ShellFrame_NavigationStopped;
            shellFrame.Navigated += ShellFrame_NavigationStopped;

            shellFrame.Navigate(Ioc.Default.GetService<SearchPage>());
        }

        private void ShellFrame_NavigationStopped(object sender, NavigationEventArgs e)
        {
            Loader.Visibility = Visibility.Collapsed;
        }

        private void ShellFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Loader.Visibility = Visibility.Visible;
        }
    }
}
