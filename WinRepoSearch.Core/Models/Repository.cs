using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinRepoSearch.Core.Models
{
    // Remove this class once your pages/features are using your data.
    // This is used by the SampleDataService.
    // It is the model class we use to display data on pages like Grid, Chart, and ListDetails.
    public record Repository
    {
        public Repository() { }

        public string RepositoryId { get; init; }
        public string RepositoryName { get; init; }

        [Key]
        public string Key => RepositoryId;

        public string? Website { get; init; }

        public string? SupportEmail { get; init; }

        public string? SupportForum { get; init; }

        public string? SupportChat { get; init; }

        public string? Notes { get; init; }
        public string? SearchCmd { get; init; }
        public string? InstallCmd { get; init; }
        public string? ListCmd { get; init; }
        public string? InfoCmd { get; init; }

        public string? AppNameColumn { get; init; }
        public string? AppIdColumn { get; init; }
        public string? AppVersionColumn { get; init; }

        public string? AppNameRegex { get; init; }
        public string? AppIdRegex { get; init; }
        public string? AppVersionRegex { get; init; }

        public string? InfoPublisherHeader { get; init; }
        public string? InfoDescriptionHeader { get; init; }
        public string? InfoWebsiteHeader { get; init; }
        public string? InfoNotesHeader { get; init; }

        public bool IsEnabled { get; set; } = true;
        public string? AppAfterVersionColumn { get; private set; }

        private LogItem ParseDetails(string[] log, SearchResult searchResult)
        {
            searchResult.PublisherName = FindLine(log, InfoPublisherHeader);
            searchResult.AppDescription = FindLine(log, InfoDescriptionHeader);
            searchResult.PublisherWebsite = FindLine(log, InfoWebsiteHeader);
            searchResult.Notes = FindNotes(log, InfoNotesHeader);

            return LogItem.Empty;

            static string ParseLines(string[] log, (int start, int end, int index) parameters)
            {
                if (parameters.start > -1 && parameters.end == -1)
                {
                    StringBuilder line = new();
                    for (int i = parameters.start; i < log.Length; ++i)
                    {
                        if (i == parameters.start)
                        {
                            line.AppendLine(log[i].Substring(parameters.index).Trim());
                        }
                        else
                        {
                            line.AppendLine(log[i].Trim());
                        }
                    }

                    return line.ToString();
                }

                return string.Empty;
            }
        }

        private string? FindNotes(string[] log, string? header)
        {
            if (string.IsNullOrWhiteSpace(header)) return null;

            var enabled = false;
            var sb = new StringBuilder();

            foreach (var line in log)
            {
                if (line.StartsWith(header))
                {
                    enabled = true;
                }

                if(enabled)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private string? FindLine(string[] log, string? header)
        {
            if (string.IsNullOrWhiteSpace(header)) return null;

            foreach(var line in log)
            {
                if (line.StartsWith(header))
                {
                    return line.Replace(header, "");
                }
            }

            return null;
        }

        internal Task<LogItem> Search(string searchTerm) => !IsEnabled
                ? Task.FromResult(LogItem.Empty)
                : ExecuteCommand(SearchCmd, searchTerm);

        internal Task<LogItem> GetInfo(SearchResult result)
            => ExecuteCommand(InfoCmd, result);

        internal Task<LogItem> Install(SearchResult result)
            => ExecuteCommand(InstallCmd, result);

        private async Task<LogItem> ExecuteCommand<TParameter>(string? command, TParameter parameter)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));

            string[] array;

            var searchTerm = string.Empty;

            if (parameter is string strParameter)
            {
                if (string.IsNullOrEmpty(strParameter))
                {
                    return LogItem.Empty;
                }

                searchTerm = strParameter;
            }
            else if(parameter is SearchResult result)
            {
                searchTerm = result.AppId;
            }

            var processInfo = new ProcessStartInfo("powershell.exe", string.Format(command, searchTerm))
            {
                CreateNoWindow = true,
                LoadUserProfile = true,
                RedirectStandardOutput = true,
                Verb="runas"
            };

            try
            {
                var process = Process.Start(processInfo)!;

                var delay = Task.Delay(TimeSpan.FromSeconds(30));
                var processTask = process.WaitForExitAsync();

                var result = await Task.WhenAny(processTask, delay);

                if (result == processTask)
                {
                    var results = await process.StandardOutput.ReadToEndAsync();

                    results = new Regex(@"^[^a-zA-Z0-9]*(?=[a-zA-Z0-9_.\-])").Replace(results, "").Trim();

                    array = results.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    return BuildResults(command, parameter, array);
                }

                return LogItem.Empty;
            }
            catch
            {
                throw;
            }            
        }

        private LogItem BuildResults<TParameter>(string command, TParameter parameter, string[] log)
        {
            switch (command.Split(' ')[1])
            {
                case "search":
                    return BuildSearchResults(log, parameter as string);

                case "info":
                case "show":
                    return BuildInfoResults(log, parameter as SearchResult);

            }

            return LogItem.Empty;
        }

        private LogItem BuildInfoResults(string[] log, SearchResult? searchResult)
        {
            _ = searchResult ?? throw new ArgumentNullException(nameof(searchResult));

            switch(searchResult.Repo.RepositoryName.ToLowerInvariant())
            {
                case "winget":
                    if(log.FirstOrDefault()?
                        .Equals(
                            "No package found matching input criteria.", 
                            StringComparison.OrdinalIgnoreCase) ?? true)
                    {
                        return LogItem.Empty;
                    }

                    return ParseDetails(log, searchResult);

                case "chocolatey":
                    return ParseDetails(log, searchResult);

                case "scoop":
                    return ParseDetails(log, searchResult);
            }

            return LogItem.Empty;
        }

        private LogItem BuildSearchResults(string[] log, string? searchTerm)
        { 
            List<SearchResult> result = new List<SearchResult>();

            var nameIndex = GetIndex(log, AppNameColumn);
            var idIndex = GetIndex(log, AppIdColumn);
            var versionIndex = GetIndex(log, AppVersionColumn);
            var afterVersionIndex = GetIndex(log, AppAfterVersionColumn);

            var nameRegex = new Regex(AppNameRegex ?? ".*");
            var idRegex = new Regex(AppIdRegex ?? ".*");
            var versionRegex = new Regex(AppVersionRegex ?? ".*");

            foreach (var line in log)
            {
                Debug.WriteLine(line);

                try
                {
                    var appName = (nameIndex > -1
                        ? line[nameIndex..Math.Min(idIndex, line.Length)]
                        : nameRegex.Match(line).Groups.Values.LastOrDefault()?.Value ?? line).Trim();

                    var appId = nameIndex != idIndex
                        ? (idIndex > -1
                            ? line[idIndex..Math.Min(versionIndex, line.Length)]
                            : idRegex.Match(line).Groups.Values.LastOrDefault()?.Value ?? line).Trim()
                        : appName;

                    var appVersion = (versionIndex > -1
                        ? line[versionIndex..Math.Min(afterVersionIndex,line.Length)]
                        : versionRegex.Match(line).Groups.Values.LastOrDefault()?.Value ?? line).Trim();

                    if (appName == new string('-', appName.Length) ||
                        appName.Equals(AppNameColumn) ||
                        appVersion.StartsWith("v") ||
                        line.IndexOf("bucket:", StringComparison.OrdinalIgnoreCase) > -1 ||
                        line.EndsWith("packages found.", StringComparison.OrdinalIgnoreCase)) continue;

                    if (string.IsNullOrWhiteSpace(appName) && string.IsNullOrEmpty(appId)) continue;

                    var item = new SearchResult(line, this)
                    {
                        AppName = appName,
                        AppId = appId,
                        AppVersion = appVersion,
                    };

                    if (!string.IsNullOrEmpty(item.AppName) &&
                        !string.IsNullOrEmpty(item.AppVersion))
                    {
                        result.Add(item);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return new LogItem(result, log.Select(l => new InnerItem(DateTimeOffset.Now, l)).ToArray());
        }

        private int GetIndex(string[] log, string? appColumn)
        {
            if (string.IsNullOrEmpty(appColumn)) return -1;

            var index = -1;

            foreach (var line in log)
            {
                index = line.IndexOf(appColumn, StringComparison.OrdinalIgnoreCase);

                if (index > -1) break;
            }

            return index;
        }
    }
}
