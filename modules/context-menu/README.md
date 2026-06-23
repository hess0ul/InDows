# Module: `context-menu`

Bring back the **Windows 10 classic right-click menu**, so you don't have to click "Show more options"
every time.

- **Script:** [`ContextMenu.ps1`](ContextMenu.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## How it works

It registers an **empty** `InprocServer32` under the Win11 context-menu CLSID
`{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}`, which makes Explorer fall back to the full classic menu. To
revert, delete that CLSID key.
