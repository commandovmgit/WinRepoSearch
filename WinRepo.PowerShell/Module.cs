using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinRepoSearch.Core.Contracts.Services;

namespace WinRepo.PowerShell
{
    public static class Module
    {
        public static IHost ServiceHost { get; private set; }

        public static IStartup iStartup => GetStartup();

        public static IStartup GetStartup()
            => ServiceHost.Services.GetRequiredService<IStartup>();


        private static bool _initialized = false;

        [ModuleInitializer]
        static public void MyInitializer()
        {
            if (_initialized) return;

            _initialized = true;

            var builder = CreateHostBuilder();
            ServiceHost = builder.Build();

            iStartup.ServiceProvider = ServiceHost.Services;

            try
            {
                Ioc.Default.ConfigureServices(ServiceHost.Services);
            }
            catch
            {
                // Ignore.
            }

            ServiceHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureContainer<IServiceCollection>(collection =>
            {
                Startup.ConfigureServices(collection);
                //Logger.LogDebug("ConfiguredServices.");
            });
    }
}
