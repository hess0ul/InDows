# Module: `apps` — customize the installed app list

The base installs a couple of **essentials** (Brave + 7-Zip) at first logon via
[`../../bootstrap.ps1`](../../bootstrap.ps1), reading the catalog
[`../../configuration.dsc.yaml`](../../configuration.dsc.yaml). This "module" is just **how to change
that list** — there's no snippet to paste.

## Add / remove apps

Edit `configuration.dsc.yaml`. Each app is one entry:

```yaml
    - resource: Microsoft.WinGet.DSC/WinGetPackage
      id: vscode
      settings: { id: Microsoft.VisualStudioCode, source: winget }   # comment
```

The bootstrap reads the `id: <pkg>, source: winget` part, so keep that on one line.

## Find package ids

```powershell
winget search <name>      # e.g. winget search "visual studio code"
winget show <id>          # confirm the exact id
```

## Per-app install options (advanced)

The base `bootstrap.ps1` installs each app with `--silent`. If an app needs custom installer flags (e.g.
"add to PATH"), give winget a `--override` with the installer's own flags. In the install loop, special-case
the id, e.g.:

```powershell
if ($id -eq 'Python.Python.3.14') {
    & $winget install --id $id @base --override '/quiet PrependPath=1'
} else {
    & $winget install --id $id @base
}
```

## Apps that refuse to run elevated

A few installers (e.g. **Spotify**) won't run from the elevated install session and fail with *"cannot be
run from an administrator context"*. The simplest fix is to **install those one or two apps manually after
setup**. (If you want them automated, run winget for that id from a scheduled task created with
`-RunLevel Limited`, which runs de-elevated.)
