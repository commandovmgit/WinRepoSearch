using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.DependencyInjection;

using WinRepoSearch.Core.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinRepo.Wpf.Views
{
    public sealed partial class LogControl : UserControl
    {
        public SearchViewModel ViewModel { 
            get => (SearchViewModel)GetValue(ViewModelProperty); 
            set => SetValue(ViewModelProperty, value); 
        }

        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(SearchViewModel), typeof(LogControl), new PropertyMetadata(default));

        public LogControl()
        {
            ViewModel = Ioc.Default.GetService<SearchViewModel>()!;
            this.InitializeComponent();
        }
    }
}
