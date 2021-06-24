using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using WinRepoSearch.Core.Models;
using WinRepoSearch.Core.Services;
using WinRepoSearch.ViewModels;

namespace WinRepo.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Search, "Repository")]
    [OutputType(typeof(SearchResult))]
    public class SearchRepositoryCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, HelpMessage = "Search text")]
        [Alias("Query", "Q")]
        [PSDefaultValue]
        public string SearchTerm { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 1, HelpMessage = "Repositories to search.")]
        [Alias("Repo", "R")]
        public DefaultRepos RepoToSearch { get; set; } = DefaultRepos.All;

        public SearchService Service { get; private set; }
        public SearchViewModel ViewModel { get; private set; }
        public ILogger Logger { get; private set; }

        private static readonly IHost ServiceHost = Module.ServiceHost;

        protected override void BeginProcessing()
        {
            Console.WriteLine($"SearchRepositoryCmdlet.BeginProcessing: Enter - SearchTerm: {SearchTerm}, RepoToSearch: {RepoToSearch}");

            Service = ServiceHost.Services.GetRequiredService<SearchService>();
            Console.WriteLine("SearchRepositoryCmdlet.BeginProcessing: Created Service.");

            Logger = ServiceHost.Services.GetRequiredService<ILogger<SearchRepositoryCmdlet>>();
            Logger.LogInformation("SearchRepositoryCmdlet.BeginProcessing: Created Logger.");

            ViewModel = ServiceHost.Services.GetRequiredService<SearchViewModel>();
            Logger.LogInformation("SearchRepositoryCmdlet.BeginProcessing: Created ViewModel.");

            base.BeginProcessing();

            Logger.LogInformation("SearchRepositoryCmdlet.BeginProcessing: Exit");
        }

        protected override void StopProcessing()
        {
            Logger.LogInformation($"SearchRepositoryCmdlet.StopProcessing: Enter - SearchTerm: {SearchTerm}, RepoToSearch: {RepoToSearch}");

            base.StopProcessing();

            Logger.LogInformation("SearchRepositoryCmdlet.StopProcessing: Exit");
        }

        protected override void EndProcessing()
        {
            Logger.LogInformation($"SearchRepositoryCmdlet.EndProcessing: Enter - SearchTerm: {SearchTerm}, RepoToSearch: {RepoToSearch}");

            base.EndProcessing();

            Logger.LogInformation("SearchRepositoryCmdlet.EndProcessing: Exit");
        }

        protected override void ProcessRecord()
        {
            Logger.LogInformation($"SearchRepositoryCmdlet.ProcessRecord: Enter - SearchTerm: {SearchTerm}, RepoToSearch: {RepoToSearch}");

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                throw new ArgumentNullException(nameof(SearchTerm));
            }

            ViewModel
                .Repositories
                .ToList()
                .ForEach(r =>
                    r.IsEnabled =
                        RepoToSearch == DefaultRepos.All ||
                        r.RepositoryName.Equals(RepoToSearch.ToString(), StringComparison.OrdinalIgnoreCase)
                );

            ViewModel.SearchTerm = $"{SearchTerm} --PSObject";

            PerformSearchAsync().GetAwaiter().GetResult();

            base.ProcessRecord();

            Logger.LogInformation("SearchRepositoryCmdlet.ProcessRecord: Exit");
        }

        private async Task PerformSearchAsync()
        {
            Logger.LogInformation("SearchRepositoryCmdlet.ProcessRecord: Before PerformSearchAsync");
            var results = Service.PerformSearchAsync(ViewModel);
            Logger.LogInformation("SearchRepositoryCmdlet.ProcessRecord: After PerformSearchAsync");

            var enumerator = results.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync())
            {
                Logger.LogInformation("SearchRepositoryCmdlet.ProcessRecord: Moved Next");

                var resultSet = enumerator.Current;

                Logger.LogInformation($"resultSet: {resultSet}");

                Logger.LogInformation($"Found {resultSet.Result.Count()} results in {resultSet.Result.FirstOrDefault()?.Repo.RepositoryName}");

                WriteVerbose(string.Join(Environment.NewLine, resultSet.Log.Select(ii => $"{ii.Timestamp}: {ii.Message}")));

                WriteObject(resultSet);
                Logger.LogInformation($"SearchRepositoryCmdlet.ProcessRecord: wrote: {resultSet}");
            }
        }
    }

}
