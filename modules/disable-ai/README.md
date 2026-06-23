# Module: `disable-ai`

Turn off Windows AI features — **Copilot**, **Recall** (screen snapshots), and the **Edge Copilot
sidebar** — machine-wide.

- **Script:** [`DisableAI.ps1`](DisableAI.ps1)
- **Anchor:** `[InDows:module] specialize-scripts` (runs once, machine-wide, before first boot)
- **Risk:** 🟢 Safe (policy keys, reversible)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## What it sets

| Feature | Key |
| --- | --- |
| Copilot | `…\WindowsCopilot\TurnOffWindowsCopilot = 1` |
| Recall | `…\WindowsAI\DisableAIDataAnalysis = 1` + `TurnOffSavingSnapshots = 1` |
| Click to Do | `…\WindowsAI\DisableClickToDo = 1` |
| Edge sidebar | `…\Edge\HubsSidebarEnabled = 0`, `ShowSidebarButton = 0` |

## Options

- Keep Edge's sidebar (remove the two `Edge` lines from the script).
- Edge **generative AI** (Compose, etc.) is a commented option in the script.
- The taskbar Copilot **button** is a per-user setting — to hide it for new users add
  `…\Explorer\Advanced\ShowCopilotButton = 0` to a `default-user` tweak; this module stays machine-wide.
