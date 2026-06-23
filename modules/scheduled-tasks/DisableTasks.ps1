# InDows module: scheduled-tasks
# Disables telemetry / CEIP / error-reporting scheduled tasks.
# Anchor: [InDows:module] first-logon-scripts  (the Task Scheduler service is fully up at logon)

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-scheduled-tasks.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

$tasks = @(
    @{ p = '\Microsoft\Windows\Customer Experience Improvement Program\'; n = 'Consolidator' }
    @{ p = '\Microsoft\Windows\Customer Experience Improvement Program\'; n = 'UsbCeip' }
    @{ p = '\Microsoft\Windows\Application Experience\';                  n = 'Microsoft Compatibility Appraiser' }
    @{ p = '\Microsoft\Windows\Application Experience\';                  n = 'ProgramDataUpdater' }
    @{ p = '\Microsoft\Windows\Windows Error Reporting\';                 n = 'QueueReporting' }
    @{ p = '\Microsoft\Windows\Feedback\Siuf\';                           n = 'DmClient' }
    @{ p = '\Microsoft\Windows\Feedback\Siuf\';                           n = 'DmClientOnScenarioDownload' }
    @{ p = '\Microsoft\Windows\DiskDiagnostic\';                          n = 'Microsoft-Windows-DiskDiagnosticDataCollector' }
)

Log '=== scheduled-tasks start ==='
foreach ($t in $tasks) {
    try { Disable-ScheduledTask -TaskPath $t.p -TaskName $t.n -ErrorAction Stop | Out-Null; Log "disabled $($t.p)$($t.n)" }
    catch { Log "skip $($t.p)$($t.n): $_" }
}
Log '=== scheduled-tasks done ==='
