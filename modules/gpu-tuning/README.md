# Module: `gpu-tuning`

Mild GPU power-state tweaks (NVIDIA `DisableDynamicPstate`, AMD `EnableUlps = 0`) that can reduce
micro-stutter on some cards.

- **Script:** [`GpuTuning.ps1`](GpuTuning.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts`
- **⚠️ Manual by design:** the vendor registry keys only exist **after** you install your real GPU driver.
  Wired at first logon it runs *before* that, so it's a harmless no-op on a fresh install — run it by hand after the driver.
- **When:** ⚠️ **MANUAL — run it yourself after installing your GPU driver.** It is *not* an
  autounattend module: on a fresh install the vendor registry keys don't exist yet.
- **Risk:** 🟡 Advanced — hardware/driver-dependent. Aggressive power-gating tweaks are **not** included
  (they cause instability on some cards).

## How to run

1. Install Windows and your **real GPU driver** (NVIDIA/AMD/Intel).
2. Open **PowerShell as Administrator**.
3. `Set-ExecutionPolicy -Scope Process Bypass -Force; .\GpuTuning.ps1`
4. Reboot.

To revert, delete the `DisableDynamicPstate` / `EnableUlps` value the script added (the log lists the
exact key), or just reinstall the driver.
