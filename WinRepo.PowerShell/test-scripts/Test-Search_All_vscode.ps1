$module = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\Scripts\WinRepo.psm1'

Remove-Module $module -ErrorAction SilentlyContinue
Import-Module $module

# Assertion
Search-WinRepoRepositories -Query 'vscode' -Repo 'All' -Verbose `
    | Sort-Object -Property repo, name `
    | Format-Table -GroupBy repo -Property name, version, repo
