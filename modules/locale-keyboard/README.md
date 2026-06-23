# Module: `locale-keyboard`

Set the **display language**, **regional formats** and **keyboard layout** automatically, so Windows
doesn't ask for them at first run.

> ⚠️ `UILanguage` (the Windows display language) only works if that **language pack is present in the
> ISO**. The standard single-language ISOs ship one language; for others, either use a multi-language ISO
> or leave `UILanguage` as `en-US`. `SystemLocale` / `UserLocale` / `InputLocale` work regardless.

## How to integrate

1. Open the base **`autounattend.xml`**.
2. Find the anchor in the **`oobeSystem`** pass:

   ```xml
   <!-- [InDows:module] oobe-international -->
   ```

3. Paste the whole [`snippet.xml`](snippet.xml) component right after it and edit the four values.

## The four values

| Element | Meaning | Example |
| --- | --- | --- |
| `InputLocale` | keyboard layout(s) — `langID:layoutID` | `040c:0000040c` |
| `SystemLocale` | language for non-Unicode apps | `fr-FR` |
| `UILanguage` | Windows display language (needs the pack) | `fr-FR` |
| `UserLocale` | number / date / currency formats | `fr-FR` |

### Language codes (`SystemLocale` / `UILanguage` / `UserLocale`)

`en-US` · `en-GB` · `fr-FR` · `de-DE` · `es-ES` · `it-IT` · `nl-NL` · `pt-BR` · `pl-PL` · `ja-JP` …

### Keyboard layouts (`InputLocale`, format `langID:layoutID`)

| Layout | Code |
| --- | --- |
| US (QWERTY) | `0409:00000409` |
| US-International | `0409:00020409` |
| French (AZERTY) | `040c:0000040c` |
| UK | `0809:00000809` |
| German | `0407:00000407` |
| Spanish | `040a:0000040a` |

You can set **several** layouts by separating them with `;`, e.g. a French region with a US-International
keyboard: `InputLocale = 040c:00020409`.

## Example — French region, US-International keyboard

```xml
<InputLocale>040c:00020409</InputLocale>
<SystemLocale>fr-FR</SystemLocale>
<UILanguage>fr-FR</UILanguage>
<UserLocale>fr-FR</UserLocale>
```

(Full code lists: search "Windows Default Input Locales" and "Available Language Packs for Windows".)
