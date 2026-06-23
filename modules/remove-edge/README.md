# Module: `remove-edge`

Remove **Microsoft Edge** (Chromium) and the legacy **Internet Explorer** capability during install.

Not in the base on purpose: Edge removal is **opinionated**, can have **side effects**, and the clean
uninstall is **region-gated** (see below). Add this module only if you really want Edge gone.

## ⚠️ Read first

- **Region:** Microsoft only allows the *official* Edge uninstall in the **EEA** (European Economic Area
  — e.g. **France**). There it works. **Outside the EEA** Microsoft blocks it, and this module will
  simply log that and leave Edge in place (no region workaround is shipped here).
- **WebView2 is kept** — lots of apps depend on the Edge WebView2 Runtime, so it is *not* removed.
- **Side effects:** Widgets, a few Settings deep-links, and the default PDF/HTML handler may change.
- **Internet Explorer** on Windows 11 is usually already gone; the script removes its capability only if
  it's still present.

## What it does

Runs [`RemoveEdge.ps1`](RemoveEdge.ps1), which:

1. removes the `Browser.InternetExplorer` capability if installed;
2. uninstalls Edge via its own `setup.exe --uninstall --system-level --force-uninstall`;
3. sets `DoNotUpdateToEdgeWithChromium` so EdgeUpdate doesn't bring it back;
4. cleans up Edge desktop shortcuts.

## How to integrate

This module needs **two** edits to `autounattend.xml` (it's a script, not a one-line setting):

### 1. Ship the script — add a `<File>` node

Inside `<Extensions>` (next to the other `<File>` nodes), paste a node whose body is the **full content
of `RemoveEdge.ps1`**:

```xml
<File path="C:\Windows\Setup\Scripts\RemoveEdge.ps1"><![CDATA[
... paste the entire contents of RemoveEdge.ps1 here ...
]]></File>
```

### 2. Call it during `specialize`

Find this anchor in the base, in the `specialize` script sequence:

```xml
<!-- [InDows:module] specialize-scripts -->
```

and add the call right after it:

```powershell
& 'C:\Windows\Setup\Scripts\RemoveEdge.ps1';
```

That's it — Edge is removed before you ever reach the desktop. A log is written to
`C:\Windows\Temp\InDows-remove-edge.log` (check the `setup.exe` exit code there if Edge survives:
a non-zero code almost always means the uninstall was blocked in your region).

## Options

- **Keep Internet Explorer**, remove only Edge: delete the IE block at the top of `RemoveEdge.ps1`.
- **Run at first logon instead of specialize**: call the script from `FirstLogonCommands` instead of the
  specialize sequence (slightly later, but works the same).

## Further reading

- Manual method (FR): <https://www.malekal.com/desinstaller-supprimer-edge/>
