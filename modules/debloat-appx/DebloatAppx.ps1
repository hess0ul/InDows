# InDows module: debloat-appx
# Removes EXTRA preinstalled Store apps, on top of whatever the base already strips.
# Anchor: [InDows:module] specialize-scripts
# Pick a preset below; edit the lists to taste. Removing a provisioned package stops NEW users getting it too.

$preset = 'safe'   # [InDows:param PRESET] the InDows GUI can switch this to 'extended'

# SAFE (default): clear bloat almost nobody wants.
$safe = @(
    'Microsoft.BingNews'
    'Microsoft.BingWeather'
    'Microsoft.BingSearch'
    'Microsoft.GamingApp'
    'Microsoft.ZuneMusic'
    'Microsoft.ZuneVideo'
    'Microsoft.WindowsFeedbackHub'
    'Microsoft.MicrosoftSolitaireCollection'
    'Microsoft.People'
    'Microsoft.windowscommunicationsapps'   # Mail & Calendar
    'Clipchamp.Clipchamp'
    'MicrosoftTeams'                         # personal Teams
    'Microsoft.Todos'
    'Microsoft.PowerAutomateDesktop'
)

# EXTENDED: safe + a few more (some people use these, hence not in 'safe').
$extra = @(
    'Microsoft.GetHelp'
    'Microsoft.Getstarted'                   # Tips
    'Microsoft.WindowsMaps'
    'Microsoft.MicrosoftOfficeHub'           # Office hub
    'Microsoft.WindowsAlarms'
    'Microsoft.MixedReality.Portal'
    'Microsoft.549981C3F5F10'                # Cortana
)

$apps = if ($preset -eq 'extended') { $safe + $extra } else { $safe }

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-debloat-appx.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log '=== debloat-appx start ==='
foreach ($a in $apps) {
    Get-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -eq $a } |
        ForEach-Object { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -ErrorAction SilentlyContinue | Out-Null; Log "provisioned removed: $a" }
    Get-AppxPackage -AllUsers -Name $a -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction SilentlyContinue; Log "installed removed: $a" }
}
Log '=== debloat-appx done ==='
