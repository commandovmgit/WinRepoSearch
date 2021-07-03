$moduleLocation = (Get-Module WinRepo.PowerShell).Path `
    ?? 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0\WinRepo.PowerShell.dll'

Write-Verbose "`$moduleLocation: [$moduleLocation]"

if (-not (Test-Path $moduleLocation)) { throw "Cannot find module at: $moduleLocation" }

$moduleLoaded = -not (Get-Module WinRepo)

#Remove-Module $moduleLocation -ErrorAction SilentlyContinue
if (-not $moduleLoaded) {
    Import-Module $moduleLocation -Force -ErrorAction SilentlyContinue
}
Push-Location

try {
    if (-not (Test-Path .\WinRepos.psm1)) {
        $defaultPath = 'C:\GitHub\WinRepoSearch\WinRepo.PowerShell\bin\Debug\net5.0'

        Set-Location $defaultPath

        Write-Verbose "Changed location to $defaultPath"
    }

    if (-not $moduleLoaded) {
        $startup = [Module]::iStartup;
        [System.IServiceProvider]$serviceProvider = $startup.ServiceProvider;

        Write-Output $startup, $serviceProvider
    }
}
finally {
    Pop-Location
}
