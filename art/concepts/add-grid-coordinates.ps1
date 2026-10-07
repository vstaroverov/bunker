param(
    [ValidateSet('overview', 'drawing')]
    [string]$Variant = 'overview'
)

Add-Type -AssemblyName System.Drawing

function Get-RowLetters([int]$Index) {
    $number = $Index + 1
    $letters = ''
    while ($number -gt 0) {
        $number--
        $letters = [char](65 + ($number % 26)) + $letters
        $number = [int][Math]::Floor($number / 26)
    }
    return $letters
}

$conceptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceName = if ($Variant -eq 'overview') { 'game-screen-layout-grid.png' } else { 'game-screen-layout-grid-large.png' }
$outputName = if ($Variant -eq 'overview') { 'game-screen-layout-grid-coordinates.png' } else { 'game-screen-layout-grid-large-coordinates.png' }
$sourcePath = Join-Path $conceptDir $sourceName
$outputPath = Join-Path $conceptDir $outputName
$columns = if ($Variant -eq 'overview') { 100 } else { 50 }
$leftMargin = 60
$topMargin = 52

$source = [System.Drawing.Image]::FromFile($sourcePath)
$cell = $source.Width / $columns
$rows = $source.Height / $cell
$canvas = [System.Drawing.Bitmap]::new($source.Width + 2 * $leftMargin, $source.Height + 2 * $topMargin)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)

try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(246, 245, 240))
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $graphics.DrawImage($source, $leftMargin, $topMargin, $source.Width, $source.Height)

    $fontSize = if ($Variant -eq 'overview') { 12 } else { 18 }
    $font = [System.Drawing.Font]::new('Arial', $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $textBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(45, 62, 68))
    $tickPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(150, 72, 91, 96), 1)
    $borderPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(72, 91, 96), 2)

    for ($column = 0; $column -lt $columns; $column++) {
        $label = [string]$column
        $centerX = $leftMargin + ($column + 0.5) * $cell
        $size = $graphics.MeasureString($label, $font)
        $textX = [float]($centerX - $size.Width / 2)
        $graphics.DrawString($label, $font, $textBrush, $textX, [float](($topMargin - $size.Height) / 2))
        $graphics.DrawString($label, $font, $textBrush, $textX, [float]($topMargin + $source.Height + ($topMargin - $size.Height) / 2))
        if ($column % 5 -eq 0) {
            $tickX = $leftMargin + $column * $cell
            $graphics.DrawLine($tickPen, $tickX, $topMargin - 9, $tickX, $topMargin)
            $graphics.DrawLine($tickPen, $tickX, $topMargin + $source.Height, $tickX, $topMargin + $source.Height + 9)
        }
    }

    for ($row = 0; $row -lt $rows; $row++) {
        $label = Get-RowLetters $row
        $centerY = $topMargin + ($row + 0.5) * $cell
        $size = $graphics.MeasureString($label, $font)
        $textY = [float]($centerY - $size.Height / 2)
        $graphics.DrawString($label, $font, $textBrush, [float](($leftMargin - $size.Width) / 2), $textY)
        $graphics.DrawString($label, $font, $textBrush, [float]($leftMargin + $source.Width + ($leftMargin - $size.Width) / 2), $textY)
        if ($row % 5 -eq 0) {
            $tickY = $topMargin + $row * $cell
            $graphics.DrawLine($tickPen, $leftMargin - 9, $tickY, $leftMargin, $tickY)
            $graphics.DrawLine($tickPen, $leftMargin + $source.Width, $tickY, $leftMargin + $source.Width + 9, $tickY)
        }
    }

    $graphics.DrawRectangle($borderPen, $leftMargin, $topMargin, $source.Width, $source.Height)
    $borderPen.Dispose()
    $tickPen.Dispose()
    $textBrush.Dispose()
    $font.Dispose()

    $canvas.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Output $outputPath
} finally {
    $graphics.Dispose()
    $canvas.Dispose()
    $source.Dispose()
}
