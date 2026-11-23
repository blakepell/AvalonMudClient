# AsteriskTextBlock

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/AsterikTextBlock/AsteriskTextBlock.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AsteriskTextBlockExample.xaml`

## Description

A `Control` that displays a masked version of its `Text` property. Every character in `Text` is replaced with the `MaskCharacter` (default: `●` U+25CF). Useful for displaying secrets or sensitive values without a full `PasswordBox`.

The displayed `MaskedText` property is read-only and updates automatically whenever `Text` or `MaskCharacter` changes.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | The real text to mask. Bind your secret value here (two-way). |
| `MaskedText` | `string` (read-only) | `""` | The displayed string of mask characters. Do not set directly. |
| `MaskCharacter` | `char` | `'●'` (U+25CF) | The character used to mask each character in `Text`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Static masked text -->
<mosaic:AsteriskTextBlock Text="MySecretValue" />

<!-- Bound to a view-model property -->
<mosaic:AsteriskTextBlock Text="{Binding ApiKey}" MaskCharacter="*" />
```

## Notes

- `Text` is the `[ContentProperty]`, so it can be set as element content.
- Unlike `PasswordBox`, there is no built-in input; this is display-only.
- Changing `MaskCharacter` at runtime immediately re-renders the masked text.
