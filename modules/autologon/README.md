# Module: `autologon`

Sign in **automatically** at boot, without typing the password. Useful for a kiosk, a home media PC, or
to make the [`apps`](../apps/) first-logon install fully hands-free.

- **Snippet:** [`snippet.xml`](snippet.xml)
- **Where:** `oobeSystem` → `Microsoft-Windows-Shell-Setup`, anchor `[InDows:module] account`
- **Risk:** 🟡 — anyone who powers on the PC is logged in as you; password is stored in clear text.

## How to integrate

Paste [`snippet.xml`](snippet.xml) after the `[InDows:module] account` anchor (same place as the
[`username`](../username/) module — they share the account), then replace `__USERNAME__` and
`__PASSWORD__`.

- **No password account?** use an empty value: `<Value></Value>`.
- **Permanent** auto-logon as written. For **one-time** (first boot only), add
  `<LogonCount>1</LogonCount>` inside `<AutoLogon>`.

> ⚠️ Clear-text password — keep your filled-in `autounattend.xml` local, never commit it.
