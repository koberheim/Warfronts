$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\HeadlessCheckPolicy.ps1")

function Assert-Equal {
    param(
        [Parameter(Mandatory = $true)]$Expected,
        [Parameter(Mandatory = $true)]$Actual,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if ($Expected -ne $Actual) {
        throw "$Message. Expected '$Expected'; got '$Actual'."
    }
}

$oneFailure = @([PSCustomObject]@{ Check = "mock"; Status = "FAIL"; Detail = "mock failure" })
Assert-Equal -Expected 1 -Actual (Get-HeadlessFailureCount -Results $oneFailure) `
    -Message "Exactly one failed result must produce a failure count of one"

$oneSkip = @([PSCustomObject]@{ Check = "mock"; Status = "SKIP"; Detail = "mock skip" })
Assert-Equal -Expected 1 -Actual (Get-HeadlessFailureCount -Results $oneSkip) `
    -Message "A skipped result must not be counted as green"

$zeroTests = Get-GoDotTestCheckResult -ExitCode 0 `
    -Output "Test results: Passed: 0 | Failed: 0 | Skipped: 0"
Assert-Equal -Expected "FAIL" -Actual $zeroTests.Status `
    -Message "A parsed zero-test suite must fail"

$skippedTests = Get-GoDotTestCheckResult -ExitCode 0 `
    -Output "Test results: Passed: 2 | Failed: 0 | Skipped: 1"
Assert-Equal -Expected "FAIL" -Actual $skippedTests.Status `
    -Message "A suite with skipped tests must fail"

$nonzeroSuite = Get-GoDotTestCheckResult -ExitCode 7 `
    -Output "Test results: Passed: 2 | Failed: 0 | Skipped: 0"
Assert-Equal -Expected "FAIL" -Actual $nonzeroSuite.Status `
    -Message "A suite with a nonzero native exit code must fail"

$validSuite = Get-GoDotTestCheckResult -ExitCode 0 `
    -Output "Test results: Passed: 2 | Failed: 0 | Skipped: 0"
Assert-Equal -Expected "PASS" -Actual $validSuite.Status `
    -Message "A nonempty suite with zero failures/skips and exit zero must pass"

$noValidationSummary = Get-DataValidationCheckResult -ExitCode 0 `
    -Output "Godot booted; timeout elapsed"
Assert-Equal -Expected "FAIL" -Actual $noValidationSummary.Status `
    -Message "A validation run with exit zero but no SUMMARY must fail"

$zeroResourceValidation = Get-DataValidationCheckResult -ExitCode 0 `
    -Output "SUMMARY: 0 error(s), 0 warning(s) across 0 resource(s) checked."
Assert-Equal -Expected "FAIL" -Actual $zeroResourceValidation.Status `
    -Message "A validation run that checks zero resources must fail"

$errorValidation = Get-DataValidationCheckResult -ExitCode 0 `
    -Output "SUMMARY: 1 error(s), 0 warning(s) across 3 resource(s) checked."
Assert-Equal -Expected "FAIL" -Actual $errorValidation.Status `
    -Message "A validation run reporting errors must fail"

$validValidation = Get-DataValidationCheckResult -ExitCode 0 `
    -Output "SUMMARY: 0 error(s), 2 warning(s) across 3 resource(s) checked."
Assert-Equal -Expected "PASS" -Actual $validValidation.Status `
    -Message "A validation run with resources and only warnings must pass"

$nonzeroSmoke = Get-SmokeCheckResult -ExitCode 9 -Output "[kill] e1"
Assert-Equal -Expected "FAIL" -Actual $nonzeroSmoke.Status `
    -Message "A smoke run with a nonzero native exit code must fail"

$validSmoke = Get-SmokeCheckResult -ExitCode 0 -Output "[kill] e1"
Assert-Equal -Expected "PASS" -Actual $validSmoke.Status `
    -Message "A clean smoke run with a kill and exit zero must pass"

Write-Output "Run-HeadlessChecks regression checks passed."
