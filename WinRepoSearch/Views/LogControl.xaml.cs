using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRepoSearch.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinRepoSearch.Views
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
