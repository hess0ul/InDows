# developer-tools

Opt-in developer conveniences — **off by default**. Bundles `sudo` and Developer Mode.

- **Anchor:** `[InDows:module] specialize-scripts` — machine-wide, before first boot.
- **What:**
  - `sudo` enabled in **force-new-window** mode (`…\CurrentVersion\Sudo\Enabled = 1`) — the safest mode, never inline.
  - **Developer Mode** (`AppModelUnlock\AllowDevelopmentWithoutDevLicense` + `AllowAllTrustedApps` = 1) — sideload + run unsigned trusted apps.
- **⚠️ Risk:** these lower guardrails on purpose (elevation via sudo, running unsigned apps). Enable only what you actually need.
