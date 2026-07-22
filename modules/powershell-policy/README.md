# powershell-policy

Sets the machine PowerShell **execution policy to RemoteSigned** — you can run your own local
`.ps1` scripts, while unsigned downloaded ones are still blocked.

- **Anchor:** `[InDows:module] specialize-scripts` — machine-wide, before first boot.
- **What:** writes `HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell\ExecutionPolicy = RemoteSigned`.
- **Note:** RemoteSigned is the common developer default; a Group Policy can still override it.
