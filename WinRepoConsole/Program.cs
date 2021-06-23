using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Console.WriteLine("Initializing Host.");

            var builder = CreateHostBuilder();
            ServiceHost = builder.Build();
            ServiceHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureContainer<IServiceCollection>(collection =>
            {
                Startup.ConfigureServices(collection);
                Console.WriteLine("ConfiguredServices.");
            });

        static void Main(string[] args)
        {
            var service = ServiceHost.Services.GetRequiredService<SearchService>();
            var logger = ServiceHost.Services.GetRequiredService<ILogger<Program>>();
            var viewModel = ServiceHost.Services.GetRequiredService<SearchViewModel>();

            var searchTerm = args.FirstOrDefault() ?? "vscode";
            var repoToSearch = "Scoop";

            Console.WriteLine($"SearchRepositoryCmdlet.ProcessRecord: Enter - SearchTerm: {searchTerm}, RepoToSearch: {repoToSearch}");

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

            Console.WriteLine("SearchRepositoryCmdlet.ProcessRecord: Before PerformSearchAsync");
            var results = service.PerformSearchAsync(viewModel);
            Console.WriteLine("SearchRepositoryCmdlet.ProcessRecord: After PerformSearchAsync");

            var enumerator = results.GetAsyncEnumerator();

            while (enumerator.MoveNextAsync().Result)
            {
                Console.WriteLine("SearchRepositoryCmdlet.ProcessRecord: Moved Next");

                var resultSet = enumerator.Current;

                logger.LogInformation($"Found {resultSet.Result.Count()} results in {resultSet.Result.FirstOrDefault()?.Repo.RepositoryName}");

                resultSet.Result.ToList().ForEach(r => logger.LogInformation(r.Markdown));

                Console.WriteLine($"SearchRepositoryCmdlet.ProcessRecord: wrote: {resultSet}");
            }

        }
    }
}
