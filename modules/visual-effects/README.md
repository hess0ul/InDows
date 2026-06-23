# Module: `visual-effects`

Snappier visuals for new users: **no transparency**, **no taskbar animations**, **instant menus**, no
minimize/maximize animation, no desktop "peek", no Start-menu startup delay.

- **Script:** [`VisualEffects.ps1`](VisualEffects.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe (purely cosmetic / performance)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

Each tweak is an **independent, labeled line** in the script — comment out anything you want to **keep**.

> A **"best performance"** master switch (`VisualFXSetting = 2`) is included but **commented out**: it turns
> off basically *all* effects and shadows for a faster-but-plainer look. Uncomment it if you want the lot.
