# InDows module: services-tune
# Sets services to Manual (or Disabled in the hardening preset). Anchor: [InDows:module] specialize-scripts
#
# Choose a preset:
#   'safe'      -> rarely-used services to Manual (they still start on demand). Zero risk.
#   'hardening' -> also disables some telemetry/remote services (advanced; read the warnings).
# Edit the lists to taste -- each name is a service. Find names with: Get-Service | Sort Name

$preset = 'safe'

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-services-tune.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

# --- SAFE: rarely needed on a personal desktop -> Manual (start on demand) ---
$toManual = @(
    'MapsBroker'        # Downloaded Maps Manager
    'Fax'               # Fax
    'RetailDemo'        # Retail Demo (store kiosks)
    'WMPNetworkSvc'     # Windows Media Player network sharing
    'PhoneSvc'          # Phone Service
    'WalletService'     # Wallet
    'lfsvc'             # Geolocation
    'SCardSvr'          # Smart Card (uncomment-worthy only if you use smart cards)
    'WbioSrvc'          # Windows Biometric (only if no fingerprint/face login)
    'TabletInputService' # Touch Keyboard & Handwriting (desktop without touch)
)
$toDisabled = @()

if ($preset -eq 'hardening') {
    # --- ADVANCED: telemetry + remote-management. ---
    # WARN: don't disable RemoteRegistry / WinRM (RemoteAccess) if you manage this PC remotely.
    # NEVER add Dhcp / Dnscache / NlaSvc / WlanSvc here -- that breaks networking.
    $toDisabled += @(
        'DiagTrack'         # Connected User Experiences and Telemetry
        'dmwappushservice'  # WAP Push message routing (telemetry)
        'RemoteRegistry'    # remote registry access
        'RetailDemo'
        'WerSvc'            # Windows Error Reporting
    )
    $toManual += @('SysMain')   # SysMain/Superfetch (some prefer it Manual on SSD)
}

Log "=== services-tune start (preset: $preset) ==="
foreach ($s in ($toManual   | Select-Object -Unique)) { Set-Service -Name $s -StartupType Manual   -ErrorAction SilentlyContinue; Log "  Manual:   $s" }
foreach ($s in ($toDisabled | Select-Object -Unique)) { Set-Service -Name $s -StartupType Disabled -ErrorAction SilentlyContinue; Log "  Disabled: $s" }
Log '=== services-tune done ==='
