using System;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WinRepo.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static ILogger<App> _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static IHost? ServiceHost { get; private set; }
        public static ILogger Logger => _logger ??= Ioc.Default.GetService<ILogger<App>>()!;

        public static IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder()
                .ConfigureContainer<IServiceCollection>(collection =>
                {
                    WinRepo.Wpf.Startup.ConfigureServices(collection);
                    Ioc.Default.ConfigureServices(collection.BuildServiceProvider());
                    //Logger.LogDebug("ConfiguredServices.");
                });


        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            Exit += App_Exit;

            ServiceHost = CreateHostBuilder().Build();

            ServiceHost.Start();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ServiceHost.StopAsync().Wait(TimeSpan.FromSeconds(15));

            ServiceHost.Dispose();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.Exception, e.Exception.Message);
            e.Handled = true;
        }
    }
}
