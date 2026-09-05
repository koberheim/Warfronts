<#
.SYNOPSIS
    Restores, builds, and exports a reproducible Windows Godot build.

.DESCRIPTION
    Uses the project's pinned Godot.NET.Sdk version and Godot export
    configuration. The default is the release/player boundary. Select the
    developer preset to produce an ExportDebug build that retains the map
    editor and debug-only C# code.

    A supplied TemplatePath is installed into an isolated temporary APPDATA
    export-template directory for this invocation, so the tracked presets do
    not contain machine-specific paths. Without it, the script checks Godot's
    normal per-user template location. Missing-template errors include the
    exact official artifact URL but never download it.

.PARAMETER Preset
    Windows Player or Windows Developer.

.PARAMETER GodotPath
    Explicit .NET-enabled Godot 4.7.2 editor executable. Resolution order is
    this parameter, $env:GODOT_MONO, then the documented local fallback.

.PARAMETER TemplatePath
    Optional explicit Windows x86_64 custom export-template executable for the
    selected mode (release for Windows Player, debug for Windows Developer).

.PARAMETER OutputPath
    Optional absolute or project-relative .exe output path.

.PARAMETER SkipRestore
    Skip the forced NuGet restore. Use only when restore state is already known
    to be valid; the default reevaluates the dependency graph before the build.
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet("Windows Player", "Windows Developer")]
    [string]$Preset = "Windows Player",
    [string]$GodotPath,
    [string]$TemplatePath,
    [string]$OutputPath,
    [switch]$SkipRestore
)

$ErrorActionPreference = "Stop"
$projectPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\godot-project"))
$projectFile = Join-Path $projectPath "project.godot"
$csprojPath = Join-Path $projectPath "FrontsOfWar.csproj"
$presetFile = Join-Path $projectPath "export_presets.cfg"
$fallbackGodot = "E:\Godot\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe"
$templateUrl = "https://github.com/godotengine/godot-builds/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_export_templates.tpz"

if (-not (Test-Path -LiteralPath $projectFile -PathType Leaf)) {
    throw "Godot project.godot was not found at '$projectFile'."
}
if (-not (Test-Path -LiteralPath $csprojPath -PathType Leaf)) {
    throw "C# project was not found at '$csprojPath'."
}
if (-not (Test-Path -LiteralPath $presetFile -PathType Leaf)) {
    throw "Export presets were not found at '$presetFile'."
}

if ([string]::IsNullOrWhiteSpace($GodotPath)) {
    $GodotPath = if (-not [string]::IsNullOrWhiteSpace($env:GODOT_MONO)) {
        $env:GODOT_MONO
    }
    else {
        $fallbackGodot
    }
}
if (-not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw "A .NET-enabled Godot executable was not found at '$GodotPath'. Pass -GodotPath '<path-to-Godot-Mono.exe>' or set `$env:GODOT_MONO."
}
$GodotPath = (Resolve-Path -LiteralPath $GodotPath).Path

function ConvertTo-ArgumentString {
    param([string[]]$Arguments)
    $parts = foreach ($argument in $Arguments) {
        if ($argument -match '\s') { '"' + $argument + '"' } else { $argument }
    }
    return ($parts -join ' ')
}

function Invoke-NativeCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    $stdoutFile = [System.IO.Path]::GetTempFileName()
    $stderrFile = [System.IO.Path]::GetTempFileName()
    try {
        $argumentString = ConvertTo-ArgumentString -Arguments $Arguments
        $process = Start-Process -FilePath $FilePath -ArgumentList $argumentString `
            -WorkingDirectory $WorkingDirectory -NoNewWindow -Wait -PassThru `
            -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile
        $stdout = Get-Content -LiteralPath $stdoutFile -Raw -ErrorAction SilentlyContinue
        $stderr = Get-Content -LiteralPath $stderrFile -Raw -ErrorAction SilentlyContinue
        return [PSCustomObject]@{
            ExitCode = $process.ExitCode
            Output = [string]$stdout + "`n" + [string]$stderr
        }
    }
    finally {
        Remove-Item -LiteralPath $stdoutFile, $stderrFile -ErrorAction SilentlyContinue
    }
}

function Invoke-NativeChecked {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    Write-Host "==> $Label"
    $result = Invoke-NativeCapture -FilePath $FilePath -Arguments $Arguments -WorkingDirectory $WorkingDirectory
    if (-not [string]::IsNullOrWhiteSpace($result.Output)) { Write-Host $result.Output }
    if ($result.ExitCode -ne 0) {
        throw "$Label failed with native exit code $($result.ExitCode)."
    }
    return $result
}

