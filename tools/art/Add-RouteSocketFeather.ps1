<#
Applies a deterministic alpha feather to the outer edges of a prepared route
tile so adjacent tiles can be placed with a small overlap and cross-blend at
runtime, instead of relying on the source art's edges happening to line up.

Earlier route tiles (D43) only asked the image generator to match pixels at
each edge in the prompt text; nothing in the pipeline enforced it, and in
practice the generator did not reproduce the same road-edge position/curve
from one independent generation to the next, producing visible zigzag seams
between placed tiles. This script replaces "hope the generation matches"
with "guarantee the composite blends" — see docs/DECISIONS.md D66 and
godot-project/assets/art/theaters/western_europe/ROUTE_TILESET_PLAN.md §2.

Alpha near each of the four edges is ramped linearly from the pixel's
original alpha (at FeatherWidth px inward or deeper) down to fully
transparent (at the true edge, 0px). Corner regions use the minimum of the
two applicable edge ramps, which is the standard approach for edge-based
feather masks so corners fade smoothly rather than being double-darkened.

Usage:
    pwsh tools/art/Add-RouteSocketFeather.ps1 `
        -Source path/to/cleaned_tile.png `
        -Destination path/to/tile_feathered.png `
        -FeatherWidth 96

Run after the existing Prepare-WesternEurope*.ps1 cleanup scripts, not
instead of them — this expects a tile that has already had its flattened
background removed.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Source,

    [Parameter(Mandatory = $true)]
    [string]$Destination,

    # Default matches ROUTE_TILESET_PLAN.md §2: 96px at the 512px generation
    # canvas (2x supersample), downsampling to a 48px final feather.
    [int]$FeatherWidth = 96,

    # Which of the tile's four edges get the feather. A closed edge on a
    # corner/T-junction piece can be excluded if it should stay hard-edged
    # against non-route ground material instead of blending as a socket.
    [string[]]$Edges = @("N", "E", "S", "W")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$featherN = $Edges -contains "N"
$featherE = $Edges -contains "E"
$featherS = $Edges -contains "S"
$featherW = $Edges -contains "W"

$sourceBitmap = [System.Drawing.Bitmap]::new($Source)
$width = $sourceBitmap.Width
$height = $sourceBitmap.Height
$output = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

for ($y = 0; $y -lt $height; $y++) {
    for ($x = 0; $x -lt $width; $x++) {
        $color = $sourceBitmap.GetPixel($x, $y)
        if ($color.A -eq 0) {
            $output.SetPixel($x, $y, $color)
            continue
        }

        # Distance to the nearest active edge, in pixels; $FeatherWidth or
        # more means "not in any feather zone" for that edge.
        $ramp = 1.0
        if ($featherN) { $ramp = [Math]::Min($ramp, [double]$y / $FeatherWidth) }
        if ($featherS) { $ramp = [Math]::Min($ramp, [double]($height - 1 - $y) / $FeatherWidth) }
        if ($featherW) { $ramp = [Math]::Min($ramp, [double]$x / $FeatherWidth) }
        if ($featherE) { $ramp = [Math]::Min($ramp, [double]($width - 1 - $x) / $FeatherWidth) }
        $ramp = [Math]::Max(0.0, [Math]::Min(1.0, $ramp))

        if ($ramp -ge 1.0) {
            $output.SetPixel($x, $y, $color)
        }
        else {
            $newAlpha = [int]([Math]::Round($color.A * $ramp))
            $output.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($newAlpha, $color.R, $color.G, $color.B))
        }
    }
}

$sourceBitmap.Dispose()

$destinationDirectory = Split-Path -Parent $Destination
New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
$output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
$output.Dispose()
Write-Output $Destination
