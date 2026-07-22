# network-profile

Sets the active network connection to the **Private** profile, so LAN device discovery and
file/printer sharing work at home.

- **Anchor:** `[InDows:module] first-logon-scripts` — needs a live network, so it runs at first logon.
- **What:** `Set-NetConnectionProfile -NetworkCategory Private` on every active connection.
- **Note:** only meaningful on a network you trust (home/office). On public Wi-Fi, keep it Public.
