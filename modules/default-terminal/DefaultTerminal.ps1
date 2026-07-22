# InDows module: default-terminal
# Make Windows Terminal the default terminal host for new users. Anchor: [InDows:module] default-user-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-default-terminal.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}
$U = 'Registry::HKEY_USERS\DefaultUser'

Log '=== default-terminal start ==='
# Default terminal application = Windows Terminal (both delegation GUIDs point at it)
RegString "$U\Console\%%Startup" 'DelegationConsole' '{2EACA947-7F5F-4CFA-BA87-8F7FBEEFBE69}'
RegString "$U\Console\%%Startup" 'DelegationTerminal' '{E12CFF52-A866-4C77-9A90-F570A7AA2C6B}'
Log '=== default-terminal done ==='
