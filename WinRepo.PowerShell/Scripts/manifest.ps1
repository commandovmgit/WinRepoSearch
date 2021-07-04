Import-Module PowerShellGet

$manifest = @{
	Path 			    = '.\Scripts\WinRepo.psd1'
	Author 			    = 'The Sharp Ninja'
	CompanyName 		= 'gatewayprogramming.school'
	Copyright 		    = '(c)2021 Gateway Programming School, Inc. All rights reserved.'
	Description 		= 'Search across WinGet, Scoop and Chocolatey simultaneously.'
	GUID 			    = '4f98a963-22fc-4d95-967b-4b3cbd5d799d'
    ModuleVersion 		= '1.0.0'
	PowerShellVersion 	= '7.0'
	FunctionsToExport 	= 'Search-WinRepoRepositories', 'Install-WinRepoRepositories', 'Show-WinRepoRepositories', 'List-WinRepoRepositories', 'UnInstall-WinRepoRepositories', 'Upgrade-WinRepoRepositories'
	RootModule 		    = '.\Scripts\WinRepo.psd1'
    ProcessorArchitecture = 'Amd64'
    ProjectUri          = 'https://github.com/gatewayprogrammingschool/WinRepoSearch'
    RequireLicenseAcceptance = $false
    LicenseUri          = 'https://github.com/gatewayprogrammingschool/WinRepoSearch/blob/223ece8706e62e767e488f4e100b75c900744a68/WinRepo.PowerShell/LICENSE.md'
    Tags                = @('WinGet', 'Scoop', 'Chocolatey', 'Automation')
    #Icon                = '.\WinRepo-small.png'
}

if(test-path .\WinRepo.psd1) {
    $result = update-ModuleManifest @manifest
    Write-Host "Updated $result"
} else {
    $result = new-ModuleManifest @manifest
    Write-Host "Created $result"
}

$manifest = @{
    Author                   = 'The Sharp Ninja'
    CompanyName              = 'gatewayprogramming.school'
    Copyright                = '(c)2021 Gateway Programming School, Inc. All rights reserved.'
    Description              = 'Search across WinGet, Scoop and Chocolatey simultaneously.'
    GUID                     = '4f98a963-22fc-4d95-967b-4b3cbd5d799d'
    ProjectUri               = 'https://github.com/gatewayprogrammingschool/WinRepoSearch'
    LicenseUri               = 'https://github.com/gatewayprogrammingschool/WinRepoSearch/blob/223ece8706e62e767e488f4e100b75c900744a68/WinRepo.PowerShell/LICENSE.md'
    Tags                     = @('WinGet', 'Scoop', 'Chocolatey', 'Automation')
    #Icon                = '.\WinRepo-small.png'
}

# New-ScriptFileInfo @manifest -PassThru test.ps1
