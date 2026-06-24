# InDows module: remove-onedrive
# Uninstalls OneDrive and hides its File Explorer entry.
# Anchor: [InDows:module] first-logon-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-remove-onedrive.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== remove-onedrive start ==='
Get-Process 'OneDrive' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
$setup = "$env:SystemRoot\SysWOW64\OneDriveSetup.exe"
if (-not (Test-Path $setup)) { $setup = "$env:SystemRoot\System32\OneDriveSetup.exe" }
if (Test-Path $setup) { Start-Process -FilePath $setup -ArgumentList '/uninstall' -Wait -ErrorAction SilentlyContinue; Log "uninstalled via $setup" }
else { Log 'OneDriveSetup.exe not found.' }

# Hide the OneDrive entry from the Explorer navigation pane (both 64-bit and 32-bit views)
foreach ($k in 'HKLM:\SOFTWARE\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}',
               'HKLM:\SOFTWARE\Wow6432Node\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}') {
    RegDword $k 'System.IsPinnedToNameSpaceTree' 0
}
Log '=== remove-onedrive done ==='
