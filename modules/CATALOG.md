# InDows modules — catalog & roadmap

The full map of tweaks InDows aims to offer as opt-in modules, distilled from three open-source tools
(see [Credits](#credits)). Each row is a concrete tweak; modules group them by category. Nothing here is
applied by the base — you add what you want.

## Legend

**Risk**

- 🟢 **Safe** — reversible preference / cleanup, no security or stability impact.
- 🟡 **Advanced** — changes behaviour, or hardware/laptop/driver-dependent (may hurt as much as help).
- 🔴 **Risky** — can reduce security or break Windows features. Opt-in, clearly warned.

**When it runs (which base anchor the module plugs into)**

- **[S]** `specialize` — machine-wide (HKLM, services, DISM, provisioned AppX). Runs before first boot.
- **[U]** `default-user` — per-user settings written to the **default profile hive** (`C:\Users\Default\NTUSER.DAT`),
  so every new user inherits them. (HKCU tweaks can't target a live user at install time.)
- **[F]** `first-logon` — needs a live session: an active NIC (DNS), an installed driver (GPU), or enumerated
  devices (USB).

**Status** — ✅ done · 🚧 in progress · 📋 planned

> Base integration: the base `autounattend.xml` exposes **six anchors**. Script modules plug into
> `specialize-scripts` **[S]**, `default-user-scripts` **[U]** or `first-logon-scripts` **[F]**; snippet
> modules plug into `specialize-shell-setup` (computer-name / timezone), `oobe-international`
> (locale-keyboard) or `account` (username). Each module's README says which one to use.

---

## Debloat & apps

### `debloat-appx` — [S] 🟢/🟡 · ✅

Remove preinstalled Store apps (provisioned + per-user) so new users don't get them.
`Remove-AppxProvisionedPackage` (image) + `Remove-AppxPackage`. Source list: Win11Debloat `Config/Apps.json`
(~180 apps, ~120 default) + WinUtil debloat (~17). Presets: `safe`, `extended`, `gaming` (Xbox), `oem` (Dell/HP/Lenovo).
🟡 for anything a user might want (Terminal, Phone Link).

### `remove-edge` — [S] 🟡 (EEA only) · ✅ done

Uninstall Microsoft Edge + Internet Explorer capability. See [`remove-edge/`](remove-edge/).

### `remove-onedrive` — [S]/[U] 🟢 · ✅

Uninstall OneDrive (`OneDriveSetup.exe /uninstall`), remove its Explorer namespace, run-key, and setup from the
default profile.

---

## Privacy & telemetry

### `privacy-telemetry` — [S]+[U] 🟢/🟡 · ✅

| Tweak | Effect | Mechanism | Risk |
|---|---|---|---|
| Telemetry off | `AllowTelemetry=0` + DiagTrack/dmwappushservice disabled | reg (HKLM policy) + service | 🟡 |
| Advertising ID | disable per-user ad ID | reg [U] | 🟢 |
| Tailored experiences / input personalization | stop ink/text harvesting | reg [U] | 🟢 |
| Activity history | `PublishUserActivities/EnableActivityFeed=0` | reg (HKLM) | 🟢 |
| Location | consent=Deny, `lfsvc` manual | reg + service | 🟢 |
| Content Delivery Manager | kill suggested content / SilentInstalledApps / OEM preinstall | reg [U]+[S] | 🟢 |
| Error reporting | WER disabled, WerSvc/PcaSvc off | reg + service | 🟡 |
| WMI Autologgers | `Start=0` on Diagtrack-Listener, SQM, etc. | reg | 🟡 |
| Find My Device | off | reg | 🟢 |

Source: WinUtil `WPFTweaksTelemetry`, Win11Debloat `Disable_Telemetry.reg`, optimizerDuck `SecurityAndPrivacy.cs`.
A **conservative preset** ≈ Win11Debloat `-RunDefaults` (~17 tweaks).

### `disable-ai` — [S] 🟢 · ✅

Copilot (`TurnOffWindowsCopilot=1`), Recall, Click-to-Do, Edge AI, Paint/Notepad AI, `WSAIFabricSvc`→manual. reg (mostly HKLM policy).

### `disable-bing-search` — [U]+[S] 🟢 · ✅

Bing web results + Bing AI in Search, Cortana (`AllowCortana=0`), search highlights, store search suggestions, search history. reg.

---

## UI & shell (per-user — default profile)

### `explorer-ui` — [U] 🟢 · ✅

Show file extensions, show hidden files, open to This PC, hide Home/Gallery, drive-letters-first, hide duplicate
removable drives, add folders to This PC. reg [U] (`…\Explorer\Advanced` + CLSID namespace).

### `taskbar-startmenu` — [U] 🟢 · ✅

Taskbar align left, hide/replace Search box, hide Task View / Chat / Widgets, End Task on right-click, Start menu:
hide Recommended, All-apps view, layout combine. reg [U] + start-layout binary.

### `context-menu` — [U] 🟢 · ✅

Restore the Windows 10 classic right-click menu (removes "Show more options"). reg [U] CLSID `InprocServer32`.
Also hide Share / Give access to / Include in library.

### `desktop-icons` — [U] 🟢 · ✅

Show/hide This PC, Recycle Bin, User folder, Network, Control Panel; hide all icons; remove shortcut arrow. reg [U]
(`HideDesktopIcons\NewStartPanel` CLSIDs).

### `visual-effects` — [U]+[S] 🟢 · ✅

Dark mode, disable transparency/animations, `MenuShowDelay=0`, `StartupDelayInMSec=0`, "adjust for best performance"
visual-effects profile. reg.

---

## Performance, gaming, power

### `performance` — [S]+[U] 🟢/🟡 · ✅

| Tweak | Effect | Mechanism | Risk |
|---|---|---|---|
| Win32PrioritySeparation | foreground boost (=38/26) | reg [S] | 🟢 |
| Multimedia scheduler | NetworkThrottlingIndex=off, SystemResponsiveness=10, Games task priority | reg [S] | 🟢 |
| Keyboard latency | KeyboardDelay=0 / Speed=31 | reg [U] | 🟢 |
| Background apps off | GlobalUserDisabled=1 | reg [U] | 🟡 |
| SvcHostSplit | single svchost (= RAM in KB) | reg [S] | 🟡 |
| Storage Sense / Explorer auto-discovery | off | reg | 🟢 |

### `gaming` — [S]+[U] 🟢/🟡 · ✅

GameDVR / background recording off (`GameDVR_Enabled=0`, policy `AllowGameDVR=0`), Game Mode, mouse acceleration
off, Fullscreen Optimizations (🟡 driver-dependent), HAGS `HwSchMode=2` (🟡). reg.

### `power` — [F] 🟢/🟡 · ✅

Hibernate + Fast Startup off (`powercfg /h off`), Ultimate Performance plan (`powercfg -duplicatescheme e9a42b02…`),
PowerThrottling off (🟡 laptops), USB power saving off (🟡 — [F], enumerates devices). powercfg + reg.

### `gpu-tuning` — [F] 🟡/🔴 · ✅ (manual)

Vendor power-state tweaks under `{4d36e968-…}\NNNN` (index known only **after** driver install → first-logon).
AMD: ULPS off, power-gating off (🔴). NVIDIA: Dynamic/Async P-states (🟡). Intel: async flips, adaptive vsync. reg [F].

---

## System

### `services-tune` — [S] 🟢/🔴 · ✅

Two presets:

- **safe** 🟢 — a handful to Manual (Maps, Fax, RemoteRegistry, RetailDemo…).
- **hardening** 🔴 — optimizerDuck's 287-service matrix (13 disabled). Can break remote mgmt / connectivity if
  mis-set — service-by-service allow-list, opt-in only.
`Set-Service -StartupType`. Source: optimizerDuck `ConfigureServices`, WinUtil `WPFTweaksServices`.

### `scheduled-tasks` — [S] 🟢 · ✅

Disable telemetry/CEIP/compatibility-appraiser/error-reporting tasks (~20). `Disable-ScheduledTask`.

### `optional-features` — [S] 🟢 · ✅

Enable/disable via DISM: Hyper-V, WSL, Windows Sandbox, .NET 3.5, NFS, legacy media (WMP/DirectPlay), OpenSSH Server,
legacy F8 recovery (bcdedit). `Enable/Disable-WindowsOptionalFeature`.

### `network-dns` — [F] 🟢 · ✅

Set DNS (presets: Cloudflare, Google, Quad9, OpenDNS, AdGuard + family/malware variants, IPv4+IPv6), disable
IPv6/Teredo. `Set-DnsClientServerAddress` (needs active NIC → first-logon). Source: WinUtil `dns.json`.

### `windows-update` — [S] 🟢/🟡 · ✅

Delivery Optimization off, pause auto-reboot while active, defer feature updates, active hours. reg (HKLM policy).
🟡 — don't disable updates outright (keeps the "don't break the OS" line).

### `security` — [S] 🟡/🔴 · ✅

Default (🟡): BitLocker auto-encryption off. **Danger zone (commented out, opt-in 🔴)**: lower UAC, disable SmartScreen,
disable Defender — each with an inline warning. (Defender key is a no-op while Tamper Protection is on; InDows does
not bypass Tamper Protection.)

### `misc` — [S] 🟢 · ✅

RTC-as-UTC for dual-boot ([S] 🟢), verbose logon, long paths, NumLock at boot, restore point (**post-install**, not
during), classic photo viewer.

---

## Setup, identity & appearance

### `username` · `computer-name` · `locale-keyboard` · `timezone` — ✅

Identity the base leaves interactive; these automate it. XML snippets (see each module folder).

### `autologon` — oobeSystem 🟡 · ✅

Sign in without typing a password (pairs with `username`). XML snippet; cleartext password.

### `wifi` — [F] 🟡 · ✅

Join a Wi-Fi network at first logon (WPA2-Personal; cleartext password). `netsh wlan`.

### `dark-mode` — [U] 🟢 · ✅

Dark theme for new users — apps and shell each optional. reg [U].

### `wallpaper` — [F] 🟢 · ✅

Set the desktop + lock-screen image (you stage the image first). reg [HKCU] + Personalization CSP.

---

## Credits

Tweak values, lists and revert logic are distilled from these MIT / open-source projects — credit to their authors:

- **WinUtil** — Chris Titus Tech — <https://github.com/christitustech/winutil>
- **Win11Debloat** — Raphire — <https://github.com/Raphire/Win11Debloat>
- **optimizerDuck** — itsfatduck — <https://github.com/itsfatduck/optimizerDuck>
