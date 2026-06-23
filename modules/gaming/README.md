# Module: `gaming`

Gaming-friendly defaults: **GameDVR / background recording OFF**, **Game Mode ON**, **mouse acceleration
OFF**.

- **Script:** [`Gaming.ps1`](Gaming.ps1)
- **Anchor:** `[InDows:module] default-user-scripts` (per-user settings for every new user + one machine policy)
- **Risk:** 🟢 Safe defaults · 🟡 the commented advanced options
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## What it sets (active)

| Tweak | Key |
| --- | --- |
| GameDVR off (policy) | `HKLM\…\Policies\Microsoft\Windows\GameDVR\AllowGameDVR = 0` |
| GameDVR off (user) | `…\System\GameConfigStore\GameDVR_Enabled = 0`, `…\GameDVR\AppCaptureEnabled = 0` |
| Game Mode on | `…\Software\Microsoft\GameBar\AutoGameModeEnabled = 1` |
| Mouse accel off | `Control Panel\Mouse\MouseSpeed/MouseThreshold1/MouseThreshold2 = 0` |

## Advanced (commented out in the script — 🟡 driver-dependent)

- **Fullscreen Optimizations OFF** (`GameDVR_FSEBehavior(Mode) = 2`) — can lower latency, can also break
  HDR/capture on some setups.
- **Hardware-Accelerated GPU Scheduling (HAGS)** (`HwSchMode = 2`) — good on recent GPUs, problematic on
  some older driver combos.

Uncomment them only if you know your GPU + driver + games like them. Keep mouse acceleration by removing
the three `MouseSpeed/Threshold` lines.
