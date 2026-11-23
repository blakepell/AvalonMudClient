# Avatar

**Base class:** `Button`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/Avatar/Avatar.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AvatarExample.xaml`

## Description

Extends `Button` to display a user avatar. The control uses an `AvatarTemplateSelector` to choose between an image-based template and an initials-based fallback template when no image source is available.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `CornerRadius` | `CornerRadius` | `0` | Rounds the avatar corners (use `"50"` or a large value for a circular avatar). |
| `AvatarTemplateSelector` | `AvatarTemplateSelector` | built-in | Template selector that picks image vs. initials template. |

Standard `Button` properties (`Width`, `Height`, `Content`, `Command`, etc.) apply normally.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Image avatar (circular) -->
<mosaic:Avatar
    Width="48" Height="48"
    CornerRadius="24"
    Content="{Binding ProfileImageSource}" />

<!-- Initials fallback (no image) -->
<mosaic:Avatar
    Width="48" Height="48"
    CornerRadius="24"
    Content="JD"
    Background="CornflowerBlue"
    Foreground="White" />
```

## Notes

- The `AvatarTemplateSelector` checks whether `Content` is an `ImageSource` or a string; it renders an `Image` for the former and a `TextBlock` for the latter.
- Because `Avatar` extends `Button`, it fires the standard `Click` event and supports `Command`/`CommandParameter`.
- Use `CornerRadius="24"` on a 48×48 avatar for a perfect circle.
