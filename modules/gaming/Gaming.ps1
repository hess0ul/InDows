# InDows module: gaming
# Background recording (GameDVR) OFF, Game Mode ON, mouse acceleration OFF.
# Runs during specialize WITH the default-user hive loaded. Anchor: [InDows:module] default-user-scripts
# (It also writes one machine-wide policy; that's fine from this anchor too.)

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-gaming.log'
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

Log '=== gaming start ==='
# GameDVR / background recording OFF (machine policy + per-user)
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR' 'AllowGameDVR' 0
RegDword "$U\System\GameConfigStore" 'GameDVR_Enabled' 0
RegDword "$U\Software\Microsoft\Windows\CurrentVersion\GameDVR" 'AppCaptureEnabled' 0
# Game Mode ON
RegDword "$U\Software\Microsoft\GameBar" 'AutoGameModeEnabled' 1
# Mouse acceleration OFF (these three are REG_SZ)
RegString "$U\Control Panel\Mouse" 'MouseSpeed' '0'
RegString "$U\Control Panel\Mouse" 'MouseThreshold1' '0'
RegString "$U\Control Panel\Mouse" 'MouseThreshold2' '0'

# --- ADVANCED (commented) - DRIVER-DEPENDENT: these can help OR hurt depending on your GPU + driver + game ---
# Fullscreen Optimizations OFF (force true exclusive fullscreen; can lower latency, can also break HDR/capture):
# RegDword "$U\System\GameConfigStore" 'GameDVR_FSEBehaviorMode' 2
# RegDword "$U\System\GameConfigStore" 'GameDVR_FSEBehavior' 2
# Hardware-Accelerated GPU Scheduling (HAGS) ON (good on recent GPUs, problematic on some older/driver combos):
# RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers' 'HwSchMode' 2
Log '=== gaming done ==='
