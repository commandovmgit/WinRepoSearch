$module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepo.PowerShell.dll'
$searchTerm = 'vscode'

import-module $module -Force

$results = Search-Repository -R Scoop -Q $searchTerm

$results | Format-Table

function Set-Startup {
                        params(
                            [WinRepoSearch.Core.Contracts.Services.IStartup]$s,
                            [System.IServiceProvider]$sp
                        )
                        $startup = $s;
                        Write-Verbose -Message "`$startup: $startup";
                        Write-Output $startup;
                        $startup.ServiceProvider = $sp;
                        Write-Verbose -Message "`$startup.ServiceProvider: $startup.ServiceProvider";
                        Write-Output $startup.ServiceProvider;
                    }