# Module: `wifi`

Join a **Wi-Fi network automatically** at first logon — handy on a laptop or a PC with no Ethernet
(so the [`apps`](../apps/) install can reach the internet on its own).

- **Script:** [`ConnectWifi.ps1`](ConnectWifi.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts` (Wi-Fi needs a live session)
- **Risk:** 🟡 — the Wi-Fi password is stored in clear text in the script.
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Setup

Edit the top of the script:

```powershell
$ssid     = 'MyNetwork'
$password = 'MyWifiPassword'
```

Assumes **WPA2-Personal** (the usual home setup). For WPA3 or enterprise (802.1X) networks, export a
working profile with `netsh wlan export profile` and embed that XML instead.

> ⚠️ Clear-text password — keep your filled-in file local, don't commit it.
