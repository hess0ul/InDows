# Module: `username`

Create a **local user account automatically** during install, so Windows doesn't stop at the
first-run *"Who's going to use this device?"* screen.

Without this module, the base leaves account creation **interactive** (you type it yourself at first
boot). Add this module when you want that step fully **unattended**.

## What it does

- Adds one local account with the name and password you choose.
- Puts it in the **Administrators** group (so it's a usable main account).
- Skips the interactive account-creation screen.

## How to integrate

1. Open the base **`autounattend.xml`**.
2. Find the anchor in the **`oobeSystem`** pass, inside the
   `Microsoft-Windows-Shell-Setup` component:

   ```xml
   <!-- [InDows:module] account -->
   ```

3. Paste the contents of [`snippet.xml`](snippet.xml) **right after** that anchor (the
   `<UserAccounts>` element must be a direct child of `Microsoft-Windows-Shell-Setup`).
4. Replace the placeholders:

   | Placeholder | Replace with | Example |
   | --- | --- | --- |
   | `__USERNAME__` | the account name | `Alex` |
   | `__PASSWORD__` | the password, plain text | `hunter2` |

## Options

- **No password** — use an empty value:

  ```xml
  <Password>
      <Value></Value>
      <PlainText>true</PlainText>
  </Password>
  ```

  (A blank password is required if you later want hands-free auto-logon; see *Gotchas*.)

- **Standard (non-admin) account** — change the group:

  ```xml
  <Group>Users</Group>
  ```

- **Several accounts** — add more `<LocalAccount wcm:action="add">…</LocalAccount>` blocks inside
  `<LocalAccounts>`.

- **Also skip the remaining first-run screens** (privacy, OneDrive, etc.) — that belongs to the base
  `<OOBE>` settings, not to this module. This module only handles the account itself.

## Gotchas

- ⚠️ **Clear-text password.** The value is stored as-is in `autounattend.xml`. **Never commit a real
  password to a public repository** — keep your filled-in file on your own machine only. Placeholders
  (`__PASSWORD__`) are what live in the repo.
- **Auto-logon / app-install flows.** Some setups (e.g. the `apps-winget` module) reboot once and rely
  on **auto-logon**. Auto-logon needs the account's password too; the simplest combo is a **blank
  password** account. With a real password, you'll log in once manually after the reboot.
- **Microsoft account vs local.** This module creates a **local** account. Joining a Microsoft account
  unattended is a different (and more brittle) flow and isn't covered here.
