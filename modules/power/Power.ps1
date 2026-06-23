# InDows module: power
# Turns Hibernate + Fast Startup OFF and activates the "Ultimate Performance" power plan.
# Anchor: [InDows:module] first-logon-scripts  (powercfg is most reliable in a live session)
#
# NOTE (laptops): hibernate off removes the hiberfile and Fast Startup. "Ultimate Performance" keeps the
# CPU/devices at full power -> great on a desktop, worse battery on a laptop.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-power.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== power start ==='
# Hibernate off (also disables Fast Startup, which relies on the hiberfile)
& powercfg.exe /hibernate off
New-Item -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Power' -Force -ErrorAction SilentlyContinue | Out-Null
New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Power' -Name 'HiberbootEnabled' -Value 0 -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null

# Ultimate Performance plan: reveal it, then activate the freshly created GUID.
$dup = (& powercfg.exe -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61) -join ' '
if ($dup -match '([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})') {
    & powercfg.exe /setactive $Matches[1]
    Log "Ultimate Performance activated ($($Matches[1]))"
} else {
    Log 'Could not create the Ultimate Performance plan (not available on this edition?).'
}

# --- ADVANCED (commented) - desktop-oriented, costs battery on a laptop ---
# Disable CPU power throttling (keeps cores at higher clocks):
# New-Item -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling' -Force -ErrorAction SilentlyContinue | Out-Null
# New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling' -Name 'PowerThrottlingOff' -Value 1 -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
# Disable USB selective suspend (USB devices never sleep -> fewer disconnect glitches, a bit more power):
# & powercfg.exe /SETACVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
# & powercfg.exe /SETDCVALUEINDEX SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
# & powercfg.exe /SETACTIVE SCHEME_CURRENT
Log '=== power done ==='
