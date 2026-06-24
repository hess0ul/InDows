# InDows module: explorer-ui
# File Explorer defaults for new users. Each line is independent -- comment out what you don't want.
# Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-explorer-ui.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$U   = 'Registry::HKEY_USERS\DefaultUser'
$adv = "$U\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"

Log '=== explorer-ui start ==='
RegDword $adv 'HideFileExt' 0                   # show known file extensions
RegDword $adv 'Hidden' 1                        # show hidden files (protected OS files stay hidden)
RegDword $adv 'LaunchTo' 1                      # open Explorer to "This PC" (2 = Home)
RegDword $adv 'ShowDriveLettersFirst' 4         # drive letter before the label (e.g. "C: Windows")
RegDword $adv 'NavPaneExpandToCurrentFolder' 1  # auto-expand the nav pane to the open folder
# Hide the "Gallery" entry in the navigation pane
RegDword "$U\Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}" 'System.IsPinnedToNameSpaceTree' 0

# --- optional (commented) ---
# Compact mode (tighter row spacing):
# RegDword $adv 'UseCompactMode' 1
# Also show PROTECTED operating-system files (clutter / risk of editing the wrong thing):
# RegDword $adv 'ShowSuperHidden' 1
# Hide the "Home" entry in the nav pane (leaves This PC):
# RegDword "$U\Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}" 'System.IsPinnedToNameSpaceTree' 0
Log '=== explorer-ui done ==='
