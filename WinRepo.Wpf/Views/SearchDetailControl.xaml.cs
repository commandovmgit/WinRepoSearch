using System.Windows;

using WinRepoSearch.Core.Models;
using WinRepoSearch.ViewModels;

namespace WinRepo.Wpf.Views
{
    public sealed partial class SearchDetailControl
    {
        public SearchResult ListDetailsMenuItem
        {
            get => (SearchResult)GetValue(ListDetailsMenuItemProperty);
            set => SetValue(ListDetailsMenuItemProperty, value);
        }
        public SearchViewModel SearchViewModel {
            get => (SearchViewModel)GetValue(SearchViewModelProperty);
            set => SetValue(SearchViewModelProperty, value);
        }

        public static readonly DependencyProperty ListDetailsMenuItemProperty =
            DependencyProperty.Register(
                nameof(ListDetailsMenuItem),
                typeof(SearchResult),
                typeof(SearchDetailControl),
                new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public static readonly DependencyProperty SearchViewModelProperty =
            DependencyProperty.Register(
                nameof(SearchViewModel),
                typeof(SearchViewModel),
                typeof(SearchDetailControl),
                new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public SearchDetailControl()
        {
            InitializeComponent();
        }

        public SearchDetailControl(SearchViewModel viewModel)
        {
            SearchViewModel = viewModel;
            ListDetailsMenuItem = viewModel.Selected!;

            ListDetailsMenuItem.PropertyChanged += (o, e) =>
            {
                switch(e.PropertyName)
                {
                    case nameof(SearchResult.Markdown):
                        break;
                }
            };

            InitializeComponent();
        }

        private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is SearchDetailControl control && e.Property.Name == nameof(ListDetailsMenuItem))
            {
                if (e.NewValue is not null && e.NewValue.GetType() != typeof(SearchResult))
                {
                    control?.SetValue(e.Property, e.OldValue);
                }
            }
            else
            {
                return;
            }


        }

        //private void MarkdownTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        //{
        //    if(e.Link.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        OpenUrl(e.Link);
        //    }

        //    void OpenUrl(string url)
        //    {
        //        try
        //        {
        //            Process.Start(url);
        //        }
        //        catch
        //        {
        //            // hack because of this: https://github.com/dotnet/corefx/issues/10361
        //            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //            {
        //                url = url.Replace("&", "^&");
        //                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        //            }
        //            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //            {
        //                Process.Start("xdg-open", url);
        //            }
        //            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //            {
        //                Process.Start("open", url);
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}
    }
}
