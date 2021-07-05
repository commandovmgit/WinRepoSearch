Write-Verbose (Get-Location)

$moduleLocation = (Get-Module 'WinRepo.Powershell').Path `
    ?? '..\WinRepo.PowerShell.dll'

$moduleLocation = Resolve-Path $moduleLocation

Write-Verbose "`$moduleLocation: [$moduleLocation]"

if (-not (Test-Path $moduleLocation)) {
    Push-Location
    Set-Location ..
    $item = Get-ChildItem WinRepo.PowerShell.dll -Recurse

    $moduleLocation = $item | Where-Object -Property FullName -NotMatch '\\ref\\' | Select-Object -First 1
    if (-not $moduleLocation) {
        throw 'Could not locate WinRepo.PowerShell.dll.'
    }
}

$moduleLocation = (Resolve-Path $moduleLocation).Path

$bin = [System.IO.Path]::GetDirectoryName($moduleLocation)

Write-Verbose "`$bin: $bin"

Set-Location $bin

if (-not (Test-Path $PSScriptRoot\NewAssemblies.ps1)) {
    $assemblies = Get-Content $PSScriptRoot\Assemblies.ps1

    $newAssemblies = @()
    $assemblies | ForEach-Object -Process {
        $assembly = $_.Replace('||PATH||', $bin)
        $newAssemblies += $assembly
    }

    $newAssemblies | Out-File -FilePath $PSScriptRoot\NewAssemblies.ps1 -Encoding utf8 -Force
}

. $PSScriptRoot\NewAssemblies.ps1

Pop-Location

$moduleLoaded = -not (Get-Module WinRepo)

#Remove-Module $moduleLocation -ErrorAction SilentlyContinue
if (-not $moduleLoaded) {
    Write-Verbose "Importing module: $moduleLocation"
    Import-Module $moduleLocation -Force -ErrorAction Stop
}
Push-Location

try {
    if (-not (Test-Path $PSScriptRoot\WinRepo.psm1) -and -not(Test-Path $PSScriptRoot\Scripts\WinRepo.psm1)) {

        Write-Host "I don't know where I am."

        Write-Host $PSScriptRoot
        Get-ChildItem $PSScriptRoot

        throw 'Panicked!'
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
