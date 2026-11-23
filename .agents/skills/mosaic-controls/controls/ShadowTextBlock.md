# ShadowTextBlock

**Base class:** `TextBlock`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ShadowTextBlock/ShadowTextBlock.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ShadowTextBlockExample.xaml`

## Description

A `TextBlock` that automatically creates and attaches a `DropShadowEffect` to its text. Shadow parameters (`BlurRadius`, `ShadowDepth`, `ShadowColor`) are exposed as dependency properties.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `BlurRadius` | `double` | `20.0` | Blur radius of the shadow — higher = softer shadow. |
| `ShadowDepth` | `double` | `5.0` | Distance the shadow is offset from the text. |
| `ShadowColor` | `Color` | `Colors.Black` | Color of the shadow. |

All standard `TextBlock` properties (`Text`, `FontSize`, `Foreground`, `TextWrapping`, etc.) apply normally.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Subtle shadow on a heading -->
<mosaic:ShadowTextBlock
    Text="Welcome"
    FontSize="36"
    BlurRadius="8"
    ShadowDepth="2"
    ShadowColor="DarkSlateBlue" />

<!-- Strong glow effect -->
<mosaic:ShadowTextBlock
    Text="Highlight"
    FontSize="24"
    Foreground="White"
    BlurRadius="20"
    ShadowDepth="0"
    ShadowColor="CornflowerBlue" />
```

## Notes

- Setting `ShadowDepth=0` with a colored `ShadowColor` produces a glow effect centered on the text.
- `BlurRadius` and `ShadowDepth` are bindable `DependencyProperty` values.
- The shadow effect is initialized in the constructor; there is no way to disable it — use a plain `TextBlock` if no shadow is needed.
