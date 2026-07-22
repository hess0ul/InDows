# InDows module: device-metadata
# Stop Windows fetching device apps/metadata (icons, info) from Microsoft for your hardware.
# Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-device-metadata.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== device-metadata start ==='
# Prevent device metadata retrieval from the network (setting + Group Policy mirror)
RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata' 'PreventDeviceMetadataFromNetwork' 1
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Device Metadata' 'PreventDeviceMetadataFromNetwork' 1
Log '=== device-metadata done ==='
