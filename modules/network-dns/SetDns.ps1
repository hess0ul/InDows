# InDows module: network-dns
# Sets DNS servers on every active physical adapter. Runs at FIRST LOGON (needs a live network).
# Anchor: [InDows:module] first-logon-scripts
# Pick a provider by editing $dns below (uncomment one). Default = Cloudflare.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-network-dns.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

$dns = @('1.1.1.1', '1.0.0.1')                 # Cloudflare
# $dns = @('1.1.1.2', '1.0.0.2')               # Cloudflare (malware blocking)
# $dns = @('9.9.9.9', '149.112.112.112')       # Quad9
# $dns = @('94.140.14.14', '94.140.15.15')     # AdGuard
# $dns = @('8.8.8.8', '8.8.4.4')               # Google
# $dns = @('208.67.222.222', '208.67.220.220') # OpenDNS

Log "=== network-dns start (servers: $($dns -join ', ')) ==="
Get-NetAdapter -Physical -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq 'Up' } | ForEach-Object {
    try {
        Set-DnsClientServerAddress -InterfaceIndex $_.ifIndex -ServerAddresses $dns -ErrorAction Stop
        Log "  set DNS on '$($_.Name)'"
    } catch { Log "  FAILED on '$($_.Name)': $_" }
}
Log '=== network-dns done ==='
