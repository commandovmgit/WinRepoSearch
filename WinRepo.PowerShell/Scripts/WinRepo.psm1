
<#PSScriptInfo

.VERSION 1.0

.GUID 4f98a963-22fc-4d95-967b-4b3cbd5d799d

.AUTHOR The Sharp Ninja

.COMPANYNAME gatewayprogramming.school

.COPYRIGHT (c)2021 Gateway Programming School, Inc. All rights reserved.

.TAGS WinGet Scoop Chocolatey Automation

.LICENSEURI https://github.com/gatewayprogrammingschool/WinRepoSearch/blob/223ece8706e62e767e488f4e100b75c900744a68/WinRepo.PowerShell/LICENSE.md

.PROJECTURI https://github.com/gatewayprogrammingschool/WinRepoSearch

.ICONURI

.EXTERNALMODULEDEPENDENCIES

.REQUIREDSCRIPTS

.EXTERNALSCRIPTDEPENDENCIES

.RELEASENOTES


.PRIVATEDATA

#>

<#

.DESCRIPTION
 Search across WinGet, Scoop and Chocolatey simultaneously.

#>

using namespace WinRepo
using namespace WinRepo.PowerShell
using namespace WinRepoSearch.Core.Models
using namespace System.IO

function Initialize {
    Write-Verbose "Initialize - Entered"

    if (Test-Path "..\bin\Debug\net5.0\Scripts") {
        Set-Location "..\bin\Debug\net5.0\Scripts"
    }

    $location = (Get-Location).Path;

    if (-not (Test-Path 'Common.ps1')) {
        $parts = $location.Split('\')
        for ($i = $parts.Length - 1; $i -gt 0; $i -= 1) {
            $path = [System.String]::Join("\", $parts[0..($i - 1)])
            Write-Verbose "Initialize - `$path: $path"
            Set-Location $path
            if (Test-Path common.ps1) {
                $common_ps1 = Get-ChildItem common.ps1 -Recurse
                Write-Verbose "Initialize - `$common_ps1: $common_ps1"
                if ($common_ps1) { break }
            } 
        }

        if ($common_ps1) {
            Write-Verbose "Initialize - Found common.ps1 at $common_ps1"
            $array = @()
            $array += $common_ps1
            $location = [Path]::GetDirectoryName($array[0].FullName)
            Set-Location $location
        }
    }

    Write-Verbose "Initialize - `$location: $location"

    Set-Location $location

    $commonPath = Join-Path . -ChildPath common.ps1

    Write-Verbose "Initialize - `$commonPath: $commonPath"

    $found = Test-Path $commonPath

    Write-Verbose "Initialize - `$found: $found"

    if (-not $found) {
        $commonPath = Join-Path $PSScriptRoot -ChildPath common.ps1

        Write-Verbose "Initialize - `$commonPath: $commonPath"

        $found = Test-Path $commonPath
    }

    Write-Verbose "Initialize - `$found: $found"

    if ($found) {

        Write-Verbose ("Get-Location: " + (Get-Location))

        $initialized = (. $commonPath)

        Write-Verbose "Initialize - `$initialized: $initialized"
        Write-Verbose "Initialize - Leaving"

        return $initialized;
    }
    else {
        throw ("Cannot find Common.ps1.  Current location: " + (Get-Location))
    }
}

function Search-WinRepoRepositories {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Query,
        [Parameter(Mandatory = $false)]$Repo = 'All'
    )
    Write-Verbose "Search-WinRepoRepositories - Entered"

    $initialized = Initialize

    Write-Verbose "`$initialized: $initialized"

    $r = Search-WinRepoRepositories_Inner -init:$initialized -Query:$Query -Repo:$Repo

    if ($r.Count -eq 0) {
        Write-Host "No results for $Query in $Repo"
    }

    Write-Verbose "Search-WinRepoRepositories - Leaving"
    return $r
}

function Get-WinRepoRepositories {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Query,
        [Parameter(Mandatory = $false)]$Repo = 'All'
    )
    Write-Verbose "Get-WinRepoRepositories - Entered"

    $initialized = Initialize

    Write-Verbose "`$initialized: $initialized"

    $r = Get-WinRepoRepositories_Inner -init:$initialized -Query:$Query -Repo:$Repo

    if ($r.Count -eq 0) {
        Write-Host "No results for $Query in $Repo"
    }

    Write-Verbose "Get-WinRepoRepositories - Leaving"
    return $r
}

