# InDows module: security
#
# DEFAULT (active): stop AUTOMATIC BitLocker device encryption (defensible - avoids surprise lockout).
#
# OPTIONAL "danger zone" (commented out below): tweaks that LOWER your protection. They are commented on
# purpose so you cannot enable them by accident. Read each warning, then uncomment ONLY what you
# understand and accept. Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-security.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== security start ==='

# ---- DEFAULT (safe): prevent automatic BitLocker device encryption ----
RegDword 'HKLM:\SYSTEM\CurrentControlSet\Control\BitLocker' 'PreventDeviceEncryption' 1
Log 'PreventDeviceEncryption = 1 (automatic BitLocker off)'

# ============================================================================
#   ##############  DANGER ZONE  ##############
#   Everything below LOWERS your security. ALL of it is disabled (commented).
#   Uncomment a block ONLY if you fully understand and accept the consequences.
#   Don't do this just because a video told you it makes the PC "faster".
# ============================================================================

# --- Lower / disable UAC ----------------------------------------------------
#   RISK: programs can elevate to admin WITHOUT asking you. Malware then runs with full rights silently.
#   Option A (softer): never show the admin consent prompt, UAC engine stays on.
# RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System' 'ConsentPromptBehaviorAdmin' 0
#   Option B (nuclear): fully disable UAC. Also breaks Store/UWP apps and some Settings pages.
# RegDword 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System' 'EnableLUA' 0
# Log 'UAC lowered (DANGER)'

# --- Disable SmartScreen ----------------------------------------------------
#   RISK: Windows stops warning you about malicious / unknown downloads, apps and websites.
# RegString 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer' 'SmartScreenEnabled' 'Off'
# RegDword  'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' 'EnableSmartScreen' 0
# RegDword  'HKLM:\SOFTWARE\Policies\Microsoft\Edge' 'SmartScreenEnabled' 0
# Log 'SmartScreen disabled (DANGER)'

# --- Microsoft Defender (GRANULAR - pick exactly what you turn off) ----------
#   Some people want Defender fully gone; others just want to drop cloud/sample sharing but KEEP real-time
#   scanning. Uncomment only the pieces you want off.
#   NOTE: all of these are IGNORED while Tamper Protection is ON (the default). To make them stick you must
#   first disable Tamper Protection by hand in Windows Security. InDows does NOT bypass it.
#
#   (a) Cloud-delivered protection off (keeps local real-time scanning):
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet' 'SpynetReporting' 0
#   (b) Automatic sample submission off (2 = never send):
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Spynet' 'SubmitSamplesConsent' 2
#   (c) Real-time monitoring off (still installed, just not scanning live):
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' 'DisableRealtimeMonitoring' 1
#   (d) THE WHOLE ENGINE off (nuclear - no antivirus at all):
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' 'DisableAntiSpyware' 1
# Log 'Defender tweaks applied (only effective if Tamper Protection was already off) (DANGER)'

Log '=== security done ==='
