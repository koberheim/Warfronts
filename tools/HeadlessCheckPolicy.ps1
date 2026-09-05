function Get-HeadlessFailureCount {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.IEnumerable]$Results
    )

    return @($Results | Where-Object { $_.Status -ne "PASS" }).Count
}

function Get-GoDotTestCheckResult {
    param(
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Output
    )

    $match = [regex]::Match($Output, 'Test results: Passed: (\d+) \| Failed: (\d+) \| Skipped: (\d+)')
    if (-not $match.Success) {
        return [PSCustomObject]@{
            Status = "FAIL"
            Detail = "exit $ExitCode; could not parse 'Test results:' line from output"
        }
    }

    $passed = [int]$match.Groups[1].Value
    $failed = [int]$match.Groups[2].Value
    $skipped = [int]$match.Groups[3].Value
    $detail = "exit $ExitCode; Passed: $passed | Failed: $failed | Skipped: $skipped"

    $ok = ($ExitCode -eq 0) -and ($passed -gt 0) -and ($failed -eq 0) -and ($skipped -eq 0)
    if (-not $ok) {
        if ($passed -eq 0 -and $failed -eq 0 -and $skipped -eq 0) {
            $detail += "; zero tests were reported"
        }
        elseif ($skipped -gt 0) {
            $detail += "; skipped tests are not accepted as green"
        }
    }

    return [PSCustomObject]@{
        Status = if ($ok) { "PASS" } else { "FAIL" }
        Detail = $detail
    }
}

function Get-DataValidationCheckResult {
    param(
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Output
    )

    $match = [regex]::Match(
        $Output,
        'SUMMARY:\s*(\d+) error\(s\),\s*(\d+) warning\(s\) across\s*(\d+) resource\(s\) checked\.'
    )
    if (-not $match.Success) {
        return [PSCustomObject]@{
            Status = "FAIL"
            Detail = "exit $ExitCode; could not parse a valid Data Validator SUMMARY line"
        }
    }

    $errors = [int]$match.Groups[1].Value
    $warnings = [int]$match.Groups[2].Value
    $resources = [int]$match.Groups[3].Value
    $detail = "exit $ExitCode; $errors error(s), $warnings warning(s), $resources resource(s) checked"
    $ok = ($ExitCode -eq 0) -and ($errors -eq 0) -and ($resources -gt 0)

    if (-not $ok) {
        if ($errors -gt 0) { $detail += "; validation errors were reported" }
        if ($resources -le 0) { $detail += "; no resources were checked" }
    }

    return [PSCustomObject]@{
        Status = if ($ok) { "PASS" } else { "FAIL" }
        Detail = $detail
    }
}

function Get-SmokeCheckResult {
    param(
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Output
    )

    $smokeLines = $Output -split "`r?`n"
    $errorLines = @($smokeLines | Where-Object { $_ -match "error|exception" })
    $killLines = @($smokeLines | Where-Object { $_ -match "\[kill\]" })
    $ok = ($ExitCode -eq 0) -and ($errorLines.Count -eq 0) -and ($killLines.Count -ge 1)

    return [PSCustomObject]@{
        Status = if ($ok) { "PASS" } else { "FAIL" }
        Detail = "exit $ExitCode; $($errorLines.Count) error/exception line(s), $($killLines.Count) [kill] line(s)"
    }
}
