# How the base `autounattend.xml` is produced

The base is generated with the **unattend-generator** by Christoph Schneegans
(<https://schneegans.de/windows/unattend-generator/>) using the settings below, then InDows grafts a few
things on top (bootstrap + essentials catalog + module anchors). Keeping a recipe makes the base
**reproducible** — anyone can regenerate it.

## Step-by-step (verified against the actual UI, in page order)

Go down the page. Only the sections below need a change — everything else: leave the default.

1. **Region and language settings** — by default *"Install Windows using these language settings"* is
   selected; switch to the radio at the **bottom** of the section: **"Select language settings
   interactively during Windows Setup"**.
2. **Windows PE stage** — select **"Run the Windows PE stage interactively"** (the **first** radio).
   **This IS the "interactive disk"** — Setup asks you where to install. As a result the next three
   sub-blocks (*Windows image to install*, *Partitioning and formatting*, *Disable Windows Defender*)
   belong to the scripted path and are **ignored** — skip them.
3. **Windows edition** — **"Use a generic product key…"** → **Pro** (already the default).
4. **Processor architectures** — keep **"Intel / AMD 64-bit"** selected.
5. **Setup settings** — ✅ check **"Bypass Windows 11 requirements check (TPM, Secure Boot, etc.)"**.
   ⬜ leave **"Allow Windows 11 to be installed without internet connection"** **UNCHECKED** — the form
   itself notes you don't need it for local accounts.
6. **Computer name** — **"Let Windows generate a random computer name"** (default).
7. **Time zone** — **"Let Windows determine your time zone…"** (default).
8. **User accounts** — ⚠️ the form **pre-fills two accounts** (`Admin`, `User`). **Clear both Account-name
   fields** (empty them), then select the radio **"Add a local ('offline') user account interactively
   during Windows Setup"** (near the bottom of the section).
9. **Password expiration** — **"Passwords do not expire"** (default). *Account Lockout* — default.
10. **File Explorer tweaks** — optional: *"Always show file extensions"*, *"Open File Explorer to This PC"*,
    *"Use the classic context (right-click) menu"* (or leave to modules).
11. **Start menu and taskbar** — set **Start menu → remove all pinned tiles (empty)** and **Taskbar → remove
    all pinned apps (empty)** so a fresh install ships with **no "promoted app" ads** in Start/taskbar. (This
    is the robust, build-correct way — the generator emits the matching `SetStartPins` / `TaskbarLayout` /
    unlock machinery.) *URL params: `StartPinsMode=Empty`, `StartTilesMode=Empty`, `TaskbarIconsMode=Empty`.*
12. **System tweaks** — ⚠️ leave **UNCHECKED** the security ones: *Disable Windows Update, Disable UAC,
    Disable SmartScreen, Disable Smart App Control*. (optional safe: *Hide Edge First Run Experience*)
13. **Core isolation** — keep enabled. *Visual effects / Desktop icons / Colors / Wallpaper / Lock screen* —
    default (InDows modules handle those).
14. **WLAN / Wi-Fi setup** — **"Configure Wi-Fi interactively"**.
15. **Express settings** — **"Disable all"**; also tick **"Do not show Bing results when searching"**.
16. **Remove bloatware** — tick the bloat you want gone. ⚠️ do **NOT** remove Microsoft Store, Windows
    Terminal, Notepad, Photos, Snipping Tool, Calculator, PowerShell, OpenSSH Client. **Leave Edge** (→
    `remove-edge` module).
17. **Run custom scripts** — leave **ALL** empty (InDows grafts its own). *AppLocker / XML markup* — default.

Then click **Download** and hand the generated `autounattend.xml` back to InDows for grafting.

## Appendix — recommended "Remove bloatware" selection

The generator lists ~60 removable apps. Recommended default for a **generic** base (verified against the
live UI):

**🚫 Never remove — these break a Windows function (not just an app):**
`Facial recognition (Windows Hello)` (face/PIN login) · `Media Features` (Windows Media codecs → breaks
audio/video playback in several apps) · `Handwriting (all languages)` (pen/touch input) ·
`Speech (all languages)` (dictation, TTS, Narrator/accessibility).

**🚫 Keep — apps users expect:**
`Microsoft Store` (⚠️ hard to reinstall) · `Calculator` · `Clock` · `Notepad (modern)` · `Paint` ·
`Photos` · `Snipping Tool` · `Windows Terminal` · `OpenSSH Client` · `Windows Media Player (modern)` ·
`Voice Recorder`.

**✅ Recommended to remove (uncontroversial bloat):**
`3D Viewer` · `Bing Search` · `Clipchamp` · `Copilot` · `Cortana` · `Dev Home` · `Family` ·
`Feedback Hub` · `Get Help` · `Mail and Calendar` · `Maps` · `Math Input Panel` · `Mixed Reality` ·
`Movies & TV` · `News` · `Office 365` (the "Get Office" promo stub, not real Office) ·
`Outlook for Windows` (new web wrapper) · `Paint 3D` · `People` · `Power Automate` · `PowerShell 2.0` ·
`Recall` · `Skype` · `Solitaire Collection` · `Steps Recorder` · `Teams` (consumer chat) · `Tips` ·
`Wallet` · `Weather` · `WordPad`.

**🟡 Up to you (depends on usage):** `OneDrive` · `OneNote` · `Sticky Notes` · `To Do` · `Xbox Apps` +
`Game Assist` · `Phone Link` · `Camera` · `Internet Explorer` (⚠️ legacy intranet apps may use its MSHTML
engine) · `PowerShell ISE` · `Remote Desktop Client` (`mstsc.exe` stays regardless) · `Quick Assist` ·
`Windows Fax and Scan` · `OneSync` · `Windows Media Player (classic)`.

> **Edge is NOT in this list** — it is removed by the `remove-edge` module, not here.

## What InDows grafts after generation

Onto the generated file, InDows adds (all inside the schneegans `<Extensions>` / OOBE structure):

1. **`base-tweaks.ps1`** as a `<File>` node + an `Order 5` call **inside the default-user hive window** —
   turns off the Start-menu "promoted apps" (Content Delivery Manager) so every new user gets an ad-free Start.
2. **`bootstrap.ps1`** as a `<File>` node — the two-stage first-logon app installer.
3. **`configuration.dsc.yaml`** as a `<File>` node — the essentials catalog (Brave + 7-Zip).
4. A **`FirstLogonCommands`** entry (`Order 2`) that runs `bootstrap.ps1`.
5. The base's default-user hive **unload is renumbered to `Order 50`**, leaving `Order 6–49` for per-user
   modules to run *while the hive is still loaded*.
6. **Six module anchors** (HTML comments) so modules know where to plug in:
   - `<!-- [InDows:module] specialize-shell-setup -->` — `specialize` Shell-Setup elements (`computer-name`, `timezone`)
   - `<!-- [InDows:module] default-user-scripts -->` — per-user scripts, **inside** the hive window (`HKEY_USERS\DefaultUser`)
   - `<!-- [InDows:module] specialize-scripts -->` — machine-wide scripts after the unload (HKLM / services / DISM / AppX)
   - `<!-- [InDows:module] oobe-international -->` — `oobeSystem` international component (`locale-keyboard`)
   - `<!-- [InDows:module] account -->` — where the `username` module's `<UserAccounts>` goes
   - `<!-- [InDows:module] first-logon-scripts -->` — scripts needing a live session (DNS / GPU / USB)

This grafting is done by a script and validated (XML well-formed, embedded scripts parse, ascending
`Order`s, default-user window correct).
