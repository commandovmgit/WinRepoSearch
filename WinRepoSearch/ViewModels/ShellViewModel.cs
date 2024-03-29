﻿using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

using WinRepoSearch.Contracts.Services;
using WinRepoSearch.Core.ViewModels;
using WinRepoSearch.Views;

namespace WinRepoSearch.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        private bool _isBackEnabled;
        private object? _selected;

        public INavigationService NavigationService { get; }

        public INavigationViewService NavigationViewService { get; }
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
            INavigationService navigationService, 
            INavigationViewService navigationViewService,
            SearchViewModel searchViewModel)
        {
            NavigationService = navigationService;
            NavigationService.Navigated += OnNavigated;
            NavigationViewService = navigationViewService;
            SearchViewModel = searchViewModel;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = NavigationViewService.SettingsItem;
                return;
            }

            var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
            if (selectedItem != null)
            {
                Selected = selectedItem;
            }
        }
    }
}
