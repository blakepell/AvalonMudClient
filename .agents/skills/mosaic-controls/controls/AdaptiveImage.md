# AdaptiveImage

**Base class:** `Image`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/AdaptiveImage/AdaptiveImage.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AdaptiveImageExample.xaml`

## Description

Extends `Image` to automatically adapt the image's pixel colors for the current Mosaic theme. When the theme changes, it applies an HSL (Hue-Saturation-Lightness) transform to each pixel of the source `BitmapSource` so that the image remains visually appropriate in both light and dark modes.

Requires `ThemeManager` to be registered in `AppServices` (the Mosaic DI container).

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MosaicThemeMode` | `MosaicThemeMode` | `Auto` | Override the theme mode; `Auto` follows `ThemeManager`. |
| `EnableLightnessMirror` | `bool` | `true` | When `true`, mirrors pixel lightness (dark pixels become light and vice versa) on theme switch. |
| `SaturationFloor` | `double` | `0.0` | Minimum saturation to keep after transform. |
| `LightnessBias` | `double` | `0.0` | Amount to shift lightness (−1.0 … 1.0). |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:AdaptiveImage
    Width="64" Height="64"
    EnableLightnessMirror="True"
    Source="/Assets/my-icon.png" />
```

## Notes

- Only works on `BitmapSource`-based images (PNG, BMP, etc.). Vector/SVG sources are unaffected.
- The pixel transform runs on `Task.Run` to avoid blocking the UI thread on large images.
- Subscribes to `ThemeManager.ThemeChanged` automatically on load; unsubscribes on unload.
