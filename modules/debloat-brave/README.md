# debloat-brave

Turns off Brave's built-in extras via **enterprise policy** — Rewards/Wallet/VPN, Leo AI, Tor,
News/Talk, and the various telemetry pings. 12 independent policy DWORDs.

- **Anchor:** `[InDows:module] specialize-scripts` — machine-wide, before first boot.
- **What:** 12 DWORDs under `HKLM\SOFTWARE\Policies\BraveSoftware\Brave` (the WinUtil Brave-debloat set).
- **Note:** only affects Brave (installed by the base essentials). Comment out any feature you use.
