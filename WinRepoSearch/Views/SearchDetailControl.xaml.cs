using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinRepoSearch.Core.Models;

namespace WinRepoSearch.Views
{
    public sealed partial class SearchDetailControl : UserControlBase
    {
        public SearchResult ListDetailsMenuItem
        {
            get => (SearchResult)GetValue(ListDetailsMenuItemProperty);
            set => SetValue(ListDetailsMenuItemProperty, value);
        }

        public static readonly DependencyProperty ListDetailsMenuItemProperty =
            DependencyProperty.Register(
                "ListDetailsMenuItem", 
                typeof(SearchResult), 
                typeof(SearchDetailControl), 
                new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public SearchDetailControl()
        {
            InitializeComponent();
        }

        private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchDetailControl)d;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }

        private void MarkdownTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            if(e.Link.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase))
            {
                OpenUrl(e.Link);
            }

            void OpenUrl(string url)
            {
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", url);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
