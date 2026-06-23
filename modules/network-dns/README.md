# Module: `network-dns`

Set your DNS servers automatically (privacy / speed / filtering), on every active network adapter.

- **Script:** [`SetDns.ps1`](SetDns.ps1)
- **Anchor:** `[InDows:module] first-logon-scripts` — runs at first logon, **because it needs a live
  network adapter** (DNS can't be set during the offline specialize pass).
- **Risk:** 🟢 Safe (reversible; `Set-DnsClientServerAddress -ResetServerAddresses` reverts to DHCP)
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## Choose a provider

Edit `$dns` at the top of the script (uncomment one line):

| Provider | Servers |
| --- | --- |
| Cloudflare | `1.1.1.1`, `1.0.0.1` |
| Cloudflare (malware filter) | `1.1.1.2`, `1.0.0.2` |
| Quad9 | `9.9.9.9`, `149.112.112.112` |
| AdGuard | `94.140.14.14`, `94.140.15.15` |
| Google | `8.8.8.8`, `8.8.4.4` |
| OpenDNS | `208.67.222.222`, `208.67.220.220` |

## Notes

- This sets **IPv4** DNS. To also pin IPv6 DNS, add the provider's IPv6 addresses to `$dns` (Set-Dns…
  accepts a mixed list).
- Static DNS persists across reconnects on that adapter; new adapters (e.g. a USB dongle plugged later)
  keep using DHCP unless you re-run it.
