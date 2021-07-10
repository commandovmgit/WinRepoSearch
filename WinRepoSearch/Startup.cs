using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using System;

using WinRepoSearch.Activation;
using WinRepoSearch.Contracts.Services;
using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Services;
using WinRepoSearch.Core.ViewModels;
using WinRepoSearch.Services;
using WinRepoSearch.ViewModels;
using WinRepoSearch.Views;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace WinRepoSearch
{
    public class Startup : IStartup
    {
        public IServiceProvider? ServiceProvider { get; set; }

        public Startup() { }

        public Startup(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<SearchService>();

            // Views and ViewModels
            services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            services.AddSingleton<SearchViewModel>();
            services.AddTransient<SearchPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            services.AddSingleton<IStartup, Startup>();
        }
    }
}
