# InDows module: desktop-icons
# Shows "This PC" and "Recycle Bin" on the desktop for new users.  (0 = show, 1 = hide)
# Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-desktop-icons.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$np = 'Registry::HKEY_USERS\DefaultUser\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\HideDesktopIcons\NewStartPanel'

Log '=== desktop-icons start ==='
RegDword $np '{20D04FE0-3AEA-1069-A2D8-08002B30309D}' 0   # This PC
RegDword $np '{645FF040-5081-101B-9F08-00AA002F954E}' 0   # Recycle Bin
# Uncomment to also show:
# RegDword $np '{59031a47-3f72-44a7-89c5-5595fe6b30ee}' 0  # User's files
# RegDword $np '{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}' 0  # Network
# RegDword $np '{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}' 0  # Control Panel
Log '=== desktop-icons done ==='
