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

        private Task<LogItem> ExecuteCommand<TParameter>(string? command, string? argument, TParameter parameter)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            _ = argument ?? throw new ArgumentNullException(nameof(argument));

            const string modulePath = @"C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepo.PowerShell.dll";

            Logger.LogInformation($"Preparing Execute({command},{argument},{parameter}) in {RepositoryName}");

            string[] array;

            var searchTerm = string.Empty;

            if (parameter is string strParameter)
            {
                if (string.IsNullOrEmpty(strParameter))
                {
                    Logger.LogInformation($"Leaving {command}({parameter}) in {RepositoryName} because parameter is an empty string.");
                    return Task.FromResult(LogItem.Empty);
                }

                searchTerm = strParameter;
            }
            else if (parameter is SearchResult result)
            {
                searchTerm = result.AppId;
            }

            Logger.LogInformation($"searchTerm: {searchTerm}");

            var isScript = command.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase);

            PowerShell? newPs = null;
            ScriptBlock? scriptBlock = null;

            try
            {
                Logger.LogInformation($"Runspace.CanUseDefaultRunspace: {Runspace.CanUseDefaultRunspace}");

                Runspace CreateLocalRunspace()
                {
                    Logger.LogInformation($"CreateLocalRunspace: Enter");

                    Runspace? lrs = null;

                    try
                    {
                        var ci = Runspace.DefaultRunspace?.ConnectionInfo
                            ?? Runspace.DefaultRunspace?.OriginalConnectionInfo;

                        if (ci is not null)
                        {
                            Logger.LogInformation($"CreateLocalRunspace: ConnectionInfo: {ci}");

                            lrs = Runspace.GetRunspaces(ci)?.FirstOrDefault();
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    var host = new WinRepoHost();
                    lrs ??= RunspaceFactory.CreateRunspace(host);

                    lrs.Name ??= "WinRepo Runspace";

                    Logger.LogInformation($"CreateLocalRunspace: Exit");
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

                var path = System.IO.Path.GetDirectoryName(modulePath);

                var scriptCode = $@"
                    pushd
                    try{{
                        Set-Location {path}
                        $module = '.\Scripts\WinRepo.psm1'

                        . '.\Scripts\Assemblies.ps1'

                        Remove-Module $module -ErrorAction SilentlyContinue
                        Import-Module $module

                        $result = Search-WinRepoRepositories -Query vscode -Repo '{RepositoryName}' -Verbose
                        $count = $result.Count

                        Write-Verbose ""script - `$count: $count""

                        if($result) {{
                            Format-Table $result
                        }}

                        return $result
                    }} catch {{
                        Write-Verbose (""script - caught: ["" + $_.ToString() + ""]"")
                    }} finally {{
                        popd
                    }}
                ";

                Console.WriteLine(scriptCode);

                scriptBlock = ScriptBlock.Create(scriptCode);

                var ps = newPs
                    .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument(modulePath)
                    .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument($@"{Path.GetDirectoryName(modulePath)}\Scripts\WinRepo.psm1")
                    .AddStatement()
                    .AddCommand("Invoke-Command")
                    .AddParameter("-NoNewScope")
                    .AddParameter("-Verbose")
                    .AddParameter("-ScriptBlock", scriptBlock)
                    ;


                Debug.WriteLine("*** Before Invoke ***");

                var asyncState = ps.BeginInvoke();

                asyncState.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(60));

                if (!asyncState.IsCompleted)
                {
                    Debug.Write("Force Stop...");
                    ps.Stop();
                    Debug.Write($"{asyncState.IsCompleted}");
                }

                var result = ps.EndInvoke(asyncState);

                Debug.WriteLine("After Invoke");

                foreach (var err in ps.Streams.Error)
                {
                    Debug.WriteLine($"ERROR: {err}");
                }

                //ps.Dispose();
                //newPs.Dispose();

                //newPs = CreatePowerShell();

                //ps = newPs.AddCommand("Invoke-Command")
                //    .AddParameter("-NoNewScope")
                //    .AddParameter("-Verbose")
                //    .AddParameter("-ScriptBlock", script)
                //    ;

                Logger.LogInformation($"Invoke-Command -NoNewScope -ScriptBlock {scriptBlock} in {RepositoryName}");

                //var result = ps.Invoke();

                Logger.LogDebug("Begin Results:");
                result.ToList().ForEach(r => Logger.LogDebug(r.ToString()));
                Logger.LogDebug("End Results:");
                Logger.LogDebug("Begin Error:");
                ps.Streams.Error.ToList().ForEach(r => Logger.LogError(r.ToString()));
                Logger.LogDebug("End Error:");

                return Task.FromResult(CleanAndBuildResult(argument, command, parameter));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in Execute({scriptBlock}) in {RepositoryName}");
                throw;
            }
            finally
            {
                Logger.LogInformation($"Leaving Execute(Invoke-Command -NoNewScope -ScriptBlock {scriptBlock}) in {RepositoryName}");
                newPs?.Dispose();
            }
        }

        public LogItem CleanAndBuildResult<TParameter>(PSDataCollection<PSObject> result, string? command, TParameter parameter)
        {
            var results = string.Join(Environment.NewLine, result.Select(i => $"{i.Properties["name"]} ({i.Properties["version"]})").ToList());

            results = new Regex(@"^[^a-zA-Z0-9]*(?=[a-zA-Z0-9_.\-])").Replace(results, "").Trim();

            var log = results.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return BuildResults(command, parameter, log);
        }

        private void Runspace_StateChanged(object? sender, System.Management.Automation.Runspaces.RunspaceStateEventArgs e)
        {
            throw new NotImplementedException();
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
                        ? line[versionIndex..Math.Min(afterVersionIndex, line.Length)]
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

    public class WinRepoPSHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
        public override Size BufferSize
        {
            get => new Size(Console.BufferWidth, Console.BufferHeight);
            set
            {
                var (width, height) = (value.Width, value.Height);

                Console.BufferHeight = height;
                Console.BufferWidth = width;
            }
        }
        public override Coordinates CursorPosition
        {
            get => new Coordinates(Console.CursorLeft, Console.CursorTop);
            set
            {
                var (left, top) = (value.X, value.Y);

                Console.CursorLeft = left;
                Console.CursorTop = top;
            }
        }
        public override int CursorSize { get => Console.CursorSize; set => Console.CursorSize = value; }
        public override ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }

        public override bool KeyAvailable => Console.KeyAvailable;

        public override Size MaxPhysicalWindowSize => new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);

        public override Size MaxWindowSize => MaxPhysicalWindowSize;

        public override Coordinates WindowPosition
        {
            get => new Coordinates(Console.WindowLeft, Console.WindowTop);
            set
            {
                var (left, top) = (value.X, value.Y);

                Console.WindowLeft = left;
                Console.WindowTop = top;
            }
        }
        public override Size WindowSize
        {
            get => new Size(Console.WindowWidth, Console.WindowHeight);
            set
            {
                var (width, height) = (value.Width, value.Height);

                Console.WindowHeight = height;
                Console.WindowWidth = width;
            }
        }
        public override string WindowTitle { get => Console.Title ?? "<untitled>"; set => Console.Title = value; }

        public override void FlushInputBuffer()
        {
            Console.In.ReadToEnd();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            var text = ConsoleReader
                .ReadFromBuffer((short)rectangle.Left,
                    (short)rectangle.Top,
                    (short)(rectangle.Right - rectangle.Left),
                    (short)(rectangle.Top - rectangle.Bottom))
                .SelectMany(s => s.ToCharArray())
                .ToImmutableArray();

            var result = new BufferCell[rectangle.Right - rectangle.Left, rectangle.Top - rectangle.Bottom];

            for (int i = 0; i < text.Length; ++i)
            {
                var y = i / (rectangle.Top - rectangle.Bottom);
                var x = i % (rectangle.Top - rectangle.Bottom);

                result[x, y] = new BufferCell()
                {
                    Character = text[i],
                    ForegroundColor = ForegroundColor,
                    BackgroundColor = BackgroundColor
                };
            }

            return result;
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            var key = Console.ReadKey();
            return new KeyInfo(key.KeyChar, key.KeyChar, CalculateKeyState(key), false);
        }

        private ControlKeyStates CalculateKeyState(ConsoleKeyInfo key)
        {
            ControlKeyStates result = default;

            if ((key.Modifiers | ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            {
                result |= ControlKeyStates.RightAltPressed;
                result |= ControlKeyStates.LeftAltPressed;
            }

            if ((key.Modifiers | ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                result |= ControlKeyStates.LeftCtrlPressed;
                result |= ControlKeyStates.RightCtrlPressed;
            }

            if ((key.Modifiers | ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            {
                result |= ControlKeyStates.ShiftPressed;
            }

            return result;
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }
    }

    public class WinRepoPSHostUserInterface : PSHostUserInterface
    {
        public bool IsVerbose { get; set; } = false;

        public override PSHostRawUserInterface RawUI => new WinRepoPSHostRawUserInterface();

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return new();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return -1;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            Console.WriteLine(caption);
            var user = userName;
            Console.Write(message);
            return new(user, ReadLineAsSecureString());
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return PromptForCredential(caption, message, userName, targetName);
        }

        public override string ReadLine()
        {
            return Console.ReadLine() ?? "";
        }

        public override SecureString ReadLineAsSecureString()
        {
            var pw = ReadLine();
            var ss = new SecureString();
            for (int i = 0; i < pw.Length; ++i)
            {
                ss.InsertAt(i, pw[i]);
            }

            return ss;
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(value);
        }

        public override void Write(string value)
        {
            Console.Write(value);
        }

        public override void WriteDebugLine(string message)
        {
            Debug.WriteLine(message);
        }

        public override void WriteErrorLine(string value)
        {
            Console.Error.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            Console.Out.WriteLine(value);
        }

        ConcurrentDictionary<long, ProgressRecord> Progress = new();

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            Progress.AddOrUpdate(sourceId, record, (_, _) => record);
        }

        public override void WriteVerboseLine(string message)
        {
            Debug.WriteLine(message);
            if (IsVerbose)
            {
                var f = Console.ForegroundColor;
                var b = Console.BackgroundColor;

                Write(ConsoleColor.Green, b, $"VERBOSE: {message}");
                Write(f, b, "");
                WriteLine();
            }
        }

        public override void WriteWarningLine(string message)
        {
            var f = Console.ForegroundColor;
            var b = Console.BackgroundColor;

            Write(ConsoleColor.Yellow, ConsoleColor.DarkBlue, "WARNING:");
            Write(f, b, " ");
            WriteLine(message);
        }
    }

    public class WinRepoHost : PSHost
    {
        private Guid? _guid;
        private WinRepoPSHostUserInterface _ui;

        public override CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        public override Guid InstanceId => _guid ??= Guid.NewGuid();

        public override string Name => nameof(WinRepoHost);

        public override PSHostUserInterface? UI => _ui ??= new WinRepoPSHostUserInterface();

        public override Version Version => new Version("1.0");

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
            Environment.ExitCode = exitCode;
        }
    }

    // https://stackoverflow.com/questions/12355378/read-from-location-on-console-c-sharp
    public class ConsoleReader
    {
        public static IEnumerable<string> ReadFromBuffer(short x, short y, short width, short height)
        {
            IntPtr? bfr = Marshal.AllocHGlobal(width * height * Marshal.SizeOf(typeof(CHAR_INFO)));
            if (bfr is null || bfr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            IntPtr buffer = bfr.Value;

            try
            {
                COORD coord = new COORD();
                SMALL_RECT rc = new SMALL_RECT();
                rc.Left = x;
                rc.Top = y;
                rc.Right = (short)(x + width - 1);
                rc.Bottom = (short)(y + height - 1);

                COORD size = new COORD();
                size.X = width;
                size.Y = height;

                const int STD_OUTPUT_HANDLE = -11;
                if (!ReadConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), buffer, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                IntPtr ptr = buffer;
                for (int h = 0; h < height; h++)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int w = 0; w < width; w++)
                    {
                        CHAR_INFO ci = (CHAR_INFO)Marshal.PtrToStructure(ptr, typeof(CHAR_INFO));
                        char[] chars = Console.OutputEncoding.GetChars(ci.charData);
                        sb.Append(chars[0]);
                        ptr += Marshal.SizeOf(typeof(CHAR_INFO));
                    }
                    yield return sb.ToString();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHAR_INFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] charData;
            public short attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, IntPtr lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
    }
}