function Resolve-StrictTempChild {
    param(
        [Parameter(Mandatory = $true)][string]$CandidatePath,
        [Parameter(Mandatory = $true)][string]$TempRoot
    )

    $resolvedRoot = (Resolve-Path -LiteralPath $TempRoot -ErrorAction Stop).Path
    $resolvedCandidate = (Resolve-Path -LiteralPath $CandidatePath -ErrorAction Stop).Path
    $rootFull = [System.IO.Path]::GetFullPath($resolvedRoot).TrimEnd([char[]]@('\', '/'))
    $candidateFull = [System.IO.Path]::GetFullPath($resolvedCandidate).TrimEnd([char[]]@('\', '/'))
    $childPrefix = $rootFull + [System.IO.Path]::DirectorySeparatorChar

    if ($candidateFull.Equals($rootFull, [System.StringComparison]::OrdinalIgnoreCase) -or
        -not $candidateFull.StartsWith($childPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing recursive cleanup outside the temporary export root. Candidate '$candidateFull'; root '$rootFull'."
    }

    return $candidateFull
}

$versionResult = Invoke-NativeCapture -FilePath $GodotPath -Arguments @("--version") -WorkingDirectory $projectPath
$versionText = $versionResult.Output.Trim()
if ($versionResult.ExitCode -ne 0) {
    throw "Godot failed its version check at '$GodotPath' with exit code $($versionResult.ExitCode)."
}
if ($versionText -notmatch '(?i)(mono|\.net)') {
    throw "'$GodotPath' is not a .NET-enabled Godot build (reported '$versionText')."
}
if ($versionText -notmatch '4\.7\.2') {
    throw "Godot 4.7.2 is required by FrontsOfWar.csproj; '$GodotPath' reported '$versionText'."
}

$isDeveloper = $Preset -eq "Windows Developer"
$configuration = if ($isDeveloper) { "ExportDebug" } else { "ExportRelease" }
$exportSwitch = if ($isDeveloper) { "--export-debug" } else { "--export-release" }
$templateFlavor = if ($isDeveloper) { "debug" } else { "release" }
$templateFileName = "windows_${templateFlavor}_x86_64.exe"
$versionId = "4.7.2.stable.mono"
$temporaryAppData = $null
$systemTempRoot = (Resolve-Path -LiteralPath ([System.IO.Path]::GetTempPath()) -ErrorAction Stop).Path
$originalAppData = $env:APPDATA

try {
    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $OutputPath = if ($isDeveloper) {
            "build/developer/FrontsOfWarDeveloper.exe"
        }
        else {
            "build/player/FrontsOfWar.exe"
        }
    }
    if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
        $OutputPath = Join-Path $projectPath $OutputPath
    }
    $OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
    $outputDirectory = Split-Path -Parent $OutputPath
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

    if ([string]::IsNullOrWhiteSpace($TemplatePath)) {
        $templatePath = Join-Path $env:APPDATA "Godot\export_templates\$versionId\$templateFileName"
        if (-not (Test-Path -LiteralPath $templatePath -PathType Leaf)) {
            throw "Missing Godot Mono $templateFlavor export template '$templatePath'. Install the pinned artifact from $templateUrl or pass -TemplatePath '<path-to-$templateFileName>'. No download was attempted."
        }
    }
    else {
        if (-not (Test-Path -LiteralPath $TemplatePath -PathType Leaf)) {
            throw "Explicit Godot Mono $templateFlavor export template was not found at '$TemplatePath'. Required artifact: $templateFileName from $templateUrl. No download was attempted."
        }

        $temporaryAppData = Join-Path $systemTempRoot ("FrontsOfWar-GodotExport-" + [Guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Path $temporaryAppData -ErrorAction Stop | Out-Null
        $temporaryAppData = Resolve-StrictTempChild -CandidatePath $temporaryAppData -TempRoot $systemTempRoot
        $temporaryTemplateDirectory = Join-Path $temporaryAppData "Godot\export_templates\$versionId"
        New-Item -ItemType Directory -Path $temporaryTemplateDirectory -ErrorAction Stop | Out-Null
        Copy-Item -LiteralPath (Resolve-Path -LiteralPath $TemplatePath).Path `
            -Destination (Join-Path $temporaryTemplateDirectory $templateFileName)
        $env:APPDATA = $temporaryAppData
        Write-Host "Using explicit $templateFlavor template through isolated APPDATA: $TemplatePath"
    }

    if (-not $SkipRestore) {
        Invoke-NativeChecked -Label "dotnet restore FrontsOfWar.csproj" -FilePath "dotnet" `
            -Arguments @("restore", $csprojPath, "--force", "--force-evaluate") -WorkingDirectory $projectPath | Out-Null
    }
    else {
        Write-Host "==> dotnet restore skipped by -SkipRestore"
    }

    Invoke-NativeChecked -Label "dotnet build FrontsOfWar.csproj -c $configuration --no-restore" `
        -FilePath "dotnet" -Arguments @("build", $csprojPath, "--configuration", $configuration, "--no-restore") `
        -WorkingDirectory $projectPath | Out-Null

    Invoke-NativeChecked -Label "$exportSwitch '$Preset' '$OutputPath'" -FilePath $GodotPath `
        -Arguments @("--headless", "--path", $projectPath, $exportSwitch, $Preset, $OutputPath) `
        -WorkingDirectory $projectPath | Out-Null

    if (-not (Test-Path -LiteralPath $OutputPath -PathType Leaf)) {
        throw "Godot reported a successful export, but the player executable was not found at '$OutputPath'."
    }
    Write-Host "Windows export complete: $OutputPath"
}
finally {
    $env:APPDATA = $originalAppData
    if ($null -ne $temporaryAppData -and (Test-Path -LiteralPath $temporaryAppData)) {
        $safeCleanupPath = Resolve-StrictTempChild -CandidatePath $temporaryAppData -TempRoot $systemTempRoot
        Remove-Item -LiteralPath $safeCleanupPath -Recurse -Force -ErrorAction Stop
    }
}