function Search-WinRepoRepositories_Inner {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$init,
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $false)]$Repo = 'All'
    )
    Write-Verbose "Search-WinRepoRepositories_Inner - Entered"

    if (-not $init) {
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
            $continue = $true;
            if (-not $_) {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $_ is $null'; 
                $continue = $false;
            }
            else {
                [WinRepoSearch.Core.Models.Repository]$repository = $_;
            }

            if ($continue -and -not $repository) {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $repository is $null'; 
                $continue = $false;
            }
            else {
            }
            if ($continue -and $repository -eq '') {
                Write-Verbose 'Search-WinRepoRepositories_Inner - $repository is empty'; 
                $continue = $false;
            }

            if ($continue) {
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
                        try {
                            $result = $block.Invoke()
                        }
                        catch {
                            Write-Verbose $_
                            $result = @();
                            return $result;
                        }

                        $count = $result.Length
                        Write-Verbose "Search-WinRepoRepositories_Inner - `$result.Length: $count"

                        switch ($repository.RepositoryName) {
                            { $_ -ieq 'scoop' } {
                                $result | ForEach-Object -Process {
                                    Write-Verbose "Search-WinRepoRepositories_Inner - ${repository.RepositoryName} - ${_.name} - ${_.version}"

                                    Add-Member -Name 'id' `
                                        -MemberType NoteProperty `
                                        -Value $_.name `
                                        -InputObject $_

                                    Add-Member -Name 'repo' `
                                        -MemberType NoteProperty `
                                        -Value $repository.RepositoryName `
                                        -InputObject $_
                                }
                            }

                            default {
                                $log = $result

                                Write-Verbose "Search-WinRepoRepositories_Inner - `$result: $result"

                                [WinRepoSearch.Core.Models.LogItem]$logItem = $repository.CleanAndBuildResult($result, 'search', $Query)

                                Write-Verbose "Search-WinRepoRepositories_Inner - `$logItem.Result: $logItem.Result"

                                $ra = @()
                                $ra += $logItem.Result

                                $array = @()
                                foreach ($searchResult in $ra) {
                                    if ([System.String]::IsNullOrWhitespace($searchResult.appName)) {
                                        $logItem.Result.Remove($searchResult);
                                        continue
                                    }

                                    $isValid = $searchResult.appId -imatch '\b[a-zA-Z0-9._-]+\b'
                                    if ($isValid) {

                                        $item = New-Object PSObject -Property @{
                                            name    = $searchResult.appName.Trim()
                                            id      = $searchResult.appId.Trim()
                                            repo    = $searchResult.Repo.RepositoryName.Trim()
                                            version = $searchResult.AppVersion.Trim()
                                        }

                                        Write-Verbose "Search-WinRepoRepositories_Inner - `$item: $item"
                                        $array += $item;
                                    }
                                    else {
                                        #$resultArray.Remove($searchResult) | Out-Null
                                    }
                                }

                                $result = $array;
                            }
                        }

                        $resultArray += $result;
                        $count = $resultArray.Length
                        Write-Verbose "Search-WinRepoRepositories_Inner - `$resultArray.Length: $count"
                    }
                    catch {
                        Write-Verbose "Search-WinRepoRepositories_Inner - $_";
                        throw $_;
                    }
                }
            }
        }

        Write-Output $resultArray
        Write-Verbose "Search-WinRepoRepositories_Inner - Completed search for $Query in $Repo"
    }
    catch {
        Write-Verbose "Search-WinRepoRepositories_Inner - Error: $_";
        throw $_;
    }
    finally {
        Write-Verbose "Search-WinRepoRepositories_Inner - Leaving"
    }
}

