# InDows base - extra default-user tweaks the schneegans base does not cover.
#
# Runs during the 'specialize' pass WHILE HKU\DefaultUser is loaded (between the base's
# 'reg load' and 'reg unload'), so every NEW user created on the machine inherits these.
#
# Today it does ONE thing: turn off the Start-menu "promoted apps" (Content Delivery Manager),
# so a fresh install has NO advertised tiles (Spotify, TikTok, Instagram...) pinned in Start.
# Those are not installed apps - just download-on-click ads Windows pins by default. This kills
# them at the source for every account, instead of un-pinning them one by one afterwards.
#
# Target: Windows PowerShell 5.1. ASCII only.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Setup\Scripts\InDows-base-tweaks.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== InDows base-tweaks start ==='
# Content Delivery Manager (per-user, written to the DEFAULT profile so all new users inherit it).
$cdm = 'Registry::HKEY_USERS\DefaultUser\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager'
RegDword $cdm 'ContentDeliveryAllowed'          0   # master switch for delivered content
RegDword $cdm 'SilentInstalledAppsEnabled'      0   # the "promoted apps" pinned-but-not-installed
RegDword $cdm 'PreInstalledAppsEnabled'         0
RegDword $cdm 'OemPreInstalledAppsEnabled'      0
RegDword $cdm 'SubscribedContent-338388Enabled' 0   # Start menu suggestions
RegDword $cdm 'SystemPaneSuggestionsEnabled'    0   # suggestions in Start / Settings
Log '=== InDows base-tweaks done ==='
