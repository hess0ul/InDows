# Module: `remove-onedrive`

Uninstall **OneDrive** and hide its File Explorer entry.

- **Script:** [`RemoveOneDrive.ps1`](RemoveOneDrive.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts`
- **Risk:** 🟢 Safe (it's just the OneDrive client; your files in the cloud are untouched)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

Runs `OneDriveSetup.exe /uninstall` and sets `System.IsPinnedToNameSpaceTree = 0` so OneDrive no longer
shows in the Explorer sidebar. If you only want it gone from the sidebar (but still installed), keep just
the `RegDword` lines.
