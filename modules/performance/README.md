# Module: `performance`

Small responsiveness tweaks: favor **foreground apps** and relax the **multimedia scheduler** caps.

- **Script:** [`Performance.ps1`](Performance.ps1)
- **Anchor:** `[InDows:module] specialize-scripts` (machine-wide)
- **Risk:** 🟢 Safe (reversible registry values)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## What it sets

| Tweak | Key | Value |
| --- | --- | --- |
| Foreground boost | `…\Control\PriorityControl\Win32PrioritySeparation` | `38` |
| System responsiveness | `…\Multimedia\SystemProfile\SystemResponsiveness` | `10` |
| Network throttling off | `…\SystemProfile\NetworkThrottlingIndex` | `0xFFFFFFFF` |
| Games task priority | `…\SystemProfile\Tasks\Games` | GPU 8 / Prio 6 / High |

## Notes

- The active tweaks are mild and widely-used. A bigger hammer — **single-svchost (`SvcHostSplit`)** — is
  included but **commented out** (set it to your RAM in KB). Service hardening lives in `services-tune`,
  power-throttling in `power`.
- `NetworkThrottlingIndex = 0xFFFFFFFF` lifts a multimedia network cap; harmless on a modern desktop, but
  if you ever see odd audio/stream timing, set it back to `10` (the default) or `0x0a`.
