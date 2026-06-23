# Module: `disable-bing-search`

Remove **Bing web results**, **Cortana** and **Search Highlights** from the Start / Search box — local
search only.

- **Script:** [`DisableBingSearch.ps1`](DisableBingSearch.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

## What it sets

| Tweak | Key |
| --- | --- |
| Cortana off | `HKLM\…\Policies\…\Windows Search\AllowCortana = 0` |
| Web search off | `…\Windows Search\DisableWebSearch = 1`, `ConnectedSearchUseWeb = 0` |
| Search Highlights off | `…\Windows Search\EnableDynamicContentInWSB = 0` |
| Bing in Search off (user) | `…\CurrentVersion\Search\BingSearchEnabled = 0` |
