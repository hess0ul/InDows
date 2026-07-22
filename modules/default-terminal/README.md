# default-terminal

Makes **Windows Terminal** the default terminal host — new console windows (cmd, PowerShell) open in
the modern tabbed Terminal instead of the legacy console.

- **Anchor:** `[InDows:module] default-user-scripts` — per-user, written to the default profile.
- **What:** sets `DelegationConsole` / `DelegationTerminal` under `HKCU\Console\%%Startup` to the Windows Terminal GUIDs.
- **Note:** Windows 11 ships Windows Terminal. Reversible in Terminal → Settings → Default terminal application.
