# InDows module: gpu-tuning  -- MANUAL, run AFTER your GPU driver is installed.
#
# This is NOT wired into autounattend.xml: on a fresh install only the basic Microsoft display driver is
# present, so the vendor registry keys don't exist yet. Install your real NVIDIA/AMD/Intel driver first,
# then run this once in an elevated PowerShell.  Risk: advanced / hardware-dependent.

$ErrorActionPreference = 'Continue'
$log = "$env:TEMP\InDows-gpu-tuning.log"
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log; Write-Host $m }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}

$class = 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}'
Log '=== gpu-tuning start ==='
Get-ChildItem $class -ErrorAction SilentlyContinue | Where-Object { $_.PSChildName -match '^\d{4}$' } | ForEach-Object {
    $p = $_.PSPath
    $d = (Get-ItemProperty -Path $p -Name 'DriverDesc' -ErrorAction SilentlyContinue).DriverDesc
    if (-not $d) { return }
    if     ($d -match 'NVIDIA')      { RegDword $p 'DisableDynamicPstate' 1; Log "NVIDIA ($d): DisableDynamicPstate = 1" }
    elseif ($d -match 'AMD|Radeon')  { RegDword $p 'EnableUlps' 0;           Log "AMD ($d): EnableUlps = 0" }
    else                             { Log "skip ($d): no tweak for this vendor" }
}
Log '=== gpu-tuning done (reboot to apply) ==='
