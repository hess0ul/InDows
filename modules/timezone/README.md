# Module: `timezone`

Set the time zone automatically during install. Without it, Windows uses the region chosen at first run.

## How to integrate

1. Open the base **`autounattend.xml`**.
2. Find the anchor in the **`specialize`** pass, inside the `Microsoft-Windows-Shell-Setup` component:

   ```xml
   <!-- [InDows:module] specialize-shell-setup -->
   ```

3. Paste [`snippet.xml`](snippet.xml) right after it and replace `__TIMEZONE__`.

## Finding the right ID

Use the **Windows time-zone ID**, not a city or a UTC offset. List all of them:

```powershell
tzutil /l          # or:  Get-TimeZone -ListAvailable | Select Id
```

| Region | ID |
| --- | --- |
| France / Central Europe | `Romance Standard Time` |
| UK / Ireland | `GMT Standard Time` |
| US Eastern | `Eastern Standard Time` |
| US Pacific | `Pacific Standard Time` |
| Coordinated Universal Time | `UTC` |

## Example

```xml
<TimeZone>Romance Standard Time</TimeZone>
```

## Note

Same `Microsoft-Windows-Shell-Setup` component as the [`computer-name`](../computer-name/) module — if you
use both, keep both elements inside the **one** component.
