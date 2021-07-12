using namespace System.Collections;
using namespace System.Collections.Generic;


. C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\NewAssemblies.ps1

Import-Module Pester

$location = Get-Location


Describe 'Search-WinRepoRepositories' {
    Context "Search All for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $table = Search-WinRepoRepositories -Query 'vscode' -Repo 'All' `
                | Sort-Object -Property repo, name `
                | Format-Table -Property name, version, repo

            $table | Should -Not -Be $null
            $table.Count | Should -not -be 0
        }
    }

    Context "Search Chocolatey for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $table = Search-WinRepoRepositories -Query 'vscode' -Repo 'Chocolatey' `
                | Sort-Object -Property repo, name `
                | Format-Table -Property name, version, repo

            $table | Should -Not -Be $null
            $table.Count | Should -not -be 0
        }
    }

    Context "Search Scoop for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $table = Search-WinRepoRepositories -Query 'vscode' -Repo 'Scoop' `
                | Sort-Object -Property repo, name `
                | Format-Table -Property name, version, repo

            $table | Should -Not -Be $null
            $table.Count | Should -not -be 0
        }
    }

    Context "Search Winget for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $table = Search-WinRepoRepositories -Query 'vscode' -Repo 'WinGet' `
                | Sort-Object -Property repo, name `
                | Format-Table -Property name, version, repo

            $table | Should -Not -Be $null
            $table.Count | Should -not -be 0
        }
    }
}

Describe 'Get-WinRepoRepositories' {
    Context "Get Info for 'gsudo' in WinGet" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $result = Get-WinRepoRepositories -Query 'gsudo' -Repo 'WinGet' `

            $t = [System.Type]([WinRepoSearch.Core.Services.SearchService])
            [Microsoft.Extensions.Hosting.IHost]$sh = [WinRepo.PowerShell.Module]::ServiceHost;
            [System.IServiceProvider]$sp = $sh.Services;
            $searchService = $sp.GetService($t);

            $repository = $searchService.Repositories `
                            | Where-Object -Property RepositoryName -Eq -Value 'WinGet' `
                            | Select-Object -First 1

            $searchResult = New-Object WinRepoSearch.Core.Models.SearchResult -ArgumentList $result, $repository

            $searchResult.AppId = 'gsudo'

            $repository.ParseDetails($result, $searchResult)

            $searchResult | Should -not -be $null
            $searchResult.AppDescription | Should -not -be $null
            $searchResult.PublisherName | Should -not -be $null
            $searchResult.PublisherWebsite | Should -not -be $null
            $searchResult.Notes | Should -not -be $null

            Write-Host $searchResult.ListMarkdown
        }
    }

    Context "Get Info for 'gsudo' in Chocolatey" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $result = Get-WinRepoRepositories -Query 'gsudo' -Repo 'Chocolatey' `

            $t = [System.Type]([WinRepoSearch.Core.Services.SearchService])
            [Microsoft.Extensions.Hosting.IHost]$sh = [WinRepo.PowerShell.Module]::ServiceHost;
            [System.IServiceProvider]$sp = $sh.Services;
            $searchService = $sp.GetService($t);

            $repository = $searchService.Repositories `
                            | Where-Object -Property RepositoryName -Eq -Value 'Chocolatey' `
                            | Select-Object -First 1

            $searchResult = New-Object WinRepoSearch.Core.Models.SearchResult -ArgumentList $result, $repository

            $repository.ParseDetails($result, $searchResult)

            $searchResult | Should -not -be $null
            $searchResult.AppDescription | Should -not -be $null
            $searchResult.PublisherName | Should -not -be $null
            $searchResult.PublisherWebsite | Should -not -be $null
            $searchResult.Notes | Should -not -be $null

            Write-Host $searchResult.ListMarkdown
        }
    }

    Context "Get Info for 'gsudo' in Scoop" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module
            
            # Assertion
            $result = Get-WinRepoRepositories -Query 'gsudo' -Repo 'Scoop' `

            $t = [System.Type]([WinRepoSearch.Core.Services.SearchService])
            [Microsoft.Extensions.Hosting.IHost]$sh = [WinRepo.PowerShell.Module]::ServiceHost;
            [System.IServiceProvider]$sp = $sh.Services;
            $searchService = $sp.GetService($t);

            $repository = $searchService.Repositories `
                            | Where-Object -Property RepositoryName -Eq -Value 'Scoop' `
                            | Select-Object -First 1

            $searchResult = New-Object WinRepoSearch.Core.Models.SearchResult -ArgumentList $result, $repository

            $repository.ParseDetails($result, $searchResult)

            $searchResult | Should -not -be $null
            $searchResult.AppDescription | Should -not -be $null
            $searchResult.PublisherWebsite | Should -not -be $null
            $searchResult.Notes | Should -not -be $null

            Write-Host $searchResult.ListMarkdown
        }
    }
}
