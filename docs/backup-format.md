# ReDows backup format (the InDows restore contract)

InDows restores from a **ReDows backup folder**. The two tools are independent: they talk only through this
on-disk format, never through shared code. This is the contract InDows reads against; if ReDows ever changes
it, the round-trip test (`RestoreRoundTripTests`) is what catches the drift.

## Layout

A backup folder is self-describing:

```
<backup>/
  C/Users/tom/notes.txt          # the file tree, one folder per drive (the ':' is dropped)
  D/Projects/...                 # so nothing collides across volumes
  redows-restore-map.json        # de-duplication map (optional)
  redows-hashes.json             # per-file SHA-256 (optional)
  secrets-vault.zip              # encrypted secrets (optional)
```

**Drive-as-folder mapping.** An original path `C:\Users\tom\notes.txt` is stored as `C/Users/tom/notes.txt`
(each segment loses its `:`; only a drive letter ever has one). Restoring the inverse is exact:
`C/Users/tom/notes.txt` becomes `C:\Users\tom\notes.txt`.

## `redows-hashes.json`

Per-file checksums, so a restore proves each file is byte-identical to its original.

```json
{ "version": 1, "algorithm": "SHA-256", "files": [ { "path": "C/data/hello.txt", "sha256": "<uppercase hex>" } ] }
```

`path` is backup-relative (forward slashes); `sha256` is uppercase hex, compared case-insensitively. Missing
or broken means "no verification", not an error.

## `redows-restore-map.json`

De-duplication: a file stored once but that belonged in several places. `storedAt` is a backup-relative path;
`belongsAt` is the list of original paths it must be replicated to.

```json
{ "version": 1, "duplicates": [ { "storedAt": "C/Backup/photo.jpg", "belongsAt": [ "C:/Pictures/photo.jpg", "D:/Copy/photo.jpg" ] } ] }
```

A file not listed here is a normal file: it restores to its single original location.

## `secrets-vault.zip`

The secrets (keys, licences), kept out of the clear tree. A **standard password-protected ZIP with WinZip
AES-256 entries**, openable by 7-Zip / WinRAR, or by InDows with the same library ReDows used (SharpZipLib).
Each entry name is a backup-relative path. Only restored when a password is given; otherwise the encrypted
file is left on disk for the user to open.

## Restore rules (invariants)

- **Non-destructive**: an existing target file is skipped (kept), never overwritten; nothing is ever deleted.
- **Verified**: each restored file is re-hashed and compared to `redows-hashes.json`; a mismatch is reported,
  never a silent success.
- **Two modes**: back to original locations, or rebuilt under a chosen folder (keeping the drive-as-folder layout).
- **UNC aware**: a `\\server\share` backup folder is read and written natively.
