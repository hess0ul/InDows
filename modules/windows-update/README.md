# Module: `windows-update`

Tame Windows Update **without disabling it**: no peer-to-peer downloads, and no surprise reboots while
you're using the PC.

- **Script:** [`WindowsUpdate.ps1`](WindowsUpdate.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟢 Safe — updates stay **on** (recommended; InDows never ships an "updates off" tweak)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

| Tweak | Effect |
| --- | --- |
| `DODownloadMode = 0` | Delivery Optimization downloads from Microsoft only (no LAN/Internet peers) |
| `NoAutoRebootWithLoggedOnUsers = 1` | never auto-reboot while signed in |
