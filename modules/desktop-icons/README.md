# Module: `desktop-icons`

Choose which **desktop icons** new users get. By default this module shows **This PC** and the **Recycle
Bin**.

- **Script:** [`DesktopIcons.ps1`](DesktopIcons.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Options

In the script, `0` = show, `1` = hide. The commented lines add **User's files**, **Network** and
**Control Panel**. To **hide** the Recycle Bin instead, set its line to `1`.

| Icon | CLSID |
| --- | --- |
| This PC | `{20D04FE0-3AEA-1069-A2D8-08002B30309D}` |
| Recycle Bin | `{645FF040-5081-101B-9F08-00AA002F954E}` |
| User's files | `{59031a47-3f72-44a7-89c5-5595fe6b30ee}` |
| Network | `{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}` |
| Control Panel | `{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}` |
