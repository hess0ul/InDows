# InDows module: visual-effects
# Snappier visuals for new users. Each line is independent -- comment out anything you want to KEEP.
# Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-visual-effects.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}
$U = 'Registry::HKEY_USERS\DefaultUser'

Log '=== visual-effects start ==='
# --- individual tweaks (active) ---
RegDword  "$U\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" 'EnableTransparency' 0    # no transparency
RegDword  "$U\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" 'TaskbarAnimations' 0      # no taskbar animations
RegString "$U\Control Panel\Desktop" 'MenuShowDelay' '0'                                              # instant menus
RegString "$U\Control Panel\Desktop\WindowMetrics" 'MinAnimate' '0'                                   # no minimize/maximize animation
RegDword  "$U\Software\Microsoft\Windows\DWM" 'EnableAeroPeek' 0                                      # no "peek at desktop"
RegDword  "$U\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize" 'StartupDelayInMSec' 0    # no Start menu startup delay

# --- NUCLEAR option (commented): "adjust for best performance" turns off basically ALL effects (plainer look) ---
# RegDword "$U\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects" 'VisualFXSetting' 2
Log '=== visual-effects done ==='
