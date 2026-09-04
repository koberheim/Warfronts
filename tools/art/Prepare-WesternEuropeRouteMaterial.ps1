param(
    [string]$Source = "C:\Users\Kevin\.codex\generated_images\01a064d9-e90e-7e11-be6d-3a309b986488\exec-de6be9b6-014d-4349-ada4-3ca74eab40c4.png",
    [string]$Destination = (Join-Path $PSScriptRoot "../../godot-project/assets/art/shared/route_materials/western_europe/route_material_sunken_lane_v01.png")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$sourceSize = 1024
$outputWidth = 352
$sourceBitmap = [System.Drawing.Bitmap]::new($Source)
$cropX = [Math]::Max(0, [int](($sourceBitmap.Width - $sourceSize) / 2))
$cropY = [Math]::Max(0, [int](($sourceBitmap.Height - $sourceSize) / 2))
$cropped = New-Object System.Drawing.Bitmap($sourceSize, $sourceSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$cropGraphics = [System.Drawing.Graphics]::FromImage($cropped)
$cropGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$cropGraphics.DrawImage($sourceBitmap, (New-Object System.Drawing.Rectangle(0, 0, $sourceSize, $sourceSize)), $cropX, $cropY, $sourceSize, $sourceSize, [System.Drawing.GraphicsUnit]::Pixel)
$cropGraphics.Dispose()
$sourceBitmap.Dispose()

# The generator represented transparency with a grey checkerboard. Remove
# only the connected-looking neutral light background; the brown route and its
# green/brown shoulder remain opaque. This is deterministic post-processing of
# the accepted visual reference, not a new gameplay texture design.
for ($y = 0; $y -lt $sourceSize; $y++) {
    for ($x = 0; $x -lt $sourceSize; $x++) {
        $color = $cropped.GetPixel($x, $y)
        $max = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
        $min = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
        if (($max - $min) -le 16 -and $max -ge 150) {
            $cropped.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
        }
    }
}

$minX = $sourceSize
$maxX = -1
for ($y = 0; $y -lt $sourceSize; $y += 2) {
    for ($x = 0; $x -lt $sourceSize; $x += 2) {
        if ($cropped.GetPixel($x, $y).A -gt 20) {
            $minX = [Math]::Min($minX, $x)
            $maxX = [Math]::Max($maxX, $x)
        }
    }
}
if ($maxX -lt $minX) { throw "No route pixels remained after background removal." }

$boundsWidth = $maxX - $minX + 1
$output = New-Object System.Drawing.Bitmap($outputWidth, $sourceSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($output)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
$graphics.DrawImage($cropped, (New-Object System.Drawing.Rectangle(0, 0, $outputWidth, $sourceSize)), $minX, 0, $boundsWidth, $sourceSize, [System.Drawing.GraphicsUnit]::Pixel)
$graphics.Dispose()
$cropped.Dispose()

# Bicubic resampling can pull the removed checkerboard's bright RGB values
# into partially transparent edge pixels. Remove that halo before Godot sees
# the texture; the route's brown/green edge pixels are retained.
for ($y = 0; $y -lt $sourceSize; $y++) {
    for ($x = 0; $x -lt $outputWidth; $x++) {
        $color = $output.GetPixel($x, $y)
        $max = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
        $min = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
        if ($color.A -lt 255 -and ($max - $min) -le 20 -and $max -ge 150) {
            $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
        }
    }
}

$destinationDirectory = Split-Path -Parent $Destination
New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
$output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
$output.Dispose()
Write-Output $Destination
