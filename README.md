# InDows

> **Declare your Windows 11 once — install it on autopilot.**
> Write one file to a USB stick, boot, walk away, and come back to a clean, debloated, ready-to-use
> machine. No clicking through forty settings pages, no post-install tinkering.

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![Windows 11](https://img.shields.io/badge/Windows-11-0078D6?logo=windows11&logoColor=white)
![PowerShell](https://img.shields.io/badge/PowerShell-5391FE?logo=powershell&logoColor=white)
![Setup: declarative](https://img.shields.io/badge/setup-declarative-success)
![Modules: 32](https://img.shields.io/badge/modules-32-brightgreen)

InDows is a single **`autounattend.xml`** you drop at the root of a Windows 11 install stick. The **base**
is deliberately generic — it carries **nothing personal**, just a sane, debloated Windows. Everything tied
to *you* (account, computer name, language, app list, disk layout…) is an **opt-in module** you bolt on
yourself, one precise action at a time.

---

## ✨ Highlights

- 🧹 **Debloated, not gutted** — strips the junk but keeps Store, Terminal, winget and anything removing it would break.
- 🔒 **Private by default** — telemetry, Copilot and the ad ID off; Defender and Windows Update left intact.
- ⚡ **Clean & quick** — hardware-requirement bypass (TPM / Secure Boot / RAM) and conservative UI/perf tweaks.
- 🧩 **Modular** — 32 opt-in modules (privacy, debloat, UI, gaming, services, DNS, identity…), each with its own tutorial.
- 📦 **Apps on first boot** — installs your essentials via winget on a live progress screen, then cleans up after itself.

## 🚀 Quick start

1. Download the official **Windows 11** ISO from Microsoft.
2. Write it to a USB stick with **[Rufus](https://rufus.ie)** (latest).
3. Copy this repo's **`autounattend.xml`** to the **root** of the stick.
4. *(optional)* Open `autounattend.xml`, find a module anchor, and paste in the module(s) you want — each
   module's `README.md` says exactly where.
5. Boot the target PC and install. The base handles the generic setup; you handle (or a module automates)
   the personal screens.

## 🔧 How it works

Everything runs off one file — **`autounattend.xml`** — which Windows Setup reads automatically from the
**root of the USB stick**.

- The **base** applies only **generic, universal** changes (see the table below).
- A **module** is a small, documented snippet you paste into `autounattend.xml` to add one precise
  behaviour (e.g. *create a local account automatically*). Each lives in `modules/<name>/` with the exact
  snippet **and** a tutorial telling you *where* to paste it and *what* to replace.

The base ships **anchor comments** (one per insertion point) so each tutorial can point you straight to
the right spot — e.g. the `username` module's README tells you to paste it next to:

```xml
<!-- [InDows:module] account -->
```

## 🧱 Base vs. modules

| In the **base** (generic) | Available as a **module** (your choice) |
| --- | --- |
| Hardware requirement bypass (TPM / Secure Boot / RAM) | `username` — create a local account, unattended |
| Debloat (remove preinstalled apps, optional features) | `computer-name` — set the PC name |
| Privacy / telemetry / Copilot off | `locale-keyboard` — language & keyboard layout |
| Conservative UI tweaks (visual perf, declutter Start/taskbar) | `timezone` — set the time zone |
| Windows Update sane defaults | `apps` — customize the installed app list |
| Minimal app install (Brave + 7-Zip) via winget | `disk` — automated disk/partition layout |

> The base installs only a couple of **essentials** (a browser + an archiver). After setup the PC
> **reboots once on its own**; **log in** and the apps install on a progress screen. Edit
> `configuration.dsc.yaml` to change that list (see the `apps` module).

## 🧩 Adding a module

Modules come in two flavours; every folder ships a **`README.md`** with where it plugs in, what to
replace, the available options, and the gotchas:

- **Snippet modules** (`snippet.xml`) — XML you paste into `autounattend.xml`, with `__PLACEHOLDERS__` to
  replace (`username`, `computer-name`, `locale-keyboard`, `timezone`, `disk`, `autologon`).
- **Script modules** (`<Name>.ps1`) — a PowerShell script the base runs during setup; you add one `<File>`
  node plus one command at the matching anchor (see
  [`modules/HOWTO-script-modules.md`](modules/HOWTO-script-modules.md)). Everything else is a script module.

Start with [`modules/username`](modules/username/) — the reference example — then browse
[`modules/CATALOG.md`](modules/CATALOG.md) for the full map of tweaks InDows offers.

## 📁 Repository layout

```
autounattend.xml                 # the BASE (generic) - ready to use
bootstrap.ps1                    # first-logon app installer (two-stage)
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

## 🙏 Credits

Tweak values, lists and revert logic are **distilled** (not copied) from these open-source projects —
all credit to their authors:

- **[WinUtil](https://github.com/christitustech/winutil)** — Chris Titus Tech
- **[Win11Debloat](https://github.com/Raphire/Win11Debloat)** — Raphire
- **[optimizerDuck](https://github.com/itsfatduck/optimizerDuck)** — itsfatduck
- **[unattend-generator](https://github.com/cschneegans/unattend-generator)** — Christoph Schneegans

Released under the [MIT License](LICENSE). Full attribution in [NOTICE.md](NOTICE.md).
