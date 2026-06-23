# InDows module: disable-ai
# Turns off Windows AI features, machine-wide. Each line is independent -- comment out what you want to keep.
# Runs elevated during the "specialize" pass. Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-disable-ai.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$path, [string]$name, [int]$val) {
    New-Item -Path $path -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $path -Name $name -Value $val -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$ai = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI'

Log '=== disable-ai start ==='
# Windows Copilot
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot' 'TurnOffWindowsCopilot' 1
# Recall: "AI data analysis" + saved screen snapshots
RegDword $ai 'DisableAIDataAnalysis' 1
RegDword $ai 'TurnOffSavingSnapshots' 1
# Click to Do (the "act on what's on screen" feature)
RegDword $ai 'DisableClickToDo' 1
# Microsoft Edge: hide the Copilot/Discover sidebar
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' 'HubsSidebarEnabled' 0
RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' 'ShowSidebarButton' 0

# --- optional (commented) ---
# Generative AI features inside Edge (Compose, etc.):
# RegDword 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' 'GenAILocalFoundationalModelSettings' 1
# The taskbar Copilot BUTTON is a per-user setting -> put it in a default-user tweak if you want it hidden
# for new users: ...\Explorer\Advanced\ShowCopilotButton = 0
Log '=== disable-ai done ==='
