<#
.SYNOPSIS
    Runs every automated check this repo has, headlessly, in one pass.

.DESCRIPTION
    GDD §19 prompt 45 / §15.6 item 4 (Data Validator): "wire the headless
    form into a pre-commit check." This script is that check, run manually
    (see tools/README.md for how to wire it as an actual git hook).

    In order:
      1. `dotnet build FrontsOfWar.csproj --no-restore` (0 warnings/errors).
      2. Every GoDotTest suite under godot-project/tests/*.cs (discovered by
         scanning for "class X : TestClass" — no suite list to keep in sync
         by hand).
      3. `--validate-data` (the Data Validator's headless CLI path).
      4. The canonical smoke run (docs/DECISIONS.md D46):
         `--headless --fixed-fps 60 --quit-after 5400`, which must print zero
         error/exception lines and at least one [kill] line.

    Prints a pass/fail table and exits non-zero if anything failed.

.PARAMETER GodotMono
    Path to the .NET-enabled ("Mono") Godot 4.7.2 console binary. Defaults to
    $env:GODOT_MONO, then to the machine-local path recorded in
    docs/DECISIONS.md D13.

.PARAMETER ProjectPath
    Path to the Godot project root (the folder containing project.godot).
    Defaults to ../godot-project relative to this script.
#>
param(
    [string]$GodotMono = $(if ($env:GODOT_MONO) { $env:GODOT_MONO } else { "E:\Godot\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" }),
    [string]$ProjectPath = (Join-Path $PSScriptRoot "..\godot-project")
)

$ErrorActionPreference = "Stop"
$results = New-Object System.Collections.Generic.List[PSObject]

function Add-CheckResult {
    param([string]$Check, [string]$Status, [string]$Detail)
    $results.Add([PSCustomObject]@{ Check = $Check; Status = $Status; Detail = $Detail })
}

# Start-Process's -ArgumentList does NOT auto-quote array elements that
# contain spaces (a long-standing Windows PowerShell 5.1 quirk) — without
# this, a project path like "...\Tower Defense\godot-project" gets split
# into separate argv entries by the child process. Build one pre-quoted
# command-line string instead.
function ConvertTo-ArgumentString {
    param([string[]]$Arguments)
    $parts = foreach ($a in $Arguments) {
        if ($a -match '\s') { '"' + $a + '"' } else { $a }
    }
    return ($parts -join ' ')
}

# Runs a native executable with stdout/stderr merged via temp files rather
# than PowerShell's `2>&1`, which wraps native stderr lines in
# NativeCommandError objects in Windows PowerShell 5.1 and would corrupt the
# error/exception line-scan below.
function Invoke-NativeCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )
    $stdoutFile = [System.IO.Path]::GetTempFileName()
    $stderrFile = [System.IO.Path]::GetTempFileName()
    try {
        $argString = ConvertTo-ArgumentString -Arguments $Arguments
        $proc = Start-Process -FilePath $FilePath -ArgumentList $argString -WorkingDirectory $WorkingDirectory `
            -NoNewWindow -Wait -PassThru -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile
        $stdout = Get-Content -Path $stdoutFile -Raw -ErrorAction SilentlyContinue
        $stderr = Get-Content -Path $stderrFile -Raw -ErrorAction SilentlyContinue
        [PSCustomObject]@{
            ExitCode = $proc.ExitCode
            Output   = [string]$stdout + "`n" + [string]$stderr
        }
    }
    finally {
        Remove-Item -Path $stdoutFile, $stderrFile -ErrorAction SilentlyContinue
    }
}

Write-Host "==> dotnet build FrontsOfWar.csproj --no-restore"
$buildResult = Invoke-NativeCapture -FilePath "dotnet" -Arguments @("build", "FrontsOfWar.csproj", "--no-restore") -WorkingDirectory $ProjectPath
Write-Host $buildResult.Output
$buildOk = ($buildResult.ExitCode -eq 0)
Add-CheckResult "dotnet build" $(if ($buildOk) { "PASS" } else { "FAIL" }) "exit $($buildResult.ExitCode)"

if (-not $buildOk) {
    Add-CheckResult "GoDotTest suites" "SKIP" "build failed"
    Add-CheckResult "validate-data" "SKIP" "build failed"
    Add-CheckResult "smoke run" "SKIP" "build failed"
}
else {
    # Discover suite class names instead of hardcoding a list, so a suite
    # added by another session (e.g. a future BuildTests) is picked up
    # automatically.
    $suiteNames = New-Object System.Collections.Generic.List[string]
    $testFiles = Get-ChildItem -Path (Join-Path $ProjectPath "tests") -Filter "*.cs" -File
    foreach ($file in $testFiles) {
        $fileContent = Get-Content -Path $file.FullName -Raw
        foreach ($m in [regex]::Matches($fileContent, 'class\s+(\w+)\s*:\s*TestClass\b')) {
            $suiteNames.Add($m.Groups[1].Value)
        }
    }
    $suiteNames = $suiteNames | Sort-Object -Unique

    if (-not (Test-Path -Path $GodotMono)) {
        Add-CheckResult "GoDotTest suites" "FAIL" "Godot Mono binary not found at $GodotMono"
        Add-CheckResult "validate-data" "FAIL" "Godot Mono binary not found at $GodotMono"
        Add-CheckResult "smoke run" "FAIL" "Godot Mono binary not found at $GodotMono"
    }
    else {
        foreach ($suite in $suiteNames) {
            Write-Host "==> godot --headless --run-tests=$suite"
            $testRun = Invoke-NativeCapture -FilePath $GodotMono `
                -Arguments @("--headless", "--path", $ProjectPath, "--run-tests=$suite", "--quit-after", "1500") `
                -WorkingDirectory $ProjectPath
            $match = [regex]::Match($testRun.Output, 'Test results: Passed: (\d+) \| Failed: (\d+) \| Skipped: (\d+)')
            if ($match.Success) {
                $failed = [int]$match.Groups[2].Value
                $detail = "Passed: $($match.Groups[1].Value) | Failed: $failed | Skipped: $($match.Groups[3].Value)"
                Add-CheckResult "tests: $suite" $(if ($failed -eq 0) { "PASS" } else { "FAIL" }) $detail
                if ($failed -ne 0) { Write-Host $testRun.Output }
            }
            else {
                Add-CheckResult "tests: $suite" "FAIL" "could not parse 'Test results:' line from output"
                Write-Host $testRun.Output
            }
        }

        Write-Host "==> godot --headless --validate-data"
        $validateRun = Invoke-NativeCapture -FilePath $GodotMono `
            -Arguments @("--headless", "--path", $ProjectPath, "--validate-data", "--quit-after", "600") `
            -WorkingDirectory $ProjectPath
        Write-Host $validateRun.Output
        $summaryMatch = [regex]::Match($validateRun.Output, 'SUMMARY:.*')
        $summaryText = $(if ($summaryMatch.Success) { $summaryMatch.Value } else { "no SUMMARY line found" })
        Add-CheckResult "validate-data" $(if ($validateRun.ExitCode -eq 0) { "PASS" } else { "FAIL" }) "$summaryText (exit $($validateRun.ExitCode))"

        Write-Host "==> godot --headless smoke run (--fixed-fps 60 --quit-after 5400)"
        $smokeRun = Invoke-NativeCapture -FilePath $GodotMono `
            -Arguments @("--headless", "--path", $ProjectPath, "--fixed-fps", "60", "--quit-after", "5400") `
            -WorkingDirectory $ProjectPath
        $smokeLines = $smokeRun.Output -split "`r?`n"
        $errorLines = $smokeLines | Where-Object { $_ -match "error|exception" }
        $killLines = $smokeLines | Where-Object { $_ -match "\[kill\]" }
        $smokeOk = (($errorLines.Count -eq 0) -and ($killLines.Count -ge 1))
        Add-CheckResult "smoke run" $(if ($smokeOk) { "PASS" } else { "FAIL" }) `
            "$($errorLines.Count) error/exception line(s), $($killLines.Count) [kill] line(s)"
        if (-not $smokeOk) { Write-Host $smokeRun.Output }
    }
}

Write-Host ""
Write-Host "===================== Run-HeadlessChecks summary ====================="
$results | Format-Table -AutoSize | Out-String -Width 200 | Write-Host

$failureCount = ($results | Where-Object { $_.Status -eq "FAIL" }).Count
if ($failureCount -gt 0) {
    Write-Host "$failureCount check(s) FAILED."
    exit 1
}
else {
    Write-Host "All checks passed."
    exit 0
}
