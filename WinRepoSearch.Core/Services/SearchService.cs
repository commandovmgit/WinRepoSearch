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

        public SearchService()
        {
            Instance = this;

            var filename = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "Repos.yaml");
            if (File.Exists(filename))
            {
                var des = new DeserializerBuilder().Build();

                var repos = des.Deserialize<Repositories>(new StringReader(File.ReadAllText(filename)));

                if (repos is not null)
                {
                    Repositories = repos.ToImmutableArray();
                }
            }
        }

        public async IAsyncEnumerable<LogItem> PerformSearchAsync(SearchViewModel? viewModel)
        {
            _ = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            foreach (var repo in viewModel.Repositories)
            {
                var result = await repo.Search(viewModel.SearchTerm);

                if (result is not null)
                {
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
