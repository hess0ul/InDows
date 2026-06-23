# Module: `explorer-ui`

File Explorer defaults for new users: **show file extensions**, **show hidden files**, and **open to
"This PC"** instead of Home.

- **Script:** [`ExplorerUI.ps1`](ExplorerUI.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

| Tweak | Value |
| --- | --- |
| `HideFileExt` | `0` (show extensions) |
| `Hidden` | `1` (show hidden — protected OS files stay hidden) |
| `LaunchTo` | `1` (This PC) — set `2` for Home |
| `ShowDriveLettersFirst` | `4` (drive letter before the label, e.g. "C: Windows") |
| `NavPaneExpandToCurrentFolder` | `1` (auto-expand the nav pane) |
| Hide "Gallery" | nav-pane Gallery entry off |

Commented options in the script: **compact mode**, **show protected OS files**, **hide "Home"**. Each
line is independent — comment out what you don't want.
