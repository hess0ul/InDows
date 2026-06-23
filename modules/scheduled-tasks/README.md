# Module: `scheduled-tasks`

Disable the **telemetry / CEIP / error-reporting** scheduled tasks (Compatibility Appraiser,
Consolidator, QueueReporting, …).

- **Script:** [`DisableTasks.ps1`](DisableTasks.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts`
- **Risk:** 🟢 Safe (disabled, not deleted — re-enable with `Enable-ScheduledTask`)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

Tasks that don't exist on your edition are skipped (logged). Add or remove entries in the `$tasks` list.
