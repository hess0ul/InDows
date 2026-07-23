# Module: `remote-desktop` — enable Remote Desktop (RDP)

Turns on **Remote Desktop** so you can connect to this PC from another machine, with **Network Level
Authentication** kept on (the secure default).

- **Script:** [`RemoteDesktop.ps1`](RemoteDesktop.ps1)
- **Where:** `specialize` pass
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟡 opens an inbound network service. Only enable it on a machine/network you control; RDP should
  be reached over a VPN or a trusted LAN, never exposed straight to the internet.

## What it does

- `fDenyTSConnections = 0` — allow incoming RDP connections.
- `RDP-Tcp\UserAuthentication = 1` — require Network Level Authentication.
- `Enable-NetFirewallRule -DisplayGroup 'Remote Desktop'` — open the firewall for it.
