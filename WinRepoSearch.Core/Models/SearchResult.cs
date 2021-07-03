using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinRepoSearch.Core.Services;

namespace WinRepoSearch.Core.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class SearchResult : ObservableObject
    {
        private DateTime? resultDate;
        private DateTime? createdDate;
        private DateTime? modifiedDate;
        private string? appName;
        private string? appId;
        private string? appDescription;
        private string? appVersion;
        private string? appRating;
        private string? publisherName;
        private string? publisherWebsite;
        private string? publisherContact;
        private string? notes;

        public event Action<bool> IsBusy;

        public SearchResult(string resultId, Repository repo)
        {
            ResultId = resultId;
            Repo = repo;

            PropertyChanged += SearchResult_PropertyChanged ;
        }

        public SearchResult(PSMemberInfoCollection<PSPropertyInfo> result, Repository repo)
        {
            _ = result ?? throw new ArgumentNullException(nameof(result));

            ResultId = result["id"]?.Value.ToString() ?? "Unknown";
            AppId = ResultId;
            AppName = result["name"]?.Value.ToString() ?? "Unknown";
            AppVersion = result["version"]?.Value.ToString() ?? "Unknown";

            Repo = repo;

            PropertyChanged += SearchResult_PropertyChanged ;
        }

        private void SearchResult_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Markdown))
            {
                OnPropertyChanged(nameof(Markdown));
            }
        }

        [Key]
        public object Key => ResultId ?? Guid.NewGuid().ToString();

        public DateTime? ResultDate { get => resultDate; init => SetProperty(ref resultDate, value); }

        public DateTime? CreatedDate { get => createdDate; init => SetProperty(ref createdDate, value); }

        public DateTime? ModifiedDate { get => modifiedDate; init => SetProperty(ref modifiedDate, value); }

        public string? AppName { get => appName; init => SetProperty(ref appName, value); }
        public string? AppId { get => appId; init => SetProperty(ref appId, value); }

        public string? AppDescription { get => appDescription; set => SetProperty(ref appDescription, value); }

        public string? AppVersion { get => appVersion; init => SetProperty(ref appVersion, value); }

        public string? AppRating { get => appRating; set => SetProperty(ref appRating, value); }

        public IEnumerable<string> Dependencies { get; }
            = new List<string>();

        public string? PublisherName { get => publisherName; set => SetProperty(ref publisherName, value); }

        public string? PublisherWebsite { get => publisherWebsite; set => SetProperty(ref publisherWebsite, value); }

        public string? PublisherContact { get => publisherContact; set => SetProperty(ref publisherContact, value); }

        public string? Notes { get => notes; set => SetProperty(ref notes, value); }

        public IEnumerable<string> Tags { get; }
            = new List<string>();

        public string TagsString => $"tags: {string.Join(", ", Tags)}";

        public IDictionary<string, string> AppMetadata { get; }
            = new ConcurrentDictionary<string, string>();

        public IDictionary<string, string> PublisherMetadata { get; }
            = new ConcurrentDictionary<string, string>();
        public string ResultId { get; }
        public Repository Repo { get; }

        public static Func<SearchResult?, Task> ExecuteDownload =>
            async result =>
            {
                if (result is null) throw new ArgumentNullException(nameof(result));

                result.Busy();
                try
                {
                    await SearchService.Instance!.PerformInstall(result);
                }
                finally
                {
                    result.Done();
                }
            };

        private void Done()
        {
            IsBusy?.Invoke(false);
        }

        private void Busy()
        {
            IsBusy?.Invoke(true);
        }

        public AsyncRelayCommand<SearchResult> InstallCommand = new (
            ExecuteDownload, 
            _ => true
            );

        public override string ToString()
        {
            return $"{AppName ?? Key} {AppVersion}";
        }

        public string Markdown => @$"
# 🔸 {AppName ?? Key ?? "<none>"} ({AppVersion ?? "<none>"})

{AppDescription ?? "<none>"}

{(string.IsNullOrWhiteSpace(PublisherName) ? "" : $"* 📰 Publisher: **{PublisherName}**")}
{(string.IsNullOrWhiteSpace(PublisherWebsite) ? "" : $"* 🌐 Website: **[{PublisherWebsite}]({PublisherWebsite ?? ""})**")}

## 📓 Notes

{Notes?.Replace(Environment.NewLine, "\n\n") ?? "<none>"}

## 🔗 Links

🔍 [Google](https://www.google.com/search?q={AppName.Replace(" ", "%20")}%20{AppVersion}) {'\n'}
🔍 [Bing](https://www.bing.com/search?q={AppName.Replace(" ", "%20")}%20{AppVersion}) {'\n'}
🔍 [Stack Overflow](https://www.stackoverflow.com/search?q={AppName.Replace(" ", "%20")}) {'\n'}
🔍 [GitHub](https://github.com/search?q=in%3A{AppName.Replace(" ", "%20")}&type=Repositories)

---

_**Results from {Repo.RepositoryName}**_
";

        public string ListMarkdown => 
        $"🔸 [{Repo.RepositoryName}] {AppName ?? Key ?? "<none>"} ({AppVersion ?? "<none>"})";
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
