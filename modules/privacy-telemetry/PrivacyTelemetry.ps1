# InDows module: privacy-telemetry
# Extra privacy hardening on top of the base. Each line is independent -- comment out what you want to keep.
# Anchor: [InDows:module] default-user-scripts (machine policy + per-user defaults)

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-privacy-telemetry.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$U = 'Registry::HKEY_USERS\DefaultUser'

Log '=== privacy-telemetry start ==='
# Activity history (machine policy)
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' 'PublishUserActivities' 0
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' 'EnableActivityFeed' 0
# Advertising ID (per-user)
RegDword "$U\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo" 'Enabled' 0
# Tailored experiences with diagnostic data (per-user)
RegDword "$U\Software\Microsoft\Windows\CurrentVersion\Privacy" 'TailoredExperiencesWithDiagnosticDataEnabled' 0
# Suggested content, tips, and silent OEM app installs (per-user, ContentDeliveryManager)
$cdm = "$U\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"
RegDword $cdm 'SystemPaneSuggestionsEnabled' 0
RegDword $cdm 'SubscribedContent-310093Enabled' 0   # welcome / "get even more out of Windows"
RegDword $cdm 'SubscribedContent-338388Enabled' 0   # Start suggestions
RegDword $cdm 'SubscribedContent-338389Enabled' 0   # tips & tricks
RegDword $cdm 'SubscribedContent-353694Enabled' 0   # Settings suggestions
RegDword $cdm 'SubscribedContent-353696Enabled' 0   # Settings suggestions
RegDword $cdm 'SilentInstalledAppsEnabled' 0        # no silent app installs
RegDword $cdm 'PreInstalledAppsEnabled' 0
RegDword $cdm 'OemPreInstalledAppsEnabled' 0
RegDword $cdm 'ContentDeliveryAllowed' 0
# Find My Device (machine policy)
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\FindMyDevice' 'AllowFindMyDevice' 0
Log '=== privacy-telemetry done ==='
