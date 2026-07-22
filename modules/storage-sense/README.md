# storage-sense

Turns **Storage Sense on** and configures it to auto-clean the **Recycle Bin** (files older than
60 days) and the **Downloads** folder (files not opened in 60 days).

- **Anchor:** `[InDows:module] default-user-scripts` — per-user, written to the default profile.
- **What:** the `StorageSense\Parameters\StoragePolicy` DWORDs (`01`,`08`,`256`,`32`,`512`) — enable + the two 60-day cadences.
- **Note:** it *deletes* old Recycle Bin / Downloads files after 60 days — don't stash things you keep in Downloads.
