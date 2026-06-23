# Module: `wallpaper`

Set a default **desktop wallpaper** and (optionally) the **lock-screen** image.

- **Script:** [`SetWallpaper.ps1`](SetWallpaper.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts`
- **Risk:** 🟢 Safe (cosmetic)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Get your image onto the disk first

The script just *points* at an image — the file has to already be on the PC. Options:

- Drop it in a folder the script reads, e.g. `C:\Windows\Web\Wallpaper\InDows\bg.jpg`. To ship it on the
  install USB, put it under `$OEM$\$$\Web\Wallpaper\InDows\` (the base's WinPE copies `$OEM$` to the disk).
- Or download it at first logon — add an `Invoke-WebRequest` line before the `Set-ItemProperty` calls.

Then set the paths at the top of the script:

```powershell
$desktop = 'C:\Windows\Web\Wallpaper\InDows\bg.jpg'
$lock    = ''      # leave empty to keep the default lock screen
$style   = '10'    # 10 Fill · 6 Fit · 2 Stretch · 0 Center · 22 Span
```
