using namespace System.Collections;
using namespace System.Collections.Generic;

import-module Pester

. '..\Scripts\Search-Repository.ps1'

Describe "Search-Repositories" {
    Context "Search All for 'vscode'" {
        It "should return results" {
            # Assertion
            $result = Search-Repositories -Query vscode -Repo All

            $result | Should -Exist `
                    | Should -BeOfType [IEnumerable[PSObject]]

            $count = $result.Count();

            $count | Should -BeGreaterThan 0

            Write-Information ("`$result.Count(): $count")
        }
    }

    Context "Search Scoop for 'vscode'" {
        It 'should return results' {
            # Assertion
            $result = Search-Repositories -Query vscode -Repo Scoop

            $result | Should -Exist `
                    | Should -BeOfType [IEnumerable[PSObject]]

            $count = $result.Count();

            $count | Should -BeGreaterThan 0

            Write-Information ("`$result.Count(): $count")
        }
    }
}
