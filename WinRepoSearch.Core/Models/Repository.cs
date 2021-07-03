using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WinRepoSearch.Core.Models
{
    // Remove this class once your pages/features are using your data.
    // This is used by the SampleDataService.
    // It is the model class we use to display data on pages like Grid, Chart, and ListDetails.
    public record Repository
    {
        public Repository()
        {
        }

        public string RepositoryId { get; init; }
        public string RepositoryName { get; init; }

        [Key]
        public string Key => RepositoryId;

        public string? Website { get; init; }

        public string? SupportEmail { get; init; }

        public string? SupportForum { get; init; }

        public string? SupportChat { get; init; }

        public string? Notes { get; init; }
        public string? Command { get; init; }
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
        public ILogger<Repository> Logger { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

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

                if (enabled)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private string? FindLine(string[] log, string? header)
        {
            if (string.IsNullOrWhiteSpace(header)) return null;

            foreach (var line in log)
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
                : ExecuteCommand(Command, SearchCmd, searchTerm);

        internal Task<LogItem> GetInfo(SearchResult result)
            => ExecuteCommand(Command, InfoCmd, result);

        internal Task<LogItem> Install(SearchResult result)
            => ExecuteCommand(Command, InstallCmd, result);

        const string searchScript = @"$module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

. 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\Assemblies.ps1'

Remove-Module $module -ErrorAction SilentlyContinue
Import-Module $module

# Assertion
$result = Search-WinRepoRepositories -Query {1} -Repo '{0}' -Verbose

$count = $result.Length;

Write-Verbose ""`$result.Length: $count""

Write-Output $result
";

        private Task<LogItem> ExecuteCommand<TParameter>(string? command, string? argument, TParameter parameter, int timeout = 0)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            _ = argument ?? throw new ArgumentNullException(nameof(argument));

            argument = argument.Split(' ').FirstOrDefault();

            Logger.LogDebug($"Preparing Execute({command},{argument},{parameter}) in {RepositoryName}");

            var searchTerm = string.Empty;

            if (parameter is string strParameter)
            {
                if (string.IsNullOrEmpty(strParameter))
                {
                    Logger.LogDebug($"Leaving {command}({parameter}) in {RepositoryName} because parameter is an empty string.");
                    return Task.FromResult(LogItem.Empty);
                }

                searchTerm = strParameter;
            }
            else if (parameter is SearchResult result)
            {
                searchTerm = result.AppId;
            }

            Logger.LogDebug($"searchTerm: {searchTerm}");

            var isScript = command.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase);

            PowerShell? newPs = null;
            ScriptBlock? scriptBlock = null;

            try
            {
                Logger.LogDebug($"Runspace.CanUseDefaultRunspace: {Runspace.CanUseDefaultRunspace}");

                Runspace CreateLocalRunspace()
                {
                    Logger.LogDebug($"CreateLocalRunspace: Enter");

                    Runspace? lrs = null;

                    try
                    {
                        var ci = Runspace.DefaultRunspace?.ConnectionInfo
                            ?? Runspace.DefaultRunspace?.OriginalConnectionInfo;

                        if (ci is not null)
                        {
                            Logger.LogDebug($"CreateLocalRunspace: ConnectionInfo: {ci}");

                            lrs = Runspace.GetRunspaces(ci)?.FirstOrDefault();
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    var host = new WinRepoHost(Logger);
                    lrs ??= RunspaceFactory.CreateRunspace(host);

                    lrs.Name ??= "WinRepo Runspace";

                    Logger.LogDebug($"CreateLocalRunspace: Exit");
                    return lrs;
                }

                Runspace? rs;
                ManualResetEventSlim? mr;

                PowerShell CreatePowerShell()
                {
                    var localPs = PowerShell.Create(CreateLocalRunspace());
                    rs = localPs.Runspace;

                    mr = new ManualResetEventSlim();

                    if (rs.RunspaceStateInfo.State != RunspaceState.Opened)
                    {
                        rs.StateChanged += (sender, args) =>
                        {
                            if (args.RunspaceStateInfo.State.Equals(RunspaceState.Opened))
                            {
                                mr.Set();
                            }
                        };

                        rs.Open();

                        mr.Wait(TimeSpan.FromSeconds(60));
                    }

                    return localPs;
                }

                newPs = CreatePowerShell();

                var scriptCode = argument switch
                {
                    _ => string.Format(searchScript, RepositoryName, parameter)
                };

                Logger.LogDebug(scriptCode);

                scriptBlock = ScriptBlock.Create(scriptCode);

                var ps = newPs
                    .AddStatement()
                    .AddCommand("Invoke-Command")
                    .AddParameter("-NoNewScope")
                    .AddParameter("-Verbose")
                    .AddParameter("-ScriptBlock", scriptBlock)
                    ;


                Logger.LogDebug("*** Before Invoke ***");

                var asyncState = ps.BeginInvoke();

                if (timeout > 0)
                {
                    asyncState.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout));
                }
                else
                {
                    asyncState.AsyncWaitHandle.WaitOne();
                }

                if (!asyncState.IsCompleted)
                {
                    Debug.Write("Force Stop...");
                    ps.Stop();
                    Debug.Write($"{asyncState.IsCompleted}");
                }

                var errors = ps.Streams.Error.ToList();
                if (errors.Count > 0)
                {

                    foreach (var error in errors)
                    {
                        Logger.LogError(error.ToString());
                    }

                    return Task.FromException<LogItem>(new AggregateException(errors.Select(e => e.Exception)));
                }
                else
                {
                    var result = ps.EndInvoke(asyncState);

                    Logger.LogDebug("After Invoke");

                    foreach (var err in ps.Streams.Error)
                    {
                        Logger.LogDebug($"ERROR: {err}");
                    }

                    //ps.Dispose();
                    //newPs.Dispose();

                    //newPs = CreatePowerShell();

                    //ps = newPs.AddCommand("Invoke-Command")
                    //    .AddParameter("-NoNewScope")
                    //    .AddParameter("-Verbose")
                    //    .AddParameter("-ScriptBlock", script)
                    //    ;

                    Logger.LogDebug($"Invoke-Command -NoNewScope -ScriptBlock {scriptBlock} in {RepositoryName}");

                    //var result = ps.Invoke();

                    Logger.LogDebug("Begin Results:");
                    result.ToList().ForEach(r => Logger.LogDebug(r?.ToString() ?? "<null>"));
                    Logger.LogDebug("End Results:");
                    Logger.LogDebug("Begin Error:");
                    ps.Streams.Error.ToList().ForEach(r => Logger.LogError(r?.ToString() ?? "<null>"));
                    Logger.LogDebug("End Error:");

                    var res = CleanAndBuildResult(result, command, parameter);
                    return Task.FromResult(res);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in Execute({scriptBlock}) in {RepositoryName}");
                throw;
            }
            finally
            {
                Logger.LogDebug($"Leaving Execute(Invoke-Command -NoNewScope -ScriptBlock {scriptBlock}) in {RepositoryName}");
                newPs?.Dispose();
            }
        }

        public LogItem CleanAndBuildResult<TParameter>(PSDataCollection<PSObject> result, string? command, TParameter parameter)
        {
            if (result.FirstOrDefault()?.Properties["name"] is null)
            {
                Logger.LogDebug("CleanAndBuildResult: Creating new results from strings.");
                var log = result.Select(i => i?.ToString() ?? "<null>").ToArray();
                var res = BuildSearchResults(log, parameter?.ToString());
                Logger.LogDebug($"CleanAndBuildResult: Returning: res.Result.Count(): {res.Result.Count()}");
                return res;
            }
            else
            {
                Logger.LogDebug("CleanAndBuildResult: Creating new results from PSObjects.");

                var res = LogItem.Empty;

                foreach(var item in result)
                {
                    var sr = new SearchResult(item.Properties, this);
                    res._result = res.Result.Append(sr);
                }

                Logger.LogDebug($"CleanAndBuildResult: Returning: res.Result.Count(): {res.Result.Count()}");
                return res;
            }
        }

        private void Runspace_StateChanged(object? sender, System.Management.Automation.Runspaces.RunspaceStateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private LogItem BuildResults<TParameter>(string command, TParameter parameter, string[] log)
            => command
                .Split(' ')
                .LastOrDefault() switch
            {
                "search" => BuildSearchResults(log, parameter as string),
                "info" or "show" => BuildInfoResults(log, parameter as SearchResult),
                _ => LogItem.Empty,
            };

        private LogItem BuildInfoResults(string[] log, SearchResult? searchResult)
        {
            _ = searchResult ?? throw new ArgumentNullException(nameof(searchResult));

            switch (searchResult.Repo.RepositoryName.ToLowerInvariant())
            {
                case "winget":
                    if (log.FirstOrDefault()?
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
                Logger.LogDebug(line);

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
                        ? line[versionIndex..Math.Min(afterVersionIndex, line.Length)]
                        : versionRegex.Match(line).Groups.Values.LastOrDefault()?.Value ?? line).Trim();

                    if (appName == new string('-', appName.Length) ||
                        appName.Equals(AppNameColumn) ||
                        appVersion.StartsWith("v") ||
                        line.IndexOf("bucket:", StringComparison.OrdinalIgnoreCase) > -1 ||
                        line.EndsWith("packages found.", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith("does not exist.", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (line.StartsWith("Did you know") ||
                        line.StartsWith("No package found matching input criteria.")) break;

                    if (string.IsNullOrWhiteSpace(appName) && string.IsNullOrEmpty(appId))
                    {
                        continue;
                    }

                    var item = new SearchResult(line, this)
                    {
                        AppName = appName,
                        AppId = appId,
                        AppVersion = appVersion,
                    };

                    if (!string.IsNullOrEmpty(item.AppName.Trim()) &&
                        !string.IsNullOrEmpty(item.AppVersion.Trim()))
                    {
                        result.Add(item);
                    }
                }
                catch
                {
                    continue;
                }
            }

            var innerItems = log.Select(l => new InnerItem(DateTimeOffset.Now, l)).ToArray();

            var res = new LogItem(result, innerItems);

            return res;
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