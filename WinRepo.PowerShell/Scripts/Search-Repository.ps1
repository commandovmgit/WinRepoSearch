using assembly '..\..\WinRepoSearch.Core\bin\x64\Debug\net5.0\WinRepoSearch.Core.dll'

using namespace WinRepo
using namespace WinRepo.PowerShell
using namespace WinRepoSearch.Core

. "$PSScriptRoot\Common.ps1"

function Search-Repositories {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $false)][DefaultRepo]$Repo = 'All'
    )

    if (-not $Query) {
        throw '-Query cannot be null or empty.'
    }

    $repos = $startup.SearchService.Repositories

    $resultArray = @()

    $repos | ForEach-Object -process {
        [Repository]$repository = $_;
        if ($Repo -eq 'All' -or $Repo -eq $repository.RepositoryName) {
            $cmd = [string]::Format($repository.SearchCmd, $repository.Command, $Query)
            if(-not $cmd.EndsWith("'--PSObject'")) { $cmd = "$cmd '--PSObject'"}
            $result = & { & $cmd }

            $resultArray += $result;
        }
    }
}
