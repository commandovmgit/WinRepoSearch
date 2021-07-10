using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WinRepoSearch.Core.Models;
using WinRepoSearch.Core.ViewModels;

namespace WinRepoSearch.Core.Contracts.Services
{
    // Remove this class once your pages/features are using your data.
    public interface ISearchService
    {
        IAsyncEnumerable<LogItem> PerformSearchAsync(SearchViewModel? viewModel);
    }
}