function Get-WinRepoRepositories_Inner {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$init,
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $false)]$Repo = 'All'
    )
    Write-Verbose "Get-WinRepoRepositories_Inner - Entered"

    if ($Repo -eq 'All') { throw "Must use single repository." }

    if (-not $init) {
        throw "-init is `$null!"
    }

    if (-not $Query) {
        throw '-Query cannot be null or empty.'
    }

    try {
        $startup = $initialized[0]
        $serviceProvider = $initialized[1]

        $repos = $startup.SearchService.Repositories

        Write-Verbose "Get-WinRepoRepositories_Inner - `$Repo: $Repo"

        $resultArray = @()

        $repos | ForEach-Object -End {
            Write-Verbose "Get-WinRepoRepositories_Inner - Completed ForEach-Object"
        } -Process {
            if (-not $_) {
                Write-Verbose 'Get-WinRepoRepositories_Inner - $_ is $null'; continue
            }

            [WinRepoSearch.Core.Models.Repository]$repository = $_;

            if (-not $repository) {
                Write-Verbose 'Get-WinRepoRepositories_Inner - $repository is $null'; continue
            }

            if ($repository -eq '') {
                Write-Verbose 'Get-WinRepoRepositories_Inner - $repository is empty'; continue
            }

            Write-Verbose ("Get-WinRepoRepositories_Inner - `$repository.RepositoryName: " + $repository.RepositoryName)

            if ($Repo -ieq $repository.RepositoryName) {
                $command = $repository.Command
                $infoCmd = $repository.InfoCmd

                Write-Verbose "Get-WinRepoRepositories_Inner - `$command [$command]"
                Write-Verbose "Get-WinRepoRepositories_Inner - `$infoCmd: [$infoCmd]"

                $cmd = [string]::Format($infoCmd, $Query)

                if (-not $cmd.EndsWith("--PSObject") -and $Repo -ieq 'scoop') {
                    $cmd = "$cmd '--PSObject'"
                }

                Write-Verbose "Get-WinRepoRepositories_Inner - `$cmd: [$cmd]"

                $block = [Scriptblock]::Create("& $command $cmd")

                try {
                    try {
                        $result = $block.Invoke()
                    }
                    catch {
                        Write-Verbose $_
                        $result = @();
                        return $result;
                    }

                    $count = $result.Length
                    Write-Verbose "Get-WinRepoRepositories_Inner - `$result.Length: $count"

                    switch ($repository.RepositoryName) {
                        { $_ -ieq 'scoop' } {
                            $result | ForEach-Object -Process {
                                Write-Verbose "Get-WinRepoRepositories_Inner - ${repository.RepositoryName} - ${_.name} - ${_.version}"

                                Add-Member -Name 'id' `
                                    -MemberType NoteProperty `
                                    -Value $_.name `
                                    -InputObject $_

                                Add-Member -Name 'repo' `
                                    -MemberType NoteProperty `
                                    -Value $repository.RepositoryName `
                                    -InputObject $_
                            }
                        }

                        default {
                            $result

                            Write-Verbose "Get-WinRepoRepositories_Inner - `$result: $result"
                        }
                    }

                    $resultArray += $result;
                    $count = $resultArray.Length
                    Write-Verbose "Get-WinRepoRepositories_Inner - `$resultArray.Length: $count"
                }
                catch {
                    Write-Verbose "Get-WinRepoRepositories_Inner - $_";
                    throw $_;
                }
            }
        }

        Write-Output $resultArray
        Write-Verbose "Get-WinRepoRepositories_Inner - Completed search for $Query in $Repo"
    }
    catch {
        Write-Verbose "Get-WinRepoRepositories_Inner - Error: $_";
        throw $_;
    }
    finally {
        Write-Verbose "Get-WinRepoRepositories_Inner - Leaving"
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
