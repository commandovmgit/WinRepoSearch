using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using WinRepoSearch.Activation;
using WinRepoSearch.Contracts.Services;
using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Services;
using WinRepoSearch.Helpers;
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

    public partial class App : Application
    {
        public static IHost? ServiceHost { get; private set; }
        public static Window MainWindow { get; set; } = new Window() { Title = "AppDisplayName".GetLocalized() };

        public static IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder()
                .ConfigureContainer<IServiceCollection>(collection =>
                {
                    Startup.ConfigureServices(collection);
                    Ioc.Default.ConfigureServices(collection.BuildServiceProvider());
                    //Logger.LogDebug("ConfiguredServices.");
                });

        public App()
        {
            //AppCenter.Start("1f00432d-26a9-4bd8-86f3-552be7829da0",
            //       typeof(Analytics), typeof(Crashes));


            InitializeComponent();
            UnhandledException += App_UnhandledException;

            ServiceHost = CreateHostBuilder().Build();

            ServiceHost.Start();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>()!;
            await activationService.ActivateAsync(args);
        }


    }
}
