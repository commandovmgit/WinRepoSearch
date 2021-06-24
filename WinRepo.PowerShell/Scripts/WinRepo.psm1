using namespace WinRepo
using namespace WinRepo.PowerShell
using namespace WinRepoSearch.Core.Models
using namespace System.IO

function Initialize {
    Write-Verbose "Initialize - Entered"

    $location = (Get-Location).Path;

    if(-not (Test-Path 'Common.ps1')) {
        $parts = $location.Split('\')
        for($i=$parts.Length-1; $i -gt 0; $i-=1) {
            $path = [System.String]::Join("\", $parts[0..($i-1)])
            Write-Verbose "Initialize - `$path: $path"
            Set-Location $path
            $common_ps1 = Get-ChildItem common.ps1 -Recurse
            Write-Verbose "Initialize - `$common_ps1: $common_ps1"
            if ($common_ps1) { break }
        }

        if($common_ps1) {
            Write-Verbose "Initialize - Found common.ps1 at $common_ps1"
            $array = @()
            $array += $common_ps1
            $location = [Path]::GetDirectoryName($array[0].FullName)
            Set-Location $location
            Write-Verbose "Initialize - `$location: $location"
        } else {
            Write-Verbose "Initialize - Common.ps1 Not found!"
        }
    }

    Write-Verbose "Initialize - `$location: $location"

    $initialized = (. '.\Common.ps1')

    Write-Verbose "Initialize - `$initialized: $initialized"
    Write-Verbose "Initialize - Leaving"

    return $initialized;
}

function Search-WinRepoRepositories {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Query,
        [Parameter(Mandatory = $false)]$Repo = $null
    )
    Write-Verbose "Search-WinRepoRepositories - Entered"

    $initialized = Initialize

    $r = Search-WinRepoRepositories_Inner -init:$initialized -Query:$Query -Repo:$Repo

    Write-Verbose "Search-WinRepoRepositories - Leaving"
    return $r
}

function Search-WinRepoRepositories_Inner {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$init,
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $false)][WinRepo.PowerShell.DefaultRepos]$Repo = 'All'
    )
    Write-Verbose "Search-WinRepoRepositories_Inner - Entered"

    if(-not $init) {
        throw "-init is `$null!"
    }

    if (-not $Query) {
        throw '-Query cannot be null or empty.'
    }

    try {
        $startup = $initialized[0]
        $serviceProvider = $initialized[1]

        $repos = $startup.SearchService.Repositories

        Write-Verbose "Search-WinRepoRepositories_Inner - `$Repo: $Repo"

        $resultArray = @()

        $repos | ForEach-Object -End {
            Write-Verbose "Search-WinRepoRepositories_Inner - Completed ForEach-Object"
        } -Process {
            if (-not $_) {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $_ is $null'; continue
            }

            [WinRepoSearch.Core.Models.Repository]$repository = $_;

            if (-not $repository) {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $repository is $null'; continue
            }

            if($repository -eq '') {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $repository is empty'; continue
            }

            Write-Verbose ("Search-WinRepoRepositories_Inner - `$repository.RepositoryName: " + $repository.RepositoryName)

            if ($Repo -ieq 'All' -or $Repo -ieq $repository.RepositoryName) {
                $command = $repository.Command
                $searchCmd = $repository.SearchCmd

                Write-Verbose "Search-WinRepoRepositories_Inner - `$command [$command]"
                Write-Verbose "Search-WinRepoRepositories_Inner - `$searchCmd: [$searchCmd]"

                $cmd = [string]::Format($searchCmd, $Query)

                if (-not $cmd.EndsWith("--PSObject") -and $Repo -ieq 'scoop') {
                    $cmd = "$cmd '--PSObject'"
                }

                Write-Verbose "Search-WinRepoRepositories_Inner - `$cmd: [$cmd]"

                $block = [Scriptblock]::Create("& $command $cmd")

                try {
                    $result = $block.Invoke()

                    $count = $result.Length
                    Write-Verbose "Search-WinRepoRepositories_Inner - `$result.Length: $count"

                    switch($repository.RepositoryName) {
                        (-ieq 'scoop') {
                            $result | ForEach-Object -Process {
                                Write-Verbose "Search-WinRepoRepositories_Inner - ${repository.RepositoryName} - ${_.name} - ${_.version}"

                                Add-Member -Name 'repo' `
                                           -MemberType NoteProperty `
                                           -Value $repository.RepositoryName `
                                           -InputObject $_
                            }
                        }

                        default {
                            $log = $result

                            [WinRepoSearch.Core.Models.LogItem]$logItem = $repository.CleanAndBuildResult($result, 'search', $Query)

                            foreach($searchResult in $logItem.Results)
                            {
                                $item = New-Object PSObject -ArgumentList {
                                    name = $searchResult.appName
                                    id = $searchResult.appId
                                    repo = $searchResult.Repo.RepositoryName
                                    version = $searchResult.AppVersion
                                }
                            }
                        }
                    }

                    $resultArray += $result;
                    $count = $resultArray.Length
                    Write-Verbose "Search-WinRepoRepositories_Inner - `$resultArray.Length: $count"
                } catch {
                    Write-Verbose "Search-WinRepoRepositories_Inner - $_";
                    throw $_;
                }
            }
        }

        Write-Output $resultArray
        Write-Verbose "Search-WinRepoRepositories_Inner - Completed search for $Query in $Repo"
    } catch {
        Write-Verbose "Search-WinRepoRepositories_Inner - Error: $_";
        throw $_;
    } finally {
        Write-Verbose "Search-WinRepoRepositories_Inner - Leaving"
    }
}

function Install-WinRepoPackage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $false)][WinRepo.PowerShell.DefaultRepos]$Repo = 'All'
    )

    if (-not $Query) {
        throw '-Query cannot be null or empty.'
    }

    $initialized = Initialize

    $startup = $initialized[0]
    $serviceProvider = $initialized[1]

    $repos = $startup.SearchService.Repositories

    Write-Host $repos

    $resultArray = @()

    $repos | ForEach-Object -Process {
        if (-not $_) {
            Write-Verbose '$_ is $null'; continue
        }

        [WinRepoSearch.Core.Models.Repository]$repository = $_;

        if (-not $repository) {
            Write-Verbose '$repository is $null'; continue
        }

        if ($repository -eq '') {
            Write-Verbose '$repository is empty'; continue
        }

        if ($Repo -eq 'All' -or $Repo -eq $repository.RepositoryName) {
            $command = $repository.Command
            $searchCmd = $repository.SearchCmd
            Write-Verbose "`$command [$command]"
            Write-Verbose "`$searchCmd: [$searchCmd]"
            $cmd = [string]::Format($searchCmd, $Query)
            if (-not $cmd.EndsWith('--PSObject')) {
                $cmd = "$cmd '--PSObject'"
            }
            Write-Verbose "`$cmd: [$cmd]"
            $block = [Scriptblock]::Create("& $command $cmd")
            $result = $block.Invoke()

            $resultArray += $result;
        }
    }

    $resultArray
}
