# InDows module: developer-tools
# Opt-in developer conveniences: `sudo` (force-new-window mode) + Developer Mode. OFF by default.
# Anchor: [InDows:module] specialize-scripts
#
# These lower a few guardrails on purpose (sideloading, elevation). Turn on only what you need.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-developer-tools.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== developer-tools start ==='
RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Sudo' 'Enabled' 1   # sudo enabled in force-new-window mode (never inline)
# Developer Mode: sideload + run unsigned trusted apps
RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock' 'AllowDevelopmentWithoutDevLicense' 1
RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock' 'AllowAllTrustedApps' 1
Log '=== developer-tools done ==='
