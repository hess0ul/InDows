# InDows module: context-menu
# Restores the Windows 10 classic right-click menu (no "Show more options" extra click).
# Anchor: [InDows:module] default-user-scripts
# Mechanism: register an EMPTY InprocServer32 for the Win11 menu CLSID, which disables it.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-context-menu.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

$key = 'Registry::HKEY_USERS\DefaultUser\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32'

Log '=== context-menu start ==='
New-Item -Path $key -Force -ErrorAction SilentlyContinue | Out-Null
# Set the key's (Default) value to an EMPTY string (this is what disables the new menu).
Set-Item -Path $key -Value '' -ErrorAction SilentlyContinue
Log '=== context-menu done ==='
