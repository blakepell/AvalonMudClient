# SimpleStackPanel

**Base class:** `Panel`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SimpleStackPanel/SimpleStackPanel.cs`

## Description

A performant alternative to the standard `StackPanel` that adds a uniform `Spacing` property to insert gaps between children without requiring margins on each child. Arranges children in a single horizontal or vertical line.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Orientation` | `Orientation` | `Vertical` | Direction children are stacked (`Horizontal` or `Vertical`). |
| `Spacing` | `double` | `0.0` | Uniform gap (in pixels) between adjacent visible children. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Vertical stack with 8px gaps -->
<mosaic:SimpleStackPanel Spacing="8">
    <TextBlock Text="Item 1" />
    <TextBlock Text="Item 2" />
    <TextBlock Text="Item 3" />
</mosaic:SimpleStackPanel>

<!-- Horizontal row with 12px gaps -->
<mosaic:SimpleStackPanel Orientation="Horizontal" Spacing="12">
    <Button Content="Back" />
    <Button Content="Next" />
    <Button Content="Finish" />
</mosaic:SimpleStackPanel>
```

## Notes

- `Spacing` is only applied between *visible* children — collapsed children are skipped.
- More efficient than a standard `StackPanel` with individual `Margin` settings.
- No `ItemsControl` equivalent; add children declaratively in XAML or via code.
