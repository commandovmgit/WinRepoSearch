using namespace System.Collections;
using namespace System.Collections.Generic;

Import-Module Pester

$location = Get-Location


Describe 'Search-Repositories' {
    Context "Search All for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            . 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\Assemblies.ps1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module

            # Assertion
            $result = Search-WinRepoRepositories -Query vscode -Repo 'All' -Verbose

            $count = $result.Length;

            $count | Should -BeGreaterThan 0

            Write-Verbose ("`$result.Length: $count")
        }
    }

    Context "Search Scoop for 'vscode'" {
        It 'should return results' {
            $module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

            . 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\Assemblies.ps1'

            Remove-Module $module -ErrorAction SilentlyContinue
            Import-Module $module

            # Assertion
            $result = Search-WinRepoRepositories -Query vscode -Repo 'Scoop' -Verbose

            $count = $result.Length;

            $count | Should -BeGreaterThan 0

            Write-Verbose ("`$result.Length: $count")

        }
    }
}
