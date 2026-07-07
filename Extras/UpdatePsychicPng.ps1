Add-Type -AssemblyName System.Drawing
$root = 'C:\Users\david\OneDrive\Documents\My Games\Terraria\tModLoader\ModSources\SariaMod'
$bmp = New-Object System.Drawing.Bitmap(2, 4)
$bmp.SetPixel(0, 0, [System.Drawing.Color]::FromArgb(255, 152, 24, 8))
$bmp.SetPixel(1, 0, [System.Drawing.Color]::FromArgb(255, 235, 82, 210))
$bmp.SetPixel(0, 1, [System.Drawing.Color]::FromArgb(255, 232, 56, 32))
$bmp.SetPixel(1, 1, [System.Drawing.Color]::FromArgb(255, 242, 155, 214))
$bmp.SetPixel(0, 2, [System.Drawing.Color]::FromArgb(255, 238, 51, 26))
$bmp.SetPixel(1, 2, [System.Drawing.Color]::FromArgb(255, 242, 155, 214))
$bmp.SetPixel(0, 3, [System.Drawing.Color]::FromArgb(255, 153, 22, 6))
$bmp.SetPixel(1, 3, [System.Drawing.Color]::FromArgb(255, 235, 82, 210))
$bmp.Save("$root\PsychicEyesForm1-5.png", [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "Saved"
