Add-Type -AssemblyName System.Drawing

function Get-UniqueColors($path) {
    if (-not (Test-Path $path)) { Write-Host "NOT FOUND: $path"; return }
    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    Write-Host "=== $([System.IO.Path]::GetFileName($path)) ($($bmp.Width)x$($bmp.Height)) ==="
    $counts = @{}
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            $c = $bmp.GetPixel($x, $y)
            if ($c.A -gt 0) {
                $k = "($($c.R),$($c.G),$($c.B),$($c.A))"
                $counts[$k]++
            }
        }
    }
    $counts.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { Write-Host "  $($_.Key) x$($_.Value)" }
    $bmp.Dispose()
}
$root = 'C:\Users\david\OneDrive\Documents\My Games\Terraria\tModLoader\ModSources\SariaMod'

$pngMap = @{
    "PsychicEyesForm1-5 (source colors)" = "$root\PsychicEyesForm1-5.png"
    "PsychicEyesForm6 (source colors)"   = "$root\PsychicEyesForm6.png"
    "NormalFaceIdle"           = "$root\Items\Strange\GlobalSariaAnimations\IdleFaces\SariaNormalFaceIdle.png"
    "6SariaNormalFaceIdle"     = "$root\Items\Strange\GlobalSariaAnimations\IdleFaces\6SariaNormalFaceIdle.png"
    "6SariaIdleEyeBackground"  = "$root\Items\Strange\GlobalSariaAnimations\IdleFaces\6SariaIdleEyeBackground.png"
}

foreach ($label in $pngMap.Keys) {
    $path = $pngMap[$label]
    if (-not (Test-Path $path)) { Write-Host "$label : FILE NOT FOUND at $path"; continue }
    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    $colors = @{}
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            $c = $bmp.GetPixel($x, $y)
            if ($c.A -gt 0) {
                $key = "($($c.R),$($c.G),$($c.B),$($c.A))"
                $colors[$key] = ($colors[$key] + 1)
            }
        }
    }
    Write-Host "`n=== $label ($($bmp.Width)x$($bmp.Height)) ==="
    foreach ($k in ($colors.Keys | Sort-Object)) { Write-Host "  $k x$($colors[$k])" }
    $bmp.Dispose()
}
