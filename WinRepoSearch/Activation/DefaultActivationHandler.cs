using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;

using WinRepoSearch.Contracts.Services;
using WinRepoSearch.ViewModels;

namespace WinRepoSearch.Activation
{
    public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        private readonly INavigationService _navigationService;

        public DefaultActivationHandler(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            _navigationService.NavigateTo(
                typeof(SearchViewModel).FullName ?? "WinRepoSearch.ViewModels.SearchViewModel", 
                args.Arguments);
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            // None of the ActivationHandlers has handled the app activation
            return _navigationService.Frame.Content == null;
        }
    }
}
