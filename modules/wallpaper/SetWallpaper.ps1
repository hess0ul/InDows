# InDows module: wallpaper
# Sets the desktop wallpaper and (optionally) the lock-screen image. Anchor: [InDows:module] first-logon-scripts
# Put your image(s) on the disk first (see README), then set the paths below. Leave a path empty to skip it.

$desktop = 'C:\Windows\Web\Wallpaper\Windows\img0.jpg'   # your desktop image
$lock    = ''                                            # your lock-screen image (optional)
$style   = '10'   # fit mode: 10 = Fill, 6 = Fit, 2 = Stretch, 0 = Center, 22 = Span

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-wallpaper.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== wallpaper start ==='
if ($desktop -and (Test-Path $desktop)) {
    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'WallPaper' -Value $desktop -ErrorAction SilentlyContinue
    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'WallpaperStyle' -Value $style -ErrorAction SilentlyContinue
    Start-Process -FilePath 'rundll32.exe' -ArgumentList 'user32.dll,UpdatePerUserSystemParameters' -ErrorAction SilentlyContinue
    Log "desktop -> $desktop (style $style)"
} elseif ($desktop) { Log "desktop image not found: $desktop" }

if ($lock -and (Test-Path $lock)) {
    $csp = 'HKLM:\SOFTWARE\Policies\Microsoft\PolicyManager\current\device\Personalization'
    New-Item -Path $csp -Force -ErrorAction SilentlyContinue | Out-Null
    Set-ItemProperty -Path $csp -Name 'LockScreenImagePath'   -Value $lock -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $csp -Name 'LockScreenImageUrl'    -Value $lock -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $csp -Name 'LockScreenImageStatus' -Value 1     -Type DWord -ErrorAction SilentlyContinue
    Log "lockscreen -> $lock"
}
Log '=== wallpaper done ==='
