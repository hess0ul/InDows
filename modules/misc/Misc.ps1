# InDows module: misc
# Small machine-wide quality-of-life tweaks. Anchor: [InDows:module] specialize-scripts
# Each is independent -- comment out the ones you don't want.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-misc.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== misc start ==='
# RTC kept in UTC -> no clock fight with a Linux dual-boot. (Only useful if you dual-boot Linux.)
RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control\TimeZoneInformation' 'RealTimeIsUniversal' 1
# Allow long file paths (> 260 characters)
RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' 'LongPathsEnabled' 1
# NumLock ON at the logon screen (.DEFAULT = the sign-in profile; for your own session it follows your last state)
RegString 'Registry::HKEY_USERS\.DEFAULT\Control Panel\Keyboard' 'InitialKeyboardIndicators' '2'
# Verbose "Starting Windows / Signing out..." status messages instead of the spinner
RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System' 'VerboseStatus' 1

# --- optional (commented) ---
# Skip the lock screen, go straight to the password box:
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Personalization' 'NoLockScreen' 1
# Disable the "shake a window to minimize the others" gesture:
# RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' 'NoWindowMinimizingShortcuts' 1
Log '=== misc done ==='
