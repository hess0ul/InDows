# InDows module: performance
# Foreground-responsiveness + multimedia scheduler tweaks. Machine-wide, specialize pass.
# Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-performance.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== performance start ==='
# Favor foreground apps (short, variable quantum with foreground boost)
RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control\PriorityControl' 'Win32PrioritySeparation' 38
# Multimedia System Profile
$mm = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile'
RegDword $mm 'SystemResponsiveness' 10
RegDword $mm 'NetworkThrottlingIndex' ([int]-1)   # -1 = 0xFFFFFFFF = network throttling off
# Per-task scheduling for games
$g = "$mm\Tasks\Games"
RegDword  $g 'GPU Priority' 8
RegDword  $g 'Priority' 6
RegString $g 'Scheduling Category' 'High'
RegString $g 'SFIO Priority' 'High'

# --- ADVANCED (commented) - bigger hammer, weigh it ---
# Force a SINGLE svchost process (less service isolation; set the value to your RAM in KB --
# 8 GB = 8388608, 16 GB = 16777216, 32 GB = 33554432):
# RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control' 'SvcHostSplitThresholdInKB' 16777216
# (Per-user perf tweaks -- background apps off, keyboard repeat speed -- belong in [U] modules.)
Log '=== performance done ==='
