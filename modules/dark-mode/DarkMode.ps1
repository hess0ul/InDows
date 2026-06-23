# InDows module: dark-mode
# Sets the dark theme for new users. Anchor: [InDows:module] default-user-scripts
# OPTIONS: comment out a line to keep that part light (e.g. dark apps + light taskbar).

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-dark-mode.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegDword([string]$p, [string]$n, [int]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType DWord -Force -ErrorAction SilentlyContinue | Out-Null
}
$P = 'Registry::HKEY_USERS\DefaultUser\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize'

Log '=== dark-mode start ==='
RegDword $P 'AppsUseLightTheme'   0   # 0 = dark apps      (comment out to keep apps light)
RegDword $P 'SystemUsesLightTheme' 0  # 0 = dark taskbar/Start (comment out to keep the shell light)
Log '=== dark-mode done ==='
