# InDows module: wifi
# Joins a Wi-Fi network at first logon (handy when there's no Ethernet). Anchor: [InDows:module] first-logon-scripts
# Edit $ssid / $password below. WPA2-Personal (the common home setup).
# SECURITY: the password is in clear text here -> keep this file local.

$ssid     = '__SSID__'
$password = '__PASSWORD__'

$ErrorActionPreference = 'Continue'
$log = 'C:\Windows\Temp\InDows-wifi.log'
function Log([string]$m) { "{0}  {1}" -f (Get-Date -Format 'o'), $m | Add-Content -Path $log }

Log "=== wifi start (SSID: $ssid) ==="
$wlanXml = @"
<?xml version="1.0"?>
<WLANProfile xmlns="http://www.microsoft.com/networking/WLAN/profile/v1">
  <name>$ssid</name>
  <SSIDConfig><SSID><name>$ssid</name></SSID></SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>auto</connectionMode>
  <MSM><security>
    <authEncryption><authentication>WPA2PSK</authentication><encryption>AES</encryption><useOneX>false</useOneX></authEncryption>
    <sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>$password</keyMaterial></sharedKey>
  </security></MSM>
</WLANProfile>
"@
$tmp = "$env:TEMP\InDows-wifi-profile.xml"
$wlanXml | Out-File -FilePath $tmp -Encoding ascii
& netsh.exe wlan add profile filename="$tmp" user=all *>> $log
& netsh.exe wlan connect name="$ssid" *>> $log
Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
Log '=== wifi done ==='
