# InDows (public) - first-logon bootstrap. TWO-STAGE, generic.
#
# Installs the apps listed in configuration.dsc.yaml at first logon, with a visible progress screen.
#
# WHY TWO STAGES: winget is not ready yet while FirstLogonCommands run; it works fine in an
# established session. So STAGE 1 registers a task and reboots; STAGE 2 (next logon) installs the
# apps, then cleans up. Assumes a VANILLA Windows 11 ISO (nothing to undo about Windows Update).
# No auto-logon: after the automatic reboot, just log in once and the install runs on its own.
#
# Target: Windows PowerShell 5.1. Runs elevated. UI text is ASCII-only (avoid console mojibake).

param([switch]$Stage2)

$ErrorActionPreference = 'Continue'
$dir        = 'C:\Windows\Setup\Scripts'
$log        = Join-Path $dir 'InDows-bootstrap.log'
$configPath = Join-Path $dir 'configuration.dsc.yaml'
$selfPath   = Join-Path $dir 'bootstrap.ps1'
$taskName   = 'InDows-install'

function Log([string]$m)  { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function Step([string]$m) { Write-Host ''; Write-Host "// $m //" -ForegroundColor Cyan;     Log "STEP: $m" }
function Ok([string]$m)   { Write-Host "    \\ $m \\"            -ForegroundColor Green;    Log "OK: $m" }
function Fail([string]$m) { Write-Host "    !! $m !!"            -ForegroundColor Red;      Log "FAIL: $m" }
function Note([string]$m) { Write-Host "    $m"                  -ForegroundColor DarkGray; Log $m }
function Wait-Key {
    # Keep this window open so the summary stays readable; close on ANY key (Enter, Esc, ...).
    Write-Host ''
    Write-Host "  Logs are saved to: $log" -ForegroundColor DarkGray
    Write-Host '  Press any key to exit...'  -ForegroundColor Yellow
    try { [void][System.Console]::ReadKey($true) }
    catch { try { [void]$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown') } catch { Start-Sleep -Seconds 60 } }
}

function Get-Winget {
    # The REAL package binary, NOT the WindowsApps App Execution Alias (mangles args at first logon).
    $p = Get-AppxPackage -Name 'Microsoft.DesktopAppInstaller' -ErrorAction SilentlyContinue |
         Sort-Object { try { [version]$_.Version } catch { [version]'0.0' } } | Select-Object -Last 1
    if ($p) { $exe = Join-Path $p.InstallLocation 'winget.exe'; if (Test-Path -LiteralPath $exe) { return $exe } }
    return (Get-Command 'winget.exe' -ErrorAction SilentlyContinue).Source
}

# ======================== STAGE 1 - first logon (winget not ready): register Stage 2, reboot ========================
if (-not $Stage2) {
    Log '=== InDows STAGE 1 (first logon) start ==='
    try {
        $a = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-WindowStyle Maximized -ExecutionPolicy Unrestricted -NoProfile -File `"$selfPath`" -Stage2"
        $t = New-ScheduledTaskTrigger -AtLogOn
        $pr = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Highest
        Register-ScheduledTask -TaskName $taskName -Action $a -Trigger $t -Principal $pr -Force -ErrorAction Stop | Out-Null
    } catch { Log "FATAL: could not register Stage 2 task: $_ - aborting (no reboot)."; return }
    Log '=== STAGE 1 done - rebooting; log in once and the apps will install ==='
    Start-Sleep -Seconds 3
    Restart-Computer -Force
    return
}

# ======================== STAGE 2 - after reboot, established session (VISIBLE UI) ========================
try { $Host.UI.RawUI.WindowTitle = 'InDows - installation' } catch {}
try { Clear-Host } catch {}
# Decode winget output as UTF-8 so captured text is clean (no wide-char mojibake).
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}
Write-Host ''
Write-Host '  ###################################################' -ForegroundColor Cyan
Write-Host '  #              InDows - installation              #' -ForegroundColor Cyan
Write-Host '  ###################################################' -ForegroundColor Cyan
Log '=== InDows STAGE 2 (post-reboot install) start ==='

Step 'Preparing winget'
for ($i = 0; $i -lt 30 -and -not (Get-Winget); $i++) {
    Get-AppxPackage -Name 'Microsoft.DesktopAppInstaller' -ErrorAction SilentlyContinue |
        ForEach-Object { Add-AppxPackage -DisableDevelopmentMode -Register "$($_.InstallLocation)\AppXManifest.xml" -ErrorAction SilentlyContinue }
    Start-Sleep -Seconds 10
}
$winget = Get-Winget
if (-not $winget) { Fail 'winget unavailable - will retry at next logon'; Log '=== STAGE 2 deferred (no winget) ==='; return }
Ok 'winget ready'

Step 'Waiting for network'
function Test-Online {
    try {
        if (-not [System.Net.NetworkInformation.NetworkInterface]::GetIsNetworkAvailable()) { return $false }
        (Invoke-WebRequest -Uri 'http://www.msftconnecttest.com/connecttest.txt' -UseBasicParsing -TimeoutSec 8).Content.Trim() -eq 'Microsoft Connect Test'
    } catch { return $false }
}
$deadline = (Get-Date).AddMinutes(5)
while (-not (Test-Online) -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 10 }
if (-not (Test-Online)) { Fail 'No network - will retry at next logon'; Log '=== STAGE 2 deferred (no network) ==='; return }
Ok 'Network OK'

# Accept the winget source agreements once.
& $winget list --accept-source-agreements *> $null

# Install every package from the catalog (configuration.dsc.yaml = the id source-of-truth).
$ids = Select-String -Path $configPath -Pattern 'id:\s*([\w.+-]+),\s*source:\s*winget' |
    Where-Object { $_.Line -notmatch '^\s*#' } |
    ForEach-Object { $_.Matches.Groups[1].Value } | Select-Object -Unique
$total = $ids.Count
Step "Installing applications ($total total)"
Log "Installing $total packages from the catalog..."
$base = '--exact', '--silent', '--accept-package-agreements', '--accept-source-agreements', '--disable-interactivity'
$failed = @(); $done = 0; $n = 0
foreach ($id in $ids) {
    $n++
    Write-Progress -Activity 'InDows - Installing applications' -Status "$id  ($n/$total)" -PercentComplete ([int](($n - 1) / $total * 100))
    # Idempotent: skip if already installed (so the retry doesn't redo everything).
    & $winget list --id $id --exact --accept-source-agreements *> $null
    if ($LASTEXITCODE -eq 0) { Note "already installed: $id  ($n/$total)"; $done++; continue }
    Write-Host ''
    Write-Host ("// [{0}/{1}] Installing {2} via winget //" -f $n, $total, $id) -ForegroundColor Cyan
    Log "STEP: install $id ($n/$total)"
    # Keep winget's noisy progress OUT of the main log: capture it; on failure log a filtered tail only.
    $wout = & $winget install --id $id @base 2>&1 | Out-String
    $code = $LASTEXITCODE
    if ($code -in 0, 3010, 1641) {
        Ok "$id installed successfully"; $done++
    } else {
        Fail "$id failed (code $code)"; $failed += $id
        ($wout -split "`r?`n" | Where-Object { $_ -match '\S' -and $_ -notmatch '^[\s\\|/-]*$' -and $_ -notmatch '[^\x00-\x7F]' -and $_ -notmatch '^\s*\d{1,3}\s*%\s*$' } | Select-Object -Last 15) | ForEach-Object { Log "    winget> $($_.Trim())" }
    }
}
Write-Progress -Activity 'InDows - Installing applications' -Completed
Log ("Install loop done. Failures: {0}" -f $(if ($failed) { $failed -join ', ' } else { 'none' }))
Write-Host ''
Write-Host ("  Result: {0}/{1} installed, {2} failed." -f $done, $total, $failed.Count) -ForegroundColor $(if ($failed.Count) { 'Yellow' } else { 'Green' })

if ($failed.Count -gt 0) {
    Note ('Failed: ' + ($failed -join ', '))
    Note 'The task will retry at next logon.'
    Log '=== STAGE 2 done (some failures; will retry) ==='
    Wait-Key
    return
}

# Success - clean up: remove the task and the InDows payload files. Keep the .log as proof.
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $configPath -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $selfPath   -Force -ErrorAction SilentlyContinue
Log '=== InDows STAGE 2 done - install complete, cleaned up ==='

Write-Host ''
Write-Host '  ###################################################' -ForegroundColor Green
Write-Host '  #          InDows: installation complete!         #' -ForegroundColor Green
Write-Host '  ###################################################' -ForegroundColor Green
Write-Host ("  {0}/{1} applications installed." -f $done, $total) -ForegroundColor Green
Wait-Key
