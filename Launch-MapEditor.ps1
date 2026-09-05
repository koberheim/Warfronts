<#
.SYNOPSIS
    Opens the Fronts of War standalone map editor.

.DESCRIPTION
    Resolves the Godot project relative to this repository-root script and
    starts it through the developer-only --map-editor launch route. The
    executable must be a .NET-enabled (Mono) Godot build.

.PARAMETER GodotMono
    Optional path to a .NET-enabled Godot executable. Resolution order is:
    this parameter, $env:GODOT_MONO, then the machine-local path recorded in
    docs/DECISIONS.md D13.

.PARAMETER GodotArguments
    Optional arguments forwarded to Godot after --map-editor.
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$GodotMono,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$GodotArguments = @()
)

$ErrorActionPreference = "Stop"
$projectPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "godot-project"))
$projectFile = Join-Path $projectPath "project.godot"
$documentedFallback = "E:\Godot\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe"

if (-not (Test-Path -LiteralPath $projectFile -PathType Leaf)) {
    throw "Fronts of War project.godot was not found at '$projectFile'. Run this launcher from a complete repository checkout."
}

if ([string]::IsNullOrWhiteSpace($GodotMono)) {
    $GodotMono = if (-not [string]::IsNullOrWhiteSpace($env:GODOT_MONO)) {
        $env:GODOT_MONO
    }
    else {
        $documentedFallback
    }
}

if (-not (Test-Path -LiteralPath $GodotMono -PathType Leaf)) {
    throw "A .NET-enabled Godot executable was not found at '$GodotMono'. Pass -GodotMono '<path-to-Godot-Mono.exe>' or set `$env:GODOT_MONO."
}

$resolvedGodot = (Resolve-Path -LiteralPath $GodotMono).Path
$versionText = (& $resolvedGodot --version 2>&1 | Out-String).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Godot failed its version check at '$resolvedGodot'. Verify the executable and try again."
}
if ($versionText -notmatch '(?i)(mono|\.net)') {
    throw "'$resolvedGodot' is not a .NET-enabled Godot build (reported '$versionText'). Install the Mono/.NET build and pass it with -GodotMono or `$env:GODOT_MONO."
}

$launchArguments = @("--path", $projectPath, "--map-editor") + $GodotArguments
& $resolvedGodot @launchArguments
exit $LASTEXITCODE
