# Module: `services-tune`

Set some Windows services to **Manual** (start on demand), with an optional **hardening** preset.

- **Script:** [`ServicesTune.ps1`](ServicesTune.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟢 Safe preset · 🔴 hardening preset (read the warnings)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Presets (set `$preset` at the top)

- **`safe`** — rarely-used services to *Manual* (Maps, Fax, RetailDemo, WMPNetworkSvc, Phone, Wallet,
  Geolocation, Smart Card, Biometric, Touch Keyboard…). Zero risk: they still start on demand.
- **`hardening`** — also **disables** telemetry/remote services (`DiagTrack`, `dmwappushservice`,
  `RemoteRegistry`, `RetailDemo`, `WerSvc`) and sets `SysMain` to Manual.

Edit the `$toManual` / `$toDisabled` lists in the script to add or remove services.

> ⚠️ **hardening warnings:** don't disable `RemoteRegistry`/WinRM if you manage this PC remotely. This
> module deliberately stays small and **never touches networking services** (`Dhcp`, `Dnscache`,
> `NlaSvc`, `WlanSvc`) — disabling those is the classic way to lose your connection. The "287-service"
> matrices from other tools are intentionally **not** reproduced wholesale.
