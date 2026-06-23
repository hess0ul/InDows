# Module: `security`

Stop **automatic BitLocker device encryption**. On modern Windows 11 this can silently encrypt your
system drive on first setup — fine if you saved the recovery key, a lockout risk if you didn't.

- **Script:** [`Security.ps1`](Security.ps1)
- **Anchor:** `[InDows:module] specialize-scripts`
- **Risk:** 🟡 default (BitLocker auto-off) · 🔴 the optional "danger zone" below (UAC / SmartScreen /
  Defender) — commented out, opt-in only.
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## ☠️ Danger zone (commented out — read before enabling)

The script also contains tweaks that **lower** your security — **lower UAC**, **disable SmartScreen**,
**disable Microsoft Defender**. They are **commented out on purpose**: nothing happens unless you open
`Security.ps1` and deliberately uncomment a block. Each one carries an inline warning.

| Tweak | What you lose |
| --- | --- |
| Lower / disable UAC | programs can elevate to admin **without asking you** (malware too) |
| Disable SmartScreen | no more warnings about malicious / unknown downloads, apps, sites |
| Disable Defender | **no real-time antivirus** (only sane if you run another trusted AV) |

> 🔴 Don't enable these just because a video says they make the PC "faster" — they don't, they make it
> **less safe**. **Defender note:** the disable key is *ignored* while Tamper Protection is on (the
> default); InDows does **not** bypass Tamper Protection, so to truly turn Defender off you'd have to do
> it by hand in Windows Security first. Strongly discouraged.
