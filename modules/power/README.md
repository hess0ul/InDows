# Module: `power`

Turn off **Hibernate + Fast Startup** and switch on the **Ultimate Performance** power plan.

- **Script:** [`Power.ps1`](Power.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts` (powercfg needs a live session)
- **Risk:** 🟡 Advanced — great on a **desktop**, not ideal on a **laptop** (worse battery, no hibernate)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## What it does

- `powercfg /hibernate off` — removes the hiberfile and **Fast Startup** (also fixes the occasional
  "Fast Startup ate my dual-boot / shutdown state" issues).
- Sets `HiberbootEnabled = 0` for good measure.
- Reveals and activates the hidden **Ultimate Performance** plan (keeps CPU/devices at full power).

Commented options in the script (also desktop-oriented): **disable CPU power throttling** and **disable USB
selective suspend**.

## Laptops

If this is a laptop, **skip this module** (or keep only the hibernate-off part and drop the Ultimate
Performance activation). Ultimate Performance disables most power-saving and will hurt battery life.
