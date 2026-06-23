# Module: `optional-features`

Turn on Windows optional features (Hyper-V, WSL, Windows Sandbox, .NET 3.5, …) via DISM.

- **Script:** [`OptionalFeatures.ps1`](OptionalFeatures.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

By default **everything is commented out** — uncomment the features you want in the script.

| Feature name | Gives you |
| --- | --- |
| `Microsoft-Hyper-V-All` | Hyper-V (Pro/Enterprise) |
| `VirtualMachinePlatform` | platform required by WSL2 / sandboxes |
| `Microsoft-Windows-Subsystem-Linux` | WSL (legacy; the `Microsoft.WSL` winget app is the modern path) |
| `Containers-DisposableClientVM` | Windows Sandbox (Pro/Enterprise) |
| `NetFx3` | .NET Framework 3.5 |

Most features need a reboot to finish — the install's own first boot handles that.
