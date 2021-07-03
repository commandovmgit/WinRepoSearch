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

    public sealed class WinRepoPSHostUserInterface : PSHostUserInterface
    {
        private ILogger Logger {get;}

        public WinRepoPSHostUserInterface(ILogger logger)
        {
            Logger = logger;
        }

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
            Logger.LogDebug(caption);
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
            Logger.LogDebug(message);
        }

        public override void WriteErrorLine(string value)
        {
            Console.Error.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            Console.Out.WriteLine(value);
        }

        private readonly ConcurrentDictionary<long, ProgressRecord> Progress = new();

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            Progress.AddOrUpdate(sourceId, record, (_, _) => record);
        }

        public override void WriteVerboseLine(string message)
        {
            Logger.LogDebug(message);
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
}
