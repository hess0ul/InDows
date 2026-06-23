# Module: `dark-mode`

Set the **dark theme** for new users.

- **Script:** [`DarkMode.ps1`](DarkMode.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe (cosmetic)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Options

| Line | Effect |
| --- | --- |
| `AppsUseLightTheme = 0` | dark **apps** |
| `SystemUsesLightTheme = 0` | dark **taskbar / Start / notifications** |

Comment out either line to keep that part light (e.g. **dark apps + light taskbar** = keep only the
first line). Comment both and it does nothing.
