using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using WinRepo.PowerShell;

using WinRepoSearch.Core.Services;
using WinRepoSearch.ViewModels;

namespace WinRepoConsole
{
    class Program
    {
        public static IHost ServiceHost { get; private set; }

        [ModuleInitializer]
        static internal void MyInitializer()
        {
            //Logger.LogDebug("Initializing Host.");

            var builder = CreateHostBuilder();
            ServiceHost = builder.Build();
            ServiceHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureContainer<IServiceCollection>(collection =>
            {
                Startup.ConfigureServices(collection);
                //Logger.LogDebug("ConfiguredServices.");
            });

        private static ILogger? Logger {get; set;}

        static async Task Main(string[] args)
        {
            var service = ServiceHost.Services.GetRequiredService<SearchService>();
            var logger = ServiceHost.Services.GetRequiredService<ILogger<Program>>();
            var viewModel = ServiceHost.Services.GetRequiredService<SearchViewModel>();

            var searchTerm = args.FirstOrDefault() ?? "vscode";
            var repoToSearch = "All";

            Logger = Startup.Services?.GetRequiredService<ILogger<Program>>();

            Logger.LogDebug($"SearchRepositoryCmdlet.ProcessRecord: Enter - SearchTerm: {searchTerm}, RepoToSearch: {repoToSearch}");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentNullException(nameof(searchTerm));
            }

            viewModel
                .Repositories
                .ToList()
                .ForEach(r =>
                    r.IsEnabled =
                        (repoToSearch == "All") ||
                        r.RepositoryName.Equals(repoToSearch, StringComparison.OrdinalIgnoreCase)
                );

            viewModel.SearchTerm = searchTerm;

            try
            {
                Logger.LogDebug("SearchRepositoryCmdlet.ProcessRecord: Before PerformSearchAsync");
                var results = service.PerformSearchAsync(viewModel);
                Logger.LogDebug("SearchRepositoryCmdlet.ProcessRecord: After PerformSearchAsync");

                await foreach (var resultSet in results)
                {
                    Logger.LogDebug("SearchRepositoryCmdlet.ProcessRecord: Moved Next");

                    Logger.LogDebug($"Found {resultSet.Result.Count()} results in {resultSet.Result.FirstOrDefault()?.Repo.RepositoryName}");

                    resultSet.Result.ToList().ForEach(r => Console.WriteLine(r.ListMarkdown));

                    Logger.LogDebug($"SearchRepositoryCmdlet.ProcessRecord: wrote: {resultSet}");
                }
            }
            catch (AggregateException ae)
            {
                Console.Error.WriteLine(ae.ToString());
            }
        }
    }
}
