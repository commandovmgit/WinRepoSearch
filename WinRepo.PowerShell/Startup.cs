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
using WinRepoSearch.Core.ViewModels;

namespace WinRepo.PowerShell
{
#nullable enable
    public class Startup : IStartup
    {
        private ISearchService? _searchService;

        public IServiceProvider? ServiceProvider { get; set; }
        public static IServiceProvider? Services { get; private set; }

        public ISearchService? SearchService
            => _searchService ??= ServiceProvider?.GetRequiredService<ISearchService>();


        public Startup() { }

        public Startup(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Services = serviceProvider;
        }

        internal static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                //Logger.LogDebug("Added console.");
            });

            services.AddTransient<SearchViewModel>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<ISearchService, SearchService>();

            services.AddSingleton<IStartup, Startup>();
        }
    }
}
