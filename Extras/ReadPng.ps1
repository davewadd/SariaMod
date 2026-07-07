Add-Type -AssemblyName System.Drawing
$root = 'C:\Users\david\OneDrive\Documents\My Games\Terraria\tModLoader\ModSources\SariaMod'
$b = [System.Drawing.Bitmap]::FromFile("$root\PsychicEyesForm1-5.png")
Write-Host "$($b.Width)x$($b.Height)"
for ($y = 0; $y -lt $b.Height; $y++) {
    $l = $b.GetPixel(0, $y)
    $r = $b.GetPixel(1, $y)
    Write-Host "row$y L=($($l.R),$($l.G),$($l.B),$($l.A)) R=($($r.R),$($r.G),$($r.B),$($r.A))"
}
$b.Dispose()
