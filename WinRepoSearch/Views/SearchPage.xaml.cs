using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;

using Microsoft.UI.Xaml.Controls;

using WinRepoSearch.Core.ViewModels;

namespace WinRepoSearch.Views
{
    public sealed partial class SearchPage : Page
    {
        public SearchViewModel ViewModel { get; }

        public SearchPage()
        {
            ViewModel = Ioc.Default.GetService<SearchViewModel>();
            InitializeComponent();
        }

        private void OnViewStateChanged(object sender, ListDetailsViewState e)
        {
            if (e == ListDetailsViewState.Both)
            {
                ViewModel.EnsureItemSelected();
            }
        }

        private void TextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var tb = sender as TextBox;
            var btn = sender as Button;

            if (tb is not null || btn is not null)
            {
                ViewModel.SearchTerm = SearchTermBox.Text;
                ViewModel.PerformSearch.NotifyCanExecuteChanged();

                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Enter:
                        SearchButton.Command.Execute(ViewModel);
                        break;
                }
            }
        }
    }
}
