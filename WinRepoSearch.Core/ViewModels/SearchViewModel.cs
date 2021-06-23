using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinRepoSearch.Contracts.ViewModels;
using WinRepoSearch.Core.Contracts.Services;
using WinRepoSearch.Core.Models;

namespace WinRepoSearch.ViewModels
{
    public class SearchViewModel : ObservableRecipient, INavigationAware
    {
        private readonly Core.Services.SearchService _searchService;
        private readonly ConcurrentBag<string> _bag = new();
        private SearchResult? _selected;
        private ObservableCollection<SearchResult> _searchResults
            = new();
        private string? _searchTerm;
        private ObservableCollection<Repository>? _repositories;
        private bool isEmpty = true;
        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy; 
            set => SetProperty(ref isBusy, value);
        }

        public void Busy([CallerMemberName] string caller = "unknown")
        {
            _bag.Add(caller);
            IsBusy = true;
        }

        public void Done([CallerMemberName] string caller = "unknown")
        {
            _bag.TryTake(out _);
            IsBusy = _bag.Any();
        }

        public SearchResult? Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    ExecuteGetInfo(this);
                }
            }
        }

        public string SearchTerm
        {
            get => _searchTerm ?? string.Empty;
            set => SetProperty(ref _searchTerm, value);
        }

        public ObservableCollection<Repository> Repositories => _repositories ??= new(_searchService.Repositories);

        public ObservableCollection<InnerItem> Log { get; } = new();

        public string LogText => string.Join($"{Environment.NewLine}", Log.Select(line => $"{line.Timestamp:T}: {line.Message}"));

        public string LogMarkdown => @$"
# Action Log
                
```
{LogText}
```
";

        public AsyncRelayCommand<SearchViewModel?> PerformSearch
        => new(
            ExecuteSearch,
            viewModel => !string.IsNullOrWhiteSpace(viewModel?.SearchTerm));

        private Func<SearchViewModel?, Task> ExecuteSearch =>
                        async viewModel =>
                        {
                            Busy();
                            try
                            {
                                SearchResults.Clear();
                                Log.Clear();
                                Log.Add(new InnerItem(DateTimeOffset.Now, $"Searching for {viewModel?.SearchTerm}\n"));
                                var needReset = true;
                                await foreach (LogItem log in _searchService.PerformSearchAsync(viewModel))
                                {
                                    if (needReset)
                                    {
                                        Log.Clear();
                                        Log.Add(new InnerItem(DateTimeOffset.Now, $"Results for {viewModel?.SearchTerm}\n"));
                                        needReset = false;
                                    }

                                    if (log.Log.Any())
                                    {
                                        log.Log.ToList().ForEach(Log.Add);
                                    }

                                    if (log.Result.Any())
                                    {
                                        log.Result.ToList().ForEach(SearchResults.Add);
                                        IsEmpty = false;
                                    }
                                    else
                                    {
                                        IsEmpty = true;
                                    }
                                }

                                Log.Add(new InnerItem(DateTimeOffset.Now, "Search Completed."));
                            }
                            finally
                            {
                                Done();
                            }
                        };

        public AsyncRelayCommand<SearchViewModel?> PerformGetInfo
        => new(
            ExecuteGetInfo,
            viewModel => !string.IsNullOrWhiteSpace(viewModel?.SearchTerm));

        private Func<SearchViewModel?, Task> ExecuteGetInfo =>
                        viewModel =>
                        {
                            try
                            {
                                Busy();
                                return _searchService.PerformGetInfoAsync(viewModel);
                            }
                            finally
                            {
                                Done();
                            }
                        };

        public ObservableCollection<SearchResult> SearchResults
        {
            get => _searchResults;
            private set => SetProperty(ref _searchResults, value);
        }
        public bool IsEmpty
        {
            get => isEmpty;
            private set
            {
                if (SetProperty(ref isEmpty, value, true))
                {
                    OnPropertyChanged(nameof(IsNotEmpty));
                }
            }
        }

        public bool IsLogEmpty => Log.Count == 0;
        public bool IsLogNotEmpty => Log.Count > 0;

        public bool IsNotEmpty => !IsEmpty;

        public SearchViewModel(Core.Services.SearchService sampleDataService)
        {
            _searchService = sampleDataService;

            Log.CollectionChanged += Log_CollectionChanged;

            SearchResults.CollectionChanged += SearchResults_CollectionChanged  ;
        }

        private void SearchResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems?.Cast<SearchResult>().FirstOrDefault();
                    if (newItem is not null)
                    {
                        newItem.IsBusy -= SearchViewModel_IsBusy;
                        newItem.IsBusy += SearchViewModel_IsBusy;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.NewItems?.Cast<SearchResult>().FirstOrDefault();
                    if (oldItem is not null)
                    {
                        oldItem.IsBusy -= SearchViewModel_IsBusy;
                    }
                    break;
            }
        }

        private void SearchViewModel_IsBusy(bool isBusy)
        {
            if (isBusy)
                Busy();
            else
                Done();
        }

        private void Log_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LogText));
            OnPropertyChanged(nameof(LogMarkdown));
            OnPropertyChanged(nameof(IsLogEmpty));
            OnPropertyChanged(nameof(IsLogNotEmpty));
        }

        public async void OnNavigatedTo(object? _)
        {
            if (SearchResults.Count == 0)
            {
                await foreach (var (items, log) in _searchService.PerformSearchAsync(this))
                {
                    items?.ToList().ForEach(SearchResults.Add);
                    log?.ToList().ForEach(Log.Add);
                }
            }
        }

        public void OnNavigatedFrom()
        {
        }

        public void EnsureItemSelected()
        {
            if (Selected == null)
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                Selected = SearchResults.FirstOrDefault();
#pragma warning restore CS8601 // Possible null reference assignment.
            }
        }
    }
}
