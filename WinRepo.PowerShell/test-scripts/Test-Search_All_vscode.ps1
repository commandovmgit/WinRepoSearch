$module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

. 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\Assemblies.ps1'

Remove-Module $module -ErrorAction SilentlyContinue
Import-Module $module

# Assertion
$result = Search-WinRepoRepositories -Query 'blazor' -Repo 'WinGet' -Verbose

$count = $result.Length;

$count | Should -BeGreaterThan 0

Write-Verbose ("`$result.Length: $count")
