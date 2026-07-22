# InDows module: powershell-policy
# Set the machine-wide PowerShell execution policy to RemoteSigned. Anchor: [InDows:module] specialize-scripts

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-powershell-policy.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }
function RegString([string]$p, [string]$n, [string]$v) {
    New-Item -Path $p -Force -ErrorAction SilentlyContinue | Out-Null
    New-ItemProperty -Path $p -Name $n -Value $v -PropertyType String -Force -ErrorAction SilentlyContinue | Out-Null
}

Log '=== powershell-policy start ==='
# LocalMachine execution policy = RemoteSigned (run your local scripts; signed remote ones only)
RegString 'HKLM:\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell' 'ExecutionPolicy' 'RemoteSigned'
Log '=== powershell-policy done ==='
