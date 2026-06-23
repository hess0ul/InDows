# InDows module: remove-edge
# Removes Microsoft Edge (Chromium) and the legacy Internet Explorer capability.
# Runs elevated, during the "specialize" pass (before first logon) or at first logon.
#
# GEOGRAPHY (read this): Microsoft only allows the *official* Edge uninstall in the EEA
# (European Economic Area, e.g. France). There, the command below works. Outside the EEA the
# uninstall is blocked by Microsoft; this script then just logs it and leaves Edge in place
# (enabling it elsewhere needs a region workaround that this module does NOT ship).
#
# KEPT ON PURPOSE: the Edge WebView2 Runtime (many apps depend on it) is NOT removed.
# EXPECT SIDE EFFECTS: Widgets, a few Settings deep links, and the default PDF/HTML handler may change.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-remove-edge.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== remove-edge start ==='

# --- Internet Explorer (usually already gone on Windows 11) ---
try {
    Get-WindowsCapability -Online -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like 'Browser.InternetExplorer*' -and $_.State -eq 'Installed' } |
        ForEach-Object { Log "Removing IE capability: $($_.Name)"; Remove-WindowsCapability -Online -Name $_.Name -ErrorAction SilentlyContinue | Out-Null }
} catch { Log "IE removal skipped: $_" }

# --- Microsoft Edge (Chromium): official uninstall via its own setup.exe ---
$setup = Get-ChildItem 'C:\Program Files (x86)\Microsoft\Edge\Application' -Recurse -Filter 'setup.exe' -ErrorAction SilentlyContinue |
         Where-Object { $_.FullName -match '\\Installer\\setup\.exe$' } | Select-Object -First 1
if ($setup) {
    Log "Uninstalling Edge via: $($setup.FullName)"
    & $setup.FullName '--uninstall' '--system-level' '--verbose-logging' '--force-uninstall'
    Log "Edge setup.exe exit: $LASTEXITCODE  (a non-zero code usually means the uninstall is blocked in your region)"
} else {
    Log 'Edge setup.exe not found (already removed?).'
}

# --- Stop EdgeUpdate from bringing Chromium Edge back ---
try {
    New-Item -Path 'HKLM:\SOFTWARE\Microsoft\EdgeUpdate' -Force -ErrorAction SilentlyContinue | Out-Null
    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\EdgeUpdate' -Name 'DoNotUpdateToEdgeWithChromium' -Value 1 -Type DWord -ErrorAction SilentlyContinue
} catch { Log "EdgeUpdate guard skipped: $_" }

# --- Tidy leftover desktop shortcuts ---
Get-ChildItem "$env:PUBLIC\Desktop", "$env:USERPROFILE\Desktop" -Filter '*Edge*.lnk' -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

Log '=== remove-edge done ==='
