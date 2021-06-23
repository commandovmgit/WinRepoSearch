using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Models;
using WinRepoSearch.Core.Services;
using WinRepoSearch.ViewModels;

namespace WinRepo.PowerShell
{
#nullable enable
    public class Startup : IStartup
    {
        public IServiceProvider? ServiceProvider { get; set; }

        public Startup() { }

        public Startup(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        internal static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                Console.WriteLine("Added console.");
            });

            services.AddTransient<SearchViewModel>();
            services.AddSingleton<SearchService>();

            services.AddSingleton<IStartup, Startup>();
        }
    }
}
