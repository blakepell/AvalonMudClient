# ShadowPanel

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ShadowPanel/ShadowPanel.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ShadowPanelExample.xaml`

## Description

A `ContentControl` that automatically applies a configurable `DropShadowEffect` to its child content. Instead of setting the effect manually on each element, wrap the element in `ShadowPanel` and tune the shadow properties.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Elevation` | `double` | `8.0` | Shadow depth (offset distance). Higher = farther from the element. |
| `ShadowThickness` | `double` | `10.0` | Blur radius — higher = softer/more diffuse shadow. |
| `ShadowOpacity` | `double` | `0.3` | Opacity of the shadow (0.0–1.0). |
| `ShadowColor` | `Color` | `Colors.Black` | Color of the drop shadow. |
| `ShadowDirection` | `double` | `315.0` | Angle in degrees (0=right, 90=down, 180=left, 270=up). Default is bottom-right. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:ShadowPanel
    Elevation="6"
    ShadowThickness="12"
    ShadowOpacity="0.25"
    ShadowColor="Black">
    <Border CornerRadius="8" Background="White" Padding="16">
        <TextBlock Text="Card with shadow" />
    </Border>
</mosaic:ShadowPanel>
```

## Notes

- The `DropShadowEffect` is applied to the `ShadowPanel` itself (not the inner child), so the shadow follows the panel bounds.
- Any of the four properties can be changed at runtime — the effect updates immediately.
- `ShadowDirection=315` produces the most natural-looking drop shadow (light source at top-left).
