# InDows module: optional-features
# Enables Windows optional features via DISM. Anchor: [InDows:module] specialize-scripts
# Uncomment the features you want. Most need a reboot to finish (the first boot covers that).

$features = @(
    # 'Microsoft-Hyper-V-All'              # Hyper-V (Pro/Enterprise only)
    # 'Microsoft-Windows-Subsystem-Linux'  # WSL (legacy feature; the winget 'Microsoft.WSL' app is newer)
    # 'VirtualMachinePlatform'             # required by WSL2 / sandboxes
    # 'Containers-DisposableClientVM'      # Windows Sandbox (Pro/Enterprise only)
    # 'NetFx3'                             # .NET Framework 3.5
)

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-optional-features.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== optional-features start ==='
if (-not $features) { Log 'no features selected (all lines commented out)'; }
foreach ($f in $features) {
    try { Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart -ErrorAction Stop | Out-Null; Log "enabled $f" }
    catch { Log "failed ${f}: $_" }
}
Log '=== optional-features done ==='
