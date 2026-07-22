# InDows module: network-profile
# Set the active network connection(s) to the Private profile (enables LAN discovery/sharing).
# Runs at FIRST LOGON (needs a live network). Anchor: [InDows:module] first-logon-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-network-profile.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== network-profile start ==='
Get-NetConnectionProfile -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        Set-NetConnectionProfile -InterfaceIndex $_.InterfaceIndex -NetworkCategory Private -ErrorAction Stop
        Log "  set Private on '$($_.Name)'"
    } catch { Log "  FAILED on '$($_.Name)': $_" }
}
Log '=== network-profile done ==='
