# device-metadata

Stops Windows from **downloading device apps and metadata** (icons, friendly names, extra info)
from Microsoft for your hardware.

- **Anchor:** `[InDows:module] specialize-scripts` — machine-wide, before first boot.
- **What:** `PreventDeviceMetadataFromNetwork = 1` under both `…\CurrentVersion\Device Metadata` and its Policies mirror.
- **Note:** some devices may show a generic name/icon instead of a fetched one. Harmless.
