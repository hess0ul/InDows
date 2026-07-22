# InDows module: storage-sense
# Turn on Storage Sense: auto-clean the Recycle Bin (60 days) and Downloads not opened in 60 days.
# Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-storage-sense.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$sp = 'Registry::HKEY_USERS\DefaultUser\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy'

$recycleDays  = 60   # [InDows:param RECYCLE_DAYS]  empty the Recycle Bin after this many days (0 = never)
$downloadDays = 60   # [InDows:param DOWNLOAD_DAYS] delete Downloads not opened in this many days (0 = never)

Log '=== storage-sense start ==='
# Storage Sense on + Recycle Bin cleanup + Downloads cleanup (numeric value names are literal)
RegDword $sp '01' 1
RegDword $sp '08' 1
RegDword $sp '256' $recycleDays
RegDword $sp '32' 1
RegDword $sp '512' $downloadDays
Log '=== storage-sense done ==='
