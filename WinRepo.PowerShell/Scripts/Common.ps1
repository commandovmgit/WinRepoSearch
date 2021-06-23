using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\CommunityToolkit.Mvvm.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.ApplicationInsights.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.Abstractions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.Binder.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.CommandLine.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.EnvironmentVariables.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.FileExtensions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.Json.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Configuration.UserSecrets.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.DependencyInjection.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.FileProviders.Abstractions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.FileProviders.Physical.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.FileSystemGlobbing.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Hosting.Abstractions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Hosting.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.Abstractions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.Configuration.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.Console.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.Debug.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.EventLog.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Logging.EventSource.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Options.ConfigurationExtensions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Options.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Extensions.Primitives.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Win32.Registry.AccessControl.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Microsoft.Win32.SystemEvents.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Newtonsoft.Json.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.CodeDom.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Configuration.ConfigurationManager.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Diagnostics.EventLog.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.DirectoryServices.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Drawing.Common.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Management.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Security.Cryptography.Pkcs.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Security.Cryptography.ProtectedData.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Security.Permissions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\System.Windows.Extensions.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepo.PowerShell.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepoSearch.Core.dll"
using assembly "C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\YamlDotNet.dll"

using namespace WinRepo
using namespace WinRepo.PowerShell
using namespace WinRepoSearch.Core

$moduleLocation = (Get-Module WinRepo.PowerShell)?.Path `
    ?? 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepo.PowerShell.dll'

Import-Module $moduleLocation -Force

[WinRepo.PowerShell.Startup]$startup = [Module]::GetStartup()
[System.IServiceProvider]$serviceProvider = $startup.ServiceProvider;
