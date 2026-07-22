# InDows module: debloat-brave
# Turn off Brave's built-in extras via enterprise policy (Rewards, Wallet, VPN, AI, Tor, telemetry...).
# Each line is independent -- comment out what you want to keep. Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-debloat-brave.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$b = 'HKLM:\SOFTWARE\Policies\BraveSoftware\Brave'

Log '=== debloat-brave start ==='
RegDword $b 'BraveRewardsDisabled' 1                       # Brave Rewards / BAT off
RegDword $b 'BraveWalletDisabled' 1                        # Brave Wallet off
RegDword $b 'BraveVPNDisabled' 1                           # Brave VPN off
RegDword $b 'BraveAIChatEnabled' 0                         # Leo AI chat off
RegDword $b 'BraveStatsPingEnabled' 0                      # usage stats ping off
RegDword $b 'BraveNewsDisabled' 1                          # Brave News off
RegDword $b 'BraveTalkDisabled' 1                          # Brave Talk off
RegDword $b 'TorDisabled' 1                                # private Tor windows off
RegDword $b 'BraveP3AEnabled' 0                            # privacy-preserving analytics off
RegDword $b 'UrlKeyedAnonymizedDataCollectionEnabled' 0    # anonymized URL data collection off
RegDword $b 'SafeBrowsingExtendedReportingEnabled' 0       # extended safe-browsing reports off
RegDword $b 'MetricsReportingEnabled' 0                    # metrics reporting off
Log '=== debloat-brave done ==='
