# Module: `privacy-telemetry`

Extra privacy hardening **on top of the base**: activity history, advertising ID, suggested content, and
Find My Device.

- **Script:** [`PrivacyTelemetry.ps1`](PrivacyTelemetry.ps1)
- **Anchor:** `[InDows:module] default-user-scripts`
- **Risk:** 🟢 Safe
- **Integration:** see [`../HOWTO-script-modules.md`](../HOWTO-script-modules.md)

The base already turns off the main telemetry/advertising switches (chosen in the generator). This module
adds the bits people often want on top.

| Tweak | Key |
| --- | --- |
| Activity history off | `…\Policies\…\System\PublishUserActivities/EnableActivityFeed = 0` |
| Advertising ID off | `…\AdvertisingInfo\Enabled = 0` |
| Tailored experiences off | `…\Privacy\TailoredExperiencesWithDiagnosticDataEnabled = 0` |
| Suggested content / tips off | `…\ContentDeliveryManager\…` (`SystemPaneSuggestions` + ~8 `SubscribedContent` keys) |
| No silent / OEM app installs | `…\ContentDeliveryManager\SilentInstalledApps / PreInstalledApps / OemPreInstalledApps = 0` |
| Find My Device off | `…\Policies\Microsoft\FindMyDevice\AllowFindMyDevice = 0` |

Each line is independent — comment out anything you want to keep.
