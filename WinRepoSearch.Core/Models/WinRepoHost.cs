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

    public sealed class WinRepoHost : PSHost
    {
        private ILogger Logger {get;}

        public WinRepoHost(ILogger logger) : base()
        {
            Logger = logger;
            _ui = new WinRepoPSHostUserInterface(Logger);
        }

       private Guid? _guid;

        private readonly WinRepoPSHostUserInterface _ui;

        public override CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        public override CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        public override Guid InstanceId => _guid ??= Guid.NewGuid();

        public override string Name => nameof(WinRepoHost);

        public override PSHostUserInterface? UI => _ui;

        public override Version Version => new ("1.0");

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
}
