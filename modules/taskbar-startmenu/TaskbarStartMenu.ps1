# InDows module: taskbar-startmenu
# Declutter the taskbar and Start menu for new users. Each line is independent.
# Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-taskbar-startmenu.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$U   = 'Registry::HKEY_USERS\DefaultUser'
$adv = "$U\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"

Log '=== taskbar-startmenu start ==='
# --- taskbar ---
RegDword $adv 'TaskbarAl' 0              # align left
RegDword $adv 'TaskbarDa' 0              # Widgets off
RegDword $adv 'TaskbarMn' 0              # Chat off
RegDword $adv 'ShowTaskViewButton' 0     # Task View button off
RegDword $adv 'TaskbarEndTask' 1         # add "End task" to the taskbar right-click menu
RegDword "$U\Software\Microsoft\Windows\CurrentVersion\Search" 'SearchboxTaskbarMode' 1   # 0 hidden / 1 icon / 2 box / 3 box+label
# --- Start menu ---
RegDword $adv 'Start_IrisRecommendations' 0   # cloud "Recommended" suggestions off
RegDword $adv 'Start_TrackDocs' 0             # don't list recently opened files
RegDword $adv 'Start_TrackProgs' 0            # don't list recently added/used apps
RegDword "$U\Software\Microsoft\Windows\CurrentVersion\Start" 'ShowRecentList' 0   # don't show recently added apps in Start
RegDword "$U\Software\Policies\Microsoft\Windows\Explorer" 'ShowRunAsDifferentUserInStart' 1   # add "Run as different user" to Start

# --- optional (commented) ---
# More pins vs more recommendations (1 = more pins, 2 = more recommendations, 0 = default):
# RegDword $adv 'Start_Layout' 1
Log '=== taskbar-startmenu done ==='
