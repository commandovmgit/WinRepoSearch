using CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using WinRepo.PowerShell;

using WinRepoSearch.Core;
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

        private static ILogger? Logger { get; set; }
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('q', "query", Required = true, HelpText = "The search criteria.")]
            public string Query { get; set; }

            [Option('r', "repo", Required = false, HelpText = "Repository to search.  Default = All.")]
            public DefaultRepos Repo { get; set; } = DefaultRepos.All;
        }

        private static ParserResult<Options> CommandBuilder(params string[] args)
            => Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Query is not null and { Length: > 0 })
                       {
                           Console.WriteLine($"Searching for: {o.Query}");
                       }

                       Console.WriteLine($"Repository to search: {o.Repo}");
                   });
        

        static async Task Main(string[] args)
        {
            var parsed = CommandBuilder(args);

            var service = ServiceHost.Services.GetRequiredService<SearchService>();
            var logger = ServiceHost.Services.GetRequiredService<ILogger<Program>>();
            var viewModel = ServiceHost.Services.GetRequiredService<SearchViewModel>();

            DefaultRepos repo = DefaultRepos.All;
            Commands command = Commands.search;
            string query = "";
            bool isVerbose = false;

            parsed.WithParsed(options =>
            {
                (repo, command, query, isVerbose) = args.FirstOrDefault()?.ToLowerInvariant() switch {
                    nameof(Commands.show) => (options.Repo, Commands.show, options.Query, options.Verbose),
                    nameof(Commands.install) => (options.Repo, Commands.install, options.Query, options.Verbose),
                    nameof(Commands.list) => (options.Repo, Commands.list, options.Query, options.Verbose),
                    nameof(Commands.uninstall) => (options.Repo, Commands.uninstall, options.Query, options.Verbose),
                    nameof(Commands.upgrade) => (options.Repo, Commands.upgrade, options.Query, options.Verbose),
                    _ => (options.Repo, Commands.search, options.Query, options.Verbose)
                };
            });

            Logger = Startup.Services?.GetRequiredService<ILogger<Program>>();

            Logger.LogDebug($"SearchRepositoryCmdlet.ProcessRecord: Enter - SearchTerm: {query}, RepoToSearch: {repo}");

            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            viewModel
                .Repositories
                .ToList()
                .ForEach(r =>
                    r.IsEnabled =
                        (repo == DefaultRepos.All) ||
                        r.RepositoryName.Equals(repo.ToString(), StringComparison.OrdinalIgnoreCase)
                );

            viewModel.SearchTerm = query;
            viewModel.Command = command;
            viewModel.IsVerbose = isVerbose;

            try
            {
                switch(command)
                {
                    case Commands.list:
                        await Show(service, viewModel);
                        break;

                    case Commands.search:
                        await Search(service, viewModel);
                        break;

                    case Commands.install:
                        break;

                    case Commands.show:
                        break;

                    case Commands.uninstall:
                        break;

                    case Commands.upgrade:
                        break;
                }
            }
            catch (AggregateException ae)
            {
                Console.Error.WriteLine(ae.ToString());
            }
        }

        private static async Task Search(SearchService service, SearchViewModel viewModel)
        {
            Logger.LogDebug("Search: Before PerformSearchAsync");
            var results = service.PerformSearchAsync(viewModel);
            Logger.LogDebug("Search: After PerformSearchAsync");

            await foreach (var resultSet in results)
            {
                Logger.LogDebug("Search: Moved Next");

                Logger.LogDebug($"Search: Found {resultSet.Result.Count()} results in {resultSet.Result.FirstOrDefault()?.Repo.RepositoryName}");

                resultSet.Result.ToList().ForEach(r => Console.WriteLine(r.ListMarkdown));

                Logger.LogDebug($"Search: wrote: {resultSet.Result.Count()} records.");
            }
        }

        private static async Task Show(SearchService service, SearchViewModel viewModel)
        {
            Logger.LogDebug("Show: Before PerformSearchAsync");
            var results = await service.PerformGetInfoAsync(viewModel);
            Logger.LogDebug("Show: After PerformSearchAsync");

            Console.WriteLine(results.Result.FirstOrDefault()?.Markdown ?? "<Not found>");
        }
    }
}
