# Module: `computer-name`

Set the computer name automatically during install. Without it, Windows assigns a random name
(`DESKTOP-XXXXXXX`) that you can still change later in Settings.

## How to integrate

1. Open the base **`autounattend.xml`**.
2. Find the anchor in the **`specialize`** pass, inside the `Microsoft-Windows-Shell-Setup` component:

   ```xml
   <!-- [InDows:module] specialize-shell-setup -->
   ```

3. Paste [`snippet.xml`](snippet.xml) right after it and replace `__COMPUTERNAME__`.

## Rules for the name

- **Max 15 characters.**
- Letters, digits and hyphens only — **no spaces, no dots**.
- Must **not** be all-numeric.
- A single `*` tells Windows to generate a random name explicitly.

## Example

```xml
<ComputerName>STUDIO-PC</ComputerName>
```

## Note

This is the same `Microsoft-Windows-Shell-Setup` component used by the [`timezone`](../timezone/) module —
if you use both, put both elements inside the **one** component, not two.
