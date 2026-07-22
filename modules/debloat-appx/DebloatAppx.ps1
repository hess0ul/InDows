# InDows module: debloat-appx
# Removes preinstalled Store apps. Each line below removes ONE app; the InDows GUI ticks/unticks them
# individually (a commented line is kept). Removing a provisioned package stops NEW users getting it too.
# Anchor: [InDows:module] specialize-scripts
#
# NOTE: run STANDALONE (outside the GUI) this removes every app listed below. Through the InDows GUI you
# choose exactly which ones to remove; the safe, low-regret apps are pre-ticked, the rest are left to you.

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-debloat-appx.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function Remove-App([string]$a) {
    Get-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -eq $a } |
        ForEach-Object { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -ErrorAction SilentlyContinue | Out-Null; Log "provisioned removed: $a" }
    Get-AppxPackage -AllUsers -Name $a -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction SilentlyContinue; Log "installed removed: $a" }
}

Log '=== debloat-appx start ==='
# === start ===  (each line = one app the GUI can toggle)
Remove-App 'Microsoft.BingNews'                     # Bing News
Remove-App 'Microsoft.BingWeather'                  # MSN Weather (the taskbar weather widget is unaffected)
Remove-App 'Microsoft.BingSearch'                   # web results in the Start-menu search box
Remove-App 'Microsoft.ZuneMusic'                    # Windows Media Player (music)
Remove-App 'Microsoft.ZuneVideo'                    # Films & TV (video)
Remove-App 'Microsoft.WindowsFeedbackHub'           # Feedback Hub
Remove-App 'Microsoft.MicrosoftSolitaireCollection' # Solitaire Collection
Remove-App 'Microsoft.People'                       # People (contacts; retired Dec 2024)
Remove-App 'Microsoft.windowscommunicationsapps'    # Mail & Calendar (retired Dec 2024, use new Outlook)
Remove-App 'Clipchamp.Clipchamp'                    # Clipchamp video editor
Remove-App 'MicrosoftTeams'                         # personal Teams / Chat (work Teams is a separate app)
Remove-App 'Microsoft.Todos'                        # Microsoft To Do
Remove-App 'Microsoft.PowerAutomateDesktop'         # Power Automate (desktop flows)
Remove-App 'Microsoft.GamingApp'                    # Xbox app = the Game Pass client (KEEP if you use Game Pass)
Remove-App 'Microsoft.GetHelp'                      # Get Help (some troubleshooters route through it)
Remove-App 'Microsoft.Getstarted'                   # Tips
Remove-App 'Microsoft.WindowsMaps'                  # Maps
Remove-App 'Microsoft.MicrosoftOfficeHub'           # Microsoft 365 (Office) hub tile (not Office itself)
Remove-App 'Microsoft.WindowsAlarms'                # Clock (alarms, timers, Focus Sessions)
Remove-App 'Microsoft.MixedReality.Portal'          # Mixed Reality Portal (deprecated)
Remove-App 'Microsoft.549981C3F5F10'                # Cortana (retired)
# === done ===
Log '=== debloat-appx done ==='
