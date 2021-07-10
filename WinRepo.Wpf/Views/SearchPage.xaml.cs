using System.Windows.Controls;

using CommunityToolkit.Mvvm.DependencyInjection;

using WinRepoSearch.Core.ViewModels;

namespace WinRepo.Wpf.Views
{
    public sealed partial class SearchPage : Page
    {
        private SearchViewModel viewModel;

        public SearchViewModel ViewModel
        {
            get => viewModel;
            set { 
                viewModel = value;
            }
        }

        private void ViewModel_UpdatedStatus(string obj)
        {
            Status.Content = obj;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SearchPage()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ViewModel = Ioc.Default.GetService<SearchViewModel>();
            InitializeComponent();
            this.DataContext = this;
        }

        private void SearchTermBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var tb = sender as TextBox;
            var btn = sender as Button;

            if (tb is not null || btn is not null)
            {
                ViewModel.SearchTerm = SearchTermBox.Text;
                ViewModel.PerformSearch.NotifyCanExecuteChanged();

                switch (e.Key)
                {
                    case System.Windows.Input.Key.Enter:
                        SearchButton.Command.Execute(ViewModel);
                        break;
                }
            }
        }

        //private void OnViewStateChanged(object sender, ListDetailsViewState e)
        //{
        //    if (e == ListDetailsViewState.Both)
        //    {
        //        ViewModel.EnsureItemSelected();
        //    }
        //}

        //private void TextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        //{
        //    var tb = sender as TextBox;
        //    var btn = sender as Button;

        //    if (tb is not null || btn is not null)
        //    {
        //        ViewModel.SearchTerm = SearchTermBox.Text;
        //        ViewModel.PerformSearch.NotifyCanExecuteChanged();

        //        switch (e.Key)
        //        {
        //            case Windows.System.VirtualKey.Enter:
        //                SearchButton.Command.Execute(ViewModel);
        //                break;
        //        }
        //    }
        //}
    }
}
