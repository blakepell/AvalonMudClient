# Badge

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/Badge/Badge.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/BadgeExample.xaml`

## Description

A small inline status or count badge. Defaults to a blue pill-shaped label 20 px tall. The badge content is set via `Text` or through `Content`.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | Convenience property — sets the badge label text directly. |

Standard `ContentControl` properties apply (`Background`, `Foreground`, `Content`, `FontSize`, etc.).

Default overrides: `Background = CornflowerBlue`, `Foreground = White`, `Height = 20`.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Simple text badge -->
<mosaic:Badge Text="New" />

<!-- Custom color badge -->
<mosaic:Badge
    Background="OrangeRed"
    Foreground="White"
    Text="3" />

<!-- Bind count from view model -->
<mosaic:Badge Text="{Binding UnreadCount}" />
```

## Notes

- The badge auto-sizes to its content horizontally and has a fixed `Height=20` by default.
- Override `Background` and `Foreground` inline or via a style to match status semantics (error = red, success = green, etc.).
