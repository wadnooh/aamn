Add-Type -AssemblyName System.Drawing

$resDir = Join-Path $PSScriptRoot "app\src\main\res"

$sizes = @(
    @{ Folder = "mipmap-mdpi"; Size = 48 },
    @{ Folder = "mipmap-hdpi"; Size = 72 },
    @{ Folder = "mipmap-xhdpi"; Size = 96 },
    @{ Folder = "mipmap-xxhdpi"; Size = 144 },
    @{ Folder = "mipmap-xxxhdpi"; Size = 192 }
)

foreach ($item in $sizes) {
    $dir = Join-Path $resDir $item.Folder
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

    # Remove conflicting XML icons in density folders
    $xmlIcon = Join-Path $dir "ic_launcher.xml"
    if (Test-Path $xmlIcon) { Remove-Item $xmlIcon -Force }
    $xmlRound = Join-Path $dir "ic_launcher_round.xml"
    if (Test-Path $xmlRound) { Remove-Item $xmlRound -Force }

    $size = $item.Size
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

    # Background in Primary Blue (#0284c7)
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(2, 132, 199))
    $g.FillRectangle($bgBrush, 0, 0, $size, $size)

    # White Bus Silhouette
    $busBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $margin = [int]($size * 0.18)
    $bw = $size - (2 * $margin)
    $bh = [int]($bw * 1.05)
    $by = [int](($size - $bh) / 2)

    $g.FillRectangle($busBrush, $margin, $by, $bw, $bh)

    # Dark Windshield (#0f172a)
    $darkBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(15, 23, 42))
    $winMargin = [int]($margin + ($bw * 0.12))
    $winW = [int]($bw * 0.76)
    $winH = [int]($bh * 0.32)
    $g.FillRectangle($darkBrush, $winMargin, [int]($by + ($bh * 0.12)), $winW, $winH)

    # Cyan Headlights (#38bdf8)
    $lightBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(56, 189, 248))
    $dotSize = [math]::Max(3, [int]($bw * 0.18))
    $g.FillEllipse($lightBrush, [int]($margin + ($bw * 0.14)), [int]($by + ($bh * 0.68)), $dotSize, $dotSize)
    $g.FillEllipse($lightBrush, [int]($margin + $bw - ($bw * 0.14) - $dotSize), [int]($by + ($bh * 0.68)), $dotSize, $dotSize)

    $pngPath = Join-Path $dir "ic_launcher.png"
    $bmp.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)

    $g.Dispose()
    $bmp.Dispose()
}

# Also generate 512x512 Store Icon
$storeBmp = New-Object System.Drawing.Bitmap(512, 512)
$sg = [System.Drawing.Graphics]::FromImage($storeBmp)
$sg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$sbg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(2, 132, 199))
$sg.FillRectangle($sbg, 0, 0, 512, 512)
$sbus = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$sg.FillRectangle($sbus, 90, 80, 332, 350)
$sdark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(15, 23, 42))
$sg.FillRectangle($sdark, 130, 120, 252, 110)
$slight = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(56, 189, 248))
$sg.FillEllipse($slight, 135, 310, 60, 60)
$sg.FillEllipse($slight, 315, 310, 60, 60)
$storeBmp.Save((Join-Path $PSScriptRoot "store-icon-512.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$sg.Dispose()
$storeBmp.Dispose()

Write-Host "PNG icons generated across all mipmap densities and 512x512 store icon!" -ForegroundColor Green
