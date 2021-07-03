﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Models;
using WinRepoSearch.ViewModels;
using YamlDotNet.Serialization;

namespace WinRepoSearch.Core.Services
{
    public class SearchService : ISearchService
    {
        public ImmutableArray<Repository> Repositories { get; } = new();
        public static SearchService? Instance { get; private set; }
        public ILogger<SearchService> Logger { get; }

        public SearchService(IServiceProvider serviceProvider, ILogger<SearchService> logger)
        {
            _ = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            Logger = logger;

            Logger.LogDebug("SearchService.ctor(): Enter");

            Instance = this;

            var filename = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "Repos.yaml");

            if (File.Exists(filename))
            {
                Logger.LogDebug($"SearchService.ctor(): Reading: {filename}");
                Logger.LogDebug(File.ReadAllText(filename));

                var des = new DeserializerBuilder().Build();

                var repos = des.Deserialize<Repositories>(new StringReader(File.ReadAllText(filename)));
                Logger.LogDebug($"SearchService.ctor(): repos: {repos?.ToString() ?? "<null>"}");

                if (repos is not null)
                {
                    repos.ToList().ForEach(r =>
                    {
                        r.Logger = serviceProvider.GetService<ILogger<Repository>>()!;
                        r.ServiceProvider = serviceProvider.GetService<IStartup>()!.ServiceProvider ?? serviceProvider;
                    });
                    Repositories = repos.ToImmutableArray();
                }

                Logger.LogDebug($"SearchService.ctor(): repos: {repos?.Count ?? -1}");
                Logger.LogDebug($"SearchService.ctor(): Repositories: {Repositories.Count()}");
            }
            else
            {
                Logger.LogError($"SearchService.ctor(): File node found: {filename}");
            }
            Logger.LogDebug("SearchService.ctor(): Exit");
        }

        public async IAsyncEnumerable<LogItem> PerformSearchAsync(SearchViewModel? viewModel)
        {
            _ = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            Logger.LogDebug($"{viewModel.Repositories.Count} repos in viewModel.");

            foreach (var repo in viewModel.Repositories.Where(r => r.IsEnabled))
            {
                Logger.LogDebug($"Searching {repo.RepositoryName} for {viewModel.SearchTerm}");

                var result = await repo.Search(viewModel.SearchTerm);

                if (result is not null)
                {
                    Logger.LogDebug($"Found {result.Result.Count()} results in {repo.RepositoryName}.");
                    yield return result;
                }
            }
        }

        public async Task<LogItem> PerformGetInfoAsync(SearchViewModel? viewModel)
        {
            _ = viewModel?.Selected ?? throw new ArgumentNullException(nameof(viewModel.Selected));

            var repo = viewModel.Selected?.Repo;

            var result = await repo?.GetInfo(viewModel.Selected);

            return result ?? LogItem.Empty;
        }

        public async Task<LogItem> PerformInstall(SearchResult? searchResult)
        {
            _ = searchResult ?? throw new ArgumentNullException(nameof(searchResult));

            var repo = searchResult.Repo;

            var result = await repo.Install(searchResult);

            return result ?? LogItem.Empty;
        }
    }
}
