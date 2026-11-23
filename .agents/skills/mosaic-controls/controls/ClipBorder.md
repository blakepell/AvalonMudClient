# ClipBorder

**Base class:** `Border`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ClipBorder/ClipBorder.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ClipBorderExample.xaml`

## Description

A `Border` subclass that applies a rounded-rectangle clip geometry to its child element, so the child content is clipped to the border's `CornerRadius`. The standard `Border` only clips the border rendering; child content (images, media, etc.) bleeds out of the corners. `ClipBorder` solves this.

No custom dependency properties are added — all configuration is through the inherited `Border` properties (`CornerRadius`, `BorderThickness`, `BorderBrush`, etc.).

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Clip an image to rounded corners -->
<mosaic:ClipBorder CornerRadius="12" Width="200" Height="120">
    <Image Source="/Assets/photo.jpg" Stretch="UniformToFill" />
</mosaic:ClipBorder>

<!-- Clip a video/media element -->
<mosaic:ClipBorder CornerRadius="8" BorderBrush="Gray" BorderThickness="1">
    <MediaElement Source="video.mp4" />
</mosaic:ClipBorder>
```

## Notes

- `ClipBorder` works with any single child `UIElement`.
- The clip geometry is a `StreamGeometry` frozen for performance; it is recalculated on each `OnRender` call.
- The child's original `Clip` property is saved and restored when the child is removed.
- For simple rectangular clipping (no corner radius needed), use a plain `Border` instead.
