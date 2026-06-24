# InDows module: disable-bing-search
# Removes Bing web results, Cortana and Search Highlights from the Start/Search box.
# Anchor: [InDows:module] default-user-scripts  (writes a machine policy + per-user defaults)

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-disable-bing-search.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$U = 'Registry::HKEY_USERS\DefaultUser'

Log '=== disable-bing-search start ==='
# Machine policy (all users)
$pol = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search'
RegDword $pol 'AllowCortana' 0
RegDword $pol 'DisableWebSearch' 1
RegDword $pol 'ConnectedSearchUseWeb' 0
RegDword $pol 'EnableDynamicContentInWSB' 0   # Search Highlights
# Per-user defaults
$s = "$U\Software\Microsoft\Windows\CurrentVersion\Search"
RegDword $s 'BingSearchEnabled' 0
RegDword $s 'CortanaConsent' 0
Log '=== disable-bing-search done ==='
