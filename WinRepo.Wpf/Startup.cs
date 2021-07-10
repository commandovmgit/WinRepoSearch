using Microsoft.Extensions.DependencyInjection;

using System;

using WinRepo.Wpf.Views;

using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Services;
using WinRepoSearch.Core.ViewModels;
using WinRepoSearch.ViewModels;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace WinRepo.Wpf
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
            // Core Services
            services.AddSingleton<SearchService>();

            // Views and ViewModels
            //services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            services.AddSingleton<SearchViewModel>();
            services.AddTransient<SearchPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            services.AddSingleton<IStartup, Startup>();
        }
    }
}
