# InDows

A clean, **declarative** Windows 11 install you write to a USB stick once and let run — no manual
tweaking afterwards. This is the **generic** edition: a sane, debloated Windows 11 base that contains
**nothing personal**. Everything tied to *you* (account, computer name, language, keyboard layout,
time zone, your app list, disk layout…) is an **opt-in module** you add yourself.

> Status: **base `autounattend.xml` ready · module library complete (32 modules).** The base
> produces a clean, debloated, hardware-bypassed Windows 11 and lets you go through the normal first-run
> screens (region, keyboard, account) yourself. Modules automate those — and a lot more (privacy, debloat,
> UI, gaming, services, DNS…) — one precise action at a time.

## How it works

InDows is built around a single file: **`autounattend.xml`**. Windows Setup reads it automatically when
it sits at the **root of your USB install stick** (write the official Windows 11 ISO with
[Rufus](https://rufus.ie), then drop `autounattend.xml` at the root of the stick).

- The **base** `autounattend.xml` applies only **generic, universal** changes (see the table below).
- A **module** is a small, documented snippet you paste into `autounattend.xml` to add one precise
  behaviour (e.g. *create a user account automatically*). Each module lives in `modules/<name>/` and
  ships with the exact XML snippet **and** a tutorial that tells you *where* to paste it and *what* to
  replace.

The base file contains **anchor comments** like:

```xml
<!-- [InDows:module] account — paste the "username" module here (see modules/username) -->
```

so each module's tutorial can point you at the right spot.

## What's in the base vs. what's a module

| In the **base** (generic) | Available as a **module** (your choice) |
| --- | --- |
| Hardware requirement bypass (TPM / Secure Boot / RAM) | `username` — create a local account, unattended |
| Debloat (remove preinstalled apps, optional features) | `computer-name` — set the PC name |
| Privacy / telemetry / Copilot off | `locale-keyboard` — language & keyboard layout |
| Conservative UI tweaks (visual perf, declutter Start/taskbar) | `timezone` — set the time zone |
| Windows Update sane defaults | `apps` — customize the installed app list |
| Minimal app install (Brave + 7-Zip) via winget | `disk` — automated disk/partition layout |

> The base installs only a couple of **essentials** (a browser + an archiver). After the install
> finishes, the PC **reboots once on its own**; **log in** and the apps install on a progress screen.
> Edit `configuration.dsc.yaml` to change that list (see the `apps` module).

## Repository layout

```
autounattend.xml                 # the BASE (generic, English) - ready to use
bootstrap.ps1                    # first-logon app installer (two-stage, generic)
configuration.dsc.yaml           # base "essentials" catalog (Brave + 7-Zip)
LICENSE  ·  NOTICE.md             # MIT + credits to the projects we distil from
docs/    base-recipe.md          # how the base autounattend is generated
modules/
  CATALOG.md                roadmap + status of every module
  HOWTO-script-modules.md   how to integrate a .ps1 module
  identity:    username/ computer-name/ locale-keyboard/ timezone/ autologon/
  appearance:  dark-mode/ wallpaper/ visual-effects/
  ui:          explorer-ui/ taskbar-startmenu/ context-menu/ desktop-icons/
  privacy:     disable-ai/ disable-bing-search/ privacy-telemetry/ remove-edge/ remove-onedrive/ debloat-appx/
  system:      services-tune/ scheduled-tasks/ optional-features/ windows-update/ security/ misc/ wifi/
  perf/games:  gaming/ performance/ power/ network-dns/ gpu-tuning(manual)/
  docs:        apps/ (customize the app list)  ·  disk/ (automated layout — advanced)
```

## Quick start

1. Download the official **Windows 11** ISO from Microsoft.
2. Write it to a USB stick with **Rufus** (latest).
3. Copy this repo's **`autounattend.xml`** to the **root** of the stick.
4. *(optional)* Open `autounattend.xml`, find a module anchor, and paste in the module(s) you want
   (follow the module's `README.md`).
5. Boot the target PC from the stick and install. The base handles the generic setup; you handle (or a
   module automates) the personal screens.

## Adding a module — the pattern

Modules come in two flavours; every folder ships a **`README.md`** that says exactly where it plugs in,
what to replace, the available options, and the gotchas:

- **Snippet modules** (`snippet.xml`) — XML you paste into `autounattend.xml`, with `__PLACEHOLDERS__` to
  replace. These cover identity/account/disk: `username`, `computer-name`, `locale-keyboard`, `timezone`,
  `disk`, `autologon`.
- **Script modules** (`<Name>.ps1`) — a PowerShell script the base runs during setup; you add one `<File>`
  node plus one command at the matching anchor (see
  [`modules/HOWTO-script-modules.md`](modules/HOWTO-script-modules.md)). Everything else is a script module.

Start with [`modules/username`](modules/username/) — it's the reference example. See
[`modules/CATALOG.md`](modules/CATALOG.md) for the full roadmap of tweaks InDows aims to offer (distilled
from WinUtil, Win11Debloat and optimizerDuck).
