param(
    [ValidateSet('overview', 'drawing')]
    [string]$Variant = 'drawing'
)

Add-Type -AssemblyName System.Drawing

$conceptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$panoramaPath = Join-Path $conceptDir 'world-layout-background-hill.png'
$outputName = if ($Variant -eq 'drawing') { 'game-screen-layout-grid-large.png' } else { 'game-screen-layout-grid.png' }
$outputPath = Join-Path $conceptDir $outputName

$width = 2400
$columns = if ($Variant -eq 'drawing') { 50 } else { 100 }
$cell = $width / $columns
$surfaceRows = if ($Variant -eq 'drawing') { 13 } else { 26 }
$surfaceY = $surfaceRows * $cell
$separatorHeight = 2 * $cell
$firstFloorY = $surfaceY + $separatorHeight
$firstFloorEndY = $firstFloorY + 12 * $cell
$secondFloorY = $firstFloorEndY + $separatorHeight
$height = if ($Variant -eq 'drawing') { $firstFloorEndY } else { $secondFloorY + 12 * $cell }

$canvas = New-Object System.Drawing.Bitmap($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$panorama = [System.Drawing.Image]::FromFile($panoramaPath)

try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.Clear([System.Drawing.Color]::FromArgb(244, 245, 242))

    # Faint panorama leaves enough contrast for a dark pen or marker.
    $backgroundCrop = [System.Drawing.Rectangle]::new(0, 0, $panorama.Width, [int][Math]::Round($panorama.Width / 3.0))
    $graphics.DrawImage($panorama, [System.Drawing.Rectangle]::new(0, 0, $width, 800), $backgroundCrop, [System.Drawing.GraphicsUnit]::Pixel)
    $wash = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(145, 250, 250, 247))
    $graphics.FillRectangle($wash, 0, 0, $width, $surfaceY)
    $wash.Dispose()

    if ($Variant -eq 'overview') {
        $externalWorldX = 55 * $cell
        $externalWorldTint = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(50, 104, 163, 171))
        $graphics.FillRectangle($externalWorldTint, $externalWorldX, 0, $width - $externalWorldX, $surfaceY)
        $externalWorldTint.Dispose()
    }

    $upperEarth = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(232, 227, 216))
    $lowerEarth = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(222, 221, 216))
    $separator = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(190, 194, 188))
    $graphics.FillRectangle($separator, 0, $surfaceY, $width, $separatorHeight)
    $graphics.FillRectangle($upperEarth, 0, $firstFloorY, $width, 12 * $cell)
    if ($Variant -eq 'overview') {
        $graphics.FillRectangle($separator, 0, $firstFloorEndY, $width, $separatorHeight)
        $graphics.FillRectangle($lowerEarth, 0, $secondFloorY, $width, $height - $secondFloorY)
    }
    $upperEarth.Dispose()
    $lowerEarth.Dispose()
    $separator.Dispose()

    $minorPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(72, 62, 74, 78), 1)
    $majorPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(128, 62, 74, 78), 1.7)
    for ($column = 0; $column -le $columns; $column++) {
        $lineX = $column * $cell
        $pen = if ($column % 5 -eq 0) { $majorPen } else { $minorPen }
        $graphics.DrawLine($pen, $lineX, 0, $lineX, $height)
    }
    for ($row = 0; $row -le ($height / $cell); $row++) {
        $lineY = $row * $cell
        $pen = if ($row % 5 -eq 0) { $majorPen } else { $minorPen }
        $graphics.DrawLine($pen, 0, $lineY, $width, $lineY)
    }
    $minorPen.Dispose()
    $majorPen.Dispose()

    $surfacePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(190, 154, 97, 62), 3)
    $floorPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(145, 66, 79, 84), 2)
    $graphics.DrawLine($surfacePen, 0, $surfaceY, $width, $surfaceY)
    if ($Variant -eq 'overview') {
        $zonePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(190, 56, 111, 123), 3)
        $zonePen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
        $graphics.DrawLine($zonePen, $externalWorldX, 0, $externalWorldX, $surfaceY)
        $zonePen.Dispose()
    }
    $boundaries = if ($Variant -eq 'drawing') { @($firstFloorY, $firstFloorEndY) } else { @($firstFloorY, $firstFloorEndY, $secondFloorY) }
    foreach ($boundaryY in $boundaries) {
        $graphics.DrawLine($floorPen, 0, $boundaryY, $width, $boundaryY)
    }
    $surfacePen.Dispose()
    $floorPen.Dispose()

    $font = [System.Drawing.Font]::new('Arial', 17, [System.Drawing.FontStyle]::Regular)
    $labelBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(150, 38, 51, 54))
    $caption = if ($Variant -eq 'drawing') {
        '50 клеток в кадре  |  1 клетка = 0,8 м  |  этаж 12 клеток + разделение 2 клетки'
    } else {
        '100 клеток в ширину  |  1 клетка = 0,8 м  |  этаж 12 клеток + разделение 2 клетки'
    }
    $graphics.DrawString($caption, $font, $labelBrush, 34, 30)
    if ($Variant -eq 'overview') {
        $graphics.DrawString('Игровая поверхность: 55 клеток', $font, $labelBrush, 34, 66)
        $graphics.DrawString('Внешний мир: 45 клеток — действия и маршруты', $font, $labelBrush, $externalWorldX + 20, 66)
    }
    $graphics.DrawString('Поверхность', $font, $labelBrush, 34, $surfaceY - 42)
    $graphics.DrawString('Разделение: 2 клетки', $font, $labelBrush, 34, $surfaceY + 10)
    $graphics.DrawString('Подземный этаж 1', $font, $labelBrush, 34, $firstFloorY + 18)
    if ($Variant -eq 'overview') {
        $graphics.DrawString('Разделение: 2 клетки', $font, $labelBrush, 34, $firstFloorEndY + 10)
        $graphics.DrawString('Подземный этаж 2', $font, $labelBrush, 34, $secondFloorY + 18)
    }
    $labelBrush.Dispose()
    $font.Dispose()

    $canvas.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Output $outputPath
} finally {
    $panorama.Dispose()
    $graphics.Dispose()
    $canvas.Dispose()
}
