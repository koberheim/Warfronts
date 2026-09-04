param(
    [string]$Source = "C:\Users\Kevin\.codex\generated_images\01a064d9-e90e-7e11-be6d-3a309b986488\exec-83ab2b9f-9a04-46fd-94f1-2307274aae2c.png",
    [string]$Destination = (Join-Path $PSScriptRoot "../../godot-project/assets/art/theaters/western_europe/terrain/route_overlays/route_overlay_sunken_lane_ne_v01.png"),
    [int]$DestinationX = 180,
    [int]$DestinationY = 0,
    [int]$DestinationWidth = 1200,
    [int]$DestinationHeight = 750
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
$transparent = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)

$sourceOriginal = [System.Drawing.Bitmap]::new($Source)
$sourceRect = New-Object System.Drawing.Rectangle(0, 0, $sourceOriginal.Width, $sourceOriginal.Height)
$sourceBitmap = $sourceOriginal.Clone($sourceRect, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sourceOriginal.Dispose()

# The generator may flatten requested transparency to a white/checkerboard
# plate. Remove only neutral light pixels; the route's brown and green art is
# chromatic enough to remain intact.
for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
    for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
        $color = $sourceBitmap.GetPixel($x, $y)
        $max = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
        $min = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
        if (($max - $min) -le 16 -and $min -ge 150) {
            $sourceBitmap.SetPixel($x, $y, $transparent)
        }
    }
}

$outputSize = 1024
$output = New-Object System.Drawing.Bitmap($outputSize, $outputSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($output)
$graphics.Clear($transparent)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy

# The accepted reference is a tall L-shaped route. This deterministic fit
# makes its named arms meet the shared socket contract while retaining the
# unique painted curve. The destination can be shifted/scaled per topology;
# an oversized destination is clipped by the 1024px tile canvas intentionally.
$destinationRect = New-Object System.Drawing.Rectangle($DestinationX, $DestinationY, $DestinationWidth, $DestinationHeight)
$sourceRect = New-Object System.Drawing.Rectangle(0, 0, $sourceBitmap.Width, $sourceBitmap.Height)
$graphics.DrawImage($sourceBitmap, $destinationRect, $sourceRect.X, $sourceRect.Y, $sourceRect.Width, $sourceRect.Height, [System.Drawing.GraphicsUnit]::Pixel)
$graphics.Dispose()
$sourceBitmap.Dispose()

# Remove light interpolation halos along the alpha boundary. GDI+ can
# re-expand fully transparent white source pixels while resampling, so this
# pass intentionally checks the RGB values regardless of the resulting alpha.
for ($y = 0; $y -lt $outputSize; $y++) {
    for ($x = 0; $x -lt $outputSize; $x++) {
        $color = $output.GetPixel($x, $y)
        $max = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
        $min = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
        if (($max - $min) -le 20 -and $min -ge 150) {
            $output.SetPixel($x, $y, $transparent)
        }
    }
}

$destinationDirectory = Split-Path -Parent $Destination
New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
$output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
$output.Dispose()
Write-Output $Destination
