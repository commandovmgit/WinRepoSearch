using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

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

        public SearchResult(string resultId, Repository repo)
        {
            ResultId = resultId;
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

        public override string ToString()
        {
            return $"{AppName ?? Key} {AppVersion}";
        }

        public string Markdown => @$"
# {AppName ?? Key ?? "<none>"} ({AppVersion ?? "<none>"})

{AppDescription ?? "<none>"}

* Publisher: **{PublisherName ?? "<none>"}**
* Website: **[{PublisherWebsite ?? "<none>"}]({PublisherWebsite ?? ""})**

## Notes

{Notes?.Replace(Environment.NewLine, "\n\n") ?? "<none>"}

## Links

[Google](https://www.google.com/search?q={AppName.Replace(" ", "%20")}%20{AppVersion})
[Bing](https://www.bing.com/search?q={AppName.Replace(" ", "%20")}%20{AppVersion})
[Stack Overflow](https://www.stackoverflow.com/search?q={AppName.Replace(" ", "%20")})
[GitHub](https://github.com/search?q=in%3A{AppName.Replace(" ", "%20")}&type=Repositories)

";
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
