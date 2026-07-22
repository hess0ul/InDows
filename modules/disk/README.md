# Module: `disk` — automated disk layout

Replace the **interactive** disk step with a fully automatic disk layout.

> In the **InDows GUI** this module opens a **partition-table editor** (Advanced category, behind the risk
> disclaimer): pick the target disk, whether to wipe it, and add/size/format each partition, then mark which one
> Windows installs to. The `snippet.xml` below is the **standalone default** (wipe + standard UEFI layout on
> disk 0) used if you integrate the module by hand; the GUI generates the equivalent XML from your layout.

- **Snippet:** [`snippet.xml`](snippet.xml)
- **Where:** `windowsPE` pass → `Microsoft-Windows-Setup` component
- **Anchor:** `[InDows:module] disk` (the base's windowsPE Setup component)
- **Risk:** 🔴 **Destructive** — `WillWipeDisk = true` erases disk 0 with no prompt.

## ⚠️ Before you use it

- This **erases disk 0**. Be certain disk 0 is the drive you want Windows on — if you have several disks,
  **unplug the others** during install, or you may wipe the wrong one.
- Only use this for a clean single-Windows machine. For dual-boot or multi-disk setups, **don't** add this
  module — keep the base's interactive disk picker.

## Integrate

Paste **both** elements from `snippet.xml` (the `<DiskConfiguration>` and the `<ImageInstall>`) into the
`Microsoft-Windows-Setup` component of the `windowsPE` pass. The layout created:

| # | Partition | Size | Format |
| --- | --- | --- | --- |
| 1 | EFI System | 300 MB | FAT32 |
| 2 | MSR | 16 MB | — |
| 3 | Windows (C:) | rest of disk | NTFS |
