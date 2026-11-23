# SmallPanel

**Base class:** `Panel`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SmallPanel/SmallPanel.cs`

## Description

A lightweight panel that stacks all its children in the same bounding box (like a `Grid` with a single cell). The panel's desired size is the size of its *largest* child. All children are arranged to fill the same `Rect`. Useful for layered overlays, decorations, and templated control backgrounds without the overhead of a `Grid`.

No custom dependency properties — all configuration is through standard `FrameworkElement` properties on the children.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Overlay a loading spinner on top of content -->
<mosaic:SmallPanel>
    <Image Source="/Assets/background.png" />
    <Border Background="#80000000">
        <TextBlock
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            Foreground="White"
            Text="Loading…" />
    </Border>
</mosaic:SmallPanel>
```

## Notes

- All children receive the same `arrangeSize` rect — use `HorizontalAlignment` / `VerticalAlignment` on children to control their position within the shared bounds.
- `Visibility.Collapsed` children are measured but given zero space during arrange.
- For complex multi-cell layouts, use a `Grid` instead.
