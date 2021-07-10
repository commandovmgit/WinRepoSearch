
using CommunityToolkit.Mvvm.ComponentModel;

using WinRepoSearch.Core.ViewModels;

namespace WinRepoSearch.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        private bool _isBackEnabled;
        private object? _selected;

        public SearchViewModel SearchViewModel { get; }

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { SetProperty(ref _isBackEnabled, value); }
        }

        public object? Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ShellViewModel(
            SearchViewModel searchViewModel)
        {
            SearchViewModel = searchViewModel;
        }

        //private void OnNavigated(object sender, NavigationEventArgs e)
        //{
        //    IsBackEnabled = NavigationService.CanGoBack;
        //    if (e.SourcePageType == typeof(SettingsPage))
        //    {
        //        Selected = NavigationViewService.SettingsItem;
        //        return;
        //    }

        //    var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        //    if (selectedItem != null)
        //    {
        //        Selected = selectedItem;
        //    }
        //}
    }
}
