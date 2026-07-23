# InDows module: remote-desktop
# Turns on Remote Desktop (RDP) so you can connect to this PC from another machine.
# Network Level Authentication stays ON (the secure default). Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-remote-desktop.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== remote-desktop start ==='
# Allow incoming Remote Desktop connections (0 = allowed).
reg.exe add "HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server" /v fDenyTSConnections /t REG_DWORD /d 0 /f
# Require Network Level Authentication (client must authenticate before a session is created).
reg.exe add "HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp" /v UserAuthentication /t REG_DWORD /d 1 /f
# Open the Windows Firewall for the Remote Desktop rule group.
Enable-NetFirewallRule -DisplayGroup 'Remote Desktop' -ErrorAction SilentlyContinue
Log '=== remote-desktop done ==='
