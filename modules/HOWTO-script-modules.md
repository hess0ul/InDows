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

| Anchor (a comment in the base) | When it runs | Use for |
| --- | --- | --- |
| `[InDows:module] specialize-scripts` | specialize (machine, before first boot) | HKLM, services, DISM, AppX removal |
| `[InDows:module] default-user-scripts` | specialize, with the **default-user hive loaded** | per-user settings (`HKEY_USERS\DefaultUser`) |
| `[InDows:module] first-logon-scripts` | first logon (live session) | DNS, GPU, devices |

**specialize** and **default-user** anchors sit in a PowerShell `RunSynchronous` sequence — add a command
with the **next free, unique `Order` number** (if you add several modules to the same pass, each one needs
its own `Order`: 90, 91, 92… — duplicates are rejected):

```xml
<RunSynchronousCommand wcm:action="add">
  <Order>99</Order>
  <Path>powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Windows\Setup\Scripts\NAME.ps1"</Path>
</RunSynchronousCommand>
```

The **first-logon** anchor sits in `FirstLogonCommands` — use `<SynchronousCommand>` / `<CommandLine>`
instead:

```xml
<SynchronousCommand wcm:action="add">
  <Order>99</Order>
  <CommandLine>powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Windows\Setup\Scripts\NAME.ps1"</CommandLine>
</SynchronousCommand>
```

> Per-user (`default-user`) scripts write to `HKEY_USERS\DefaultUser` — the base loads that hive around the
> anchor, so every **new** user inherits the settings. Don't use `HKCU` there (no user exists yet).

Each script writes a small log to `C:\Windows\Temp\InDows-<module>.log` for troubleshooting.
