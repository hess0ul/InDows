# How to add a script module

Some modules ship a PowerShell script (`*.ps1`) instead of a one-line XML setting. Adding one to your
`autounattend.xml` is always the same **two steps**.

## 1. Ship the script — add a `<File>` node

Inside `<Extensions>` (next to the other `<File>` nodes), paste a node whose body is the **entire content**
of the module's `.ps1`:

```xml
<File path="C:\Windows\Setup\Scripts\NAME.ps1"><![CDATA[
... paste the whole .ps1 here ...
]]></File>
```

(`NAME.ps1` = the script's file name, given in the module's README.)

## 2. Call it at the right anchor

Each script runs at one of three points. Find the matching anchor comment in the base and add the call
right after it. The module's README says **which anchor** to use.

| Anchor (a comment in the base) | When it runs | `Order` to use | Use for |
| --- | --- | --- | --- |
| `[InDows:module] default-user-scripts` | specialize, **default-user hive loaded** | **6–49** | per-user settings (`HKEY_USERS\DefaultUser`) |
| `[InDows:module] specialize-scripts` | specialize, **after** the hive is unloaded | **51+** | HKLM, services, DISM, AppX removal |
| `[InDows:module] first-logon-scripts` | first logon (live session) | **3+** | DNS, GPU, devices |

The `specialize` and `default-user` anchors sit in a `RunSynchronous` sequence with **numbered `Order`s**.
The base reserves: `1–4` = setup + `DefaultUser.ps1`, `5` = InDows base tweaks, **`50` = unload the
default-user hive**. So the window matters:

- **`default-user-scripts`** runs *before* the unload → pick an `Order` in **6–49** (the hive is loaded).
- **`specialize-scripts`** runs *after* the unload → pick **51+** (the hive is gone — `HKLM` only there).
- Adding several modules to the same anchor? Give each its own number (7, 8, 9…). Duplicates are rejected.

```xml
<RunSynchronousCommand wcm:action="add">
  <Order>10</Order>
  <Path>powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Windows\Setup\Scripts\NAME.ps1"</Path>
</RunSynchronousCommand>
```

The **first-logon** anchor sits in `FirstLogonCommands` — use `<SynchronousCommand>` / `<CommandLine>`
instead (`Order` 3+, since the base uses 1 and 2):

```xml
<SynchronousCommand wcm:action="add">
  <Order>3</Order>
  <CommandLine>powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Windows\Setup\Scripts\NAME.ps1"</CommandLine>
</SynchronousCommand>
```

> Per-user (`default-user`) scripts write to `HKEY_USERS\DefaultUser` — the base keeps that hive loaded
> from `Order 3` until the `Order 50` unload, so a `6–49` command lands inside that window and every **new**
> user inherits the settings. Don't use `HKCU` there (no user exists yet).

Each script writes a small log to `C:\Windows\Temp\InDows-<module>.log` for troubleshooting.
