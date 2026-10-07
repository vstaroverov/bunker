Add-Type -AssemblyName System.Drawing

$conceptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourcePath = Join-Path $conceptDir 'game-screen-layout-grid-coordinates.png'
$outputPath = Join-Path $conceptDir 'first-screen-layout-from-sketch.png'
$source = [System.Drawing.Image]::FromFile($sourcePath)
$canvas = [System.Drawing.Bitmap]::new($source.Width, $source.Height)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$originX = 60
$originY = 52
$cell = 24

function X([double]$Column) { return [float]($originX + $Column * $cell) }
function Y([double]$Row) { return [float]($originY + $Row * $cell) }
function Box([double]$Column, [double]$Row, [double]$Width, [double]$Height, $Pen, $Brush) {
    $rectangle = [System.Drawing.RectangleF]::new((X $Column), (Y $Row), [float]($Width * $cell), [float]($Height * $cell))
    $graphics.FillRectangle($Brush, $rectangle)
    $graphics.DrawRectangle($Pen, $rectangle.X, $rectangle.Y, $rectangle.Width, $rectangle.Height)
}
function Mark([string]$Label, [double]$Column, [double]$Row, $Color) {
    $cx = X $Column
    $cy = Y $Row
    $circle = [System.Drawing.RectangleF]::new($cx - 18, $cy - 18, 36, 36)
    $fill = [System.Drawing.SolidBrush]::new($Color)
    $graphics.FillEllipse($fill, $circle)
    $graphics.DrawEllipse($whitePen, $circle)
    $size = $graphics.MeasureString($Label, $numberFont)
    $graphics.DrawString($Label, $numberFont, $whiteBrush, [float]($cx - $size.Width / 2), [float]($cy - $size.Height / 2))
    $fill.Dispose()
}

try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $graphics.DrawImage($source, 0, 0, $source.Width, $source.Height)

    $baseColor = [System.Drawing.Color]::FromArgb(183, 72, 51)
    $externalColor = [System.Drawing.Color]::FromArgb(40, 112, 128)
    $basePen = [System.Drawing.Pen]::new($baseColor, 5)
    $detailPen = [System.Drawing.Pen]::new($baseColor, 3)
    $externalPen = [System.Drawing.Pen]::new($externalColor, 5)
    $baseFill = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(30, 183, 72, 51))
    $externalFill = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(32, 40, 112, 128))
    $whiteBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
    $whitePen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, 2)
    $numberFont = [System.Drawing.Font]::new('Arial', 21, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)

    # Surface: entrance group, stair, lift, exterior gate and repairable ruins.
    Box 2 19 18 7 $basePen $baseFill
    Box 3 21 7 5 $detailPen $baseFill
    Box 12 21 7 5 $detailPen $baseFill
    $graphics.DrawLine($detailPen, (X 15.5), (Y 21), (X 15.5), (Y 26))
    for ($step = 0; $step -lt 4; $step++) {
        $graphics.DrawLine($detailPen, (X (3.5 + $step * 1.4)), (Y (25.4 - $step)), (X (4.7 + $step * 1.4)), (Y (25.4 - $step)))
    }
    Box 20 20 8 6 $basePen $baseFill
    $graphics.DrawLine($detailPen, (X 24), (Y 21), (X 24), (Y 26))
    Box 30 21 21 5 $basePen $baseFill
    # Six-cell repair project within the broader ruin silhouette: medium room.
    $mediumPen = [System.Drawing.Pen]::new($baseColor, 3)
    $mediumPen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
    $graphics.DrawRectangle($mediumPen, (X 37), (Y 22), 6 * $cell, 4 * $cell)
    $mediumPen.Dispose()

    # Interaction landmarks beyond the playable surface boundary at x = 55.
    Box 57 22 5 4 $externalPen $externalFill
    $graphics.DrawLine($externalPen, (X 63), (Y 24), (X 65), (Y 26))
    $graphics.DrawLine($externalPen, (X 65), (Y 24), (X 63), (Y 26))
    $graphics.DrawLine($externalPen, (X 62.7), (Y 25), (X 65.3), (Y 25))
    $graphics.DrawLine($externalPen, (X 66), (Y 17), (X 66), (Y 26))
    $flagPoints = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new((X 66), (Y 17)),
        [System.Drawing.PointF]::new((X 69), (Y 18)),
        [System.Drawing.PointF]::new((X 66), (Y 19))
    )
    $graphics.FillPolygon($externalFill, $flagPoints)
    $graphics.DrawPolygon($externalPen, $flagPoints)

    # One underground level: aligned vertical routes, corridor, door, blocked end.
    Box 2 33 8 7 $basePen $baseFill
    Box 12 33 7 7 $basePen $baseFill
    $graphics.DrawLine($detailPen, (X 15.5), (Y 33), (X 15.5), (Y 40))
    for ($step = 0; $step -lt 5; $step++) {
        $graphics.DrawLine($detailPen, (X (2.6 + $step * 1.25)), (Y (39.2 - $step)), (X (3.7 + $step * 1.25)), (Y (39.2 - $step)))
    }
    Box 20 34 26 6 $basePen $baseFill
    Box 27 37 2 3 $detailPen $baseFill
    $rubble = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new((X 46), (Y 40)),
        [System.Drawing.PointF]::new((X 47), (Y 38.4)),
        [System.Drawing.PointF]::new((X 48), (Y 38.8)),
        [System.Drawing.PointF]::new((X 48.7), (Y 37)),
        [System.Drawing.PointF]::new((X 49.7), (Y 36.6)),
        [System.Drawing.PointF]::new((X 50.5), (Y 34.8))
    )
    $graphics.DrawLines($basePen, $rubble)

    Mark '1' 67 15 $externalColor
    Mark '2' 64 22 $externalColor
    Mark '3' 59.5 20 $externalColor
    Mark '4' 40 19 $baseColor
    Mark '5' 24 18 $baseColor
    Mark '6' 11 17 $baseColor
    Mark '7' 16 23 $baseColor
    Mark '8' 6 23 $baseColor
    Mark '9' 36 32 $baseColor

    $numberFont.Dispose()
    $whitePen.Dispose()
    $whiteBrush.Dispose()
    $baseFill.Dispose()
    $externalFill.Dispose()
    $basePen.Dispose()
    $detailPen.Dispose()
    $externalPen.Dispose()

    $canvas.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Output $outputPath
} finally {
    $graphics.Dispose()
    $canvas.Dispose()
    $source.Dispose()
}
