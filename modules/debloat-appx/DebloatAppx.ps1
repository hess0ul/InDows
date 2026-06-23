# InDows module: debloat-appx
# Removes EXTRA preinstalled Store apps, on top of whatever the base already strips.
# Anchor: [InDows:module] specialize-scripts
# Edit $apps to taste. Removing a provisioned package stops NEW users getting it too.

$apps = @(
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
