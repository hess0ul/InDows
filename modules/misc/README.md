# Module: `misc`

Small machine-wide quality-of-life tweaks.

- **Script:** [`Misc.ps1`](Misc.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

| Tweak | Key | When you want it |
| --- | --- | --- |
| RTC in UTC | `…\TimeZoneInformation\RealTimeIsUniversal = 1` | **only if you dual-boot Linux** (stops the clock jumping by your UTC offset) |
| Long paths | `…\FileSystem\LongPathsEnabled = 1` | dev work with deep folder trees |
| NumLock at logon | `HKU\.DEFAULT\…\Keyboard\InitialKeyboardIndicators = 2` | desktop keyboard with a numpad |
| Verbose status | `…\Policies\System\VerboseStatus = 1` | see "Starting Windows / Signing out" detail |

Commented options in the script: **skip the lock screen** (`NoLockScreen`) and **disable the window-shake
gesture**. Each line is independent — comment out anything you don't want.
