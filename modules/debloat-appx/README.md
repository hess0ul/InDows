# Module: `debloat-appx`

Remove **extra** preinstalled Store apps, on top of whatever the base already strips (the base's debloat
is chosen in the generator — see [`../../docs/base-recipe.md`](../../docs/base-recipe.md)).

- **Script:** [`DebloatAppx.ps1`](DebloatAppx.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟢 Safe for the listed apps · 🟡 if you add apps you might actually use
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Editing the list

`$apps` holds package family names (no version). Removing the **provisioned** package stops new users
from getting it; removing the **installed** package clears it for existing users. Find names with:

```powershell
Get-AppxProvisionedPackage -Online | Select DisplayName
```

> Don't add things you want (e.g. `Microsoft.WindowsTerminal`, `Microsoft.WindowsStore`,
> `Microsoft.WindowsNotepad`, the `…VCLibs…`/`…UI.Xaml…` runtimes). Removing the Store or the runtimes
> breaks other apps.
