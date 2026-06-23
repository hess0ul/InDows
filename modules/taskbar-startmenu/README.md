# Module: `taskbar-startmenu`

Declutter the taskbar and Start menu for new users.

- **Script:** [`TaskbarStartMenu.ps1`](TaskbarStartMenu.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

| Tweak | Value |
| --- | --- |
| Taskbar alignment | `TaskbarAl = 0` (left) |
| Widgets | `TaskbarDa = 0` (off) |
| Chat | `TaskbarMn = 0` (off) |
| Task View | `ShowTaskViewButton = 0` (off) |
| End task | `TaskbarEndTask = 1` (adds "End task" to the taskbar right-click) |
| Start "Recommended" | `Start_IrisRecommendations = 0` (cloud suggestions off) |
| Recent files/apps | `Start_TrackDocs = 0`, `Start_TrackProgs = 0` (off) |
| Search box | `SearchboxTaskbarMode = 1` (icon only; `0` hidden, `2` box, `3` box+label) |

> Fully removing the Start "Recommended" *section* isn't possible on Home/Pro via a single switch — this
> just turns off the cloud/iris suggestions inside it.
