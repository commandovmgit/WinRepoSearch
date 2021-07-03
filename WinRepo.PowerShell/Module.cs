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


        [ModuleInitializer]
        static public void MyInitializer()
        {
            //Logger.LogDebug("Initializing Host.");

            var builder = CreateHostBuilder();
            ServiceHost = builder.Build();

            iStartup.ServiceProvider = ServiceHost.Services;

            //Logger.LogDebug($"startup.ServiceProvider: [{iStartup.ServiceProvider}]");

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
