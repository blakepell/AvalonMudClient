# RelativePanel

**Base class:** `Panel`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/RelativePanel/RelativePanel.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/RelativePanelExample.xaml`

## Description

A layout panel that positions children relative to the panel itself or relative to sibling elements, using attached properties. Inspired by the UWP `RelativePanel`. Relationships are resolved using a topological dependency graph (`RelativePanelGraph`).

## Attached Properties

Use these on child elements to define their position:

| Attached Property | Type | Description |
|---|---|---|
| `RelativePanel.AlignLeftWithPanel` | `bool` | Aligns the element's left edge with the panel's left edge. |
| `RelativePanel.AlignTopWithPanel` | `bool` | Aligns the element's top edge with the panel's top edge. |
| `RelativePanel.AlignRightWithPanel` | `bool` | Aligns the element's right edge with the panel's right edge. |
| `RelativePanel.AlignBottomWithPanel` | `bool` | Aligns the element's bottom edge with the panel's bottom edge. |
| `RelativePanel.AlignLeftWith` | `UIElement` | Aligns the element's left edge with the referenced element's left edge. |
| `RelativePanel.AlignTopWith` | `UIElement` | Aligns the element's top edge with the referenced element's top edge. |
| `RelativePanel.AlignRightWith` | `UIElement` | Aligns the element's right edge with the referenced element's right edge. |
| `RelativePanel.AlignBottomWith` | `UIElement` | Aligns the element's bottom edge with the referenced element's bottom edge. |
| `RelativePanel.RightOf` | `UIElement` | Places the element to the right of the referenced element. |
| `RelativePanel.LeftOf` | `UIElement` | Places the element to the left of the referenced element. |
| `RelativePanel.Below` | `UIElement` | Places the element below the referenced element. |
| `RelativePanel.Above` | `UIElement` | Places the element above the referenced element. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:RelativePanel Width="300" Height="200">
    <TextBlock x:Name="TitleBlock"
        mosaic:RelativePanel.AlignTopWithPanel="True"
        mosaic:RelativePanel.AlignLeftWithPanel="True"
        Text="Title" />

    <Button x:Name="SaveBtn"
        mosaic:RelativePanel.AlignRightWithPanel="True"
        mosaic:RelativePanel.AlignBottomWithPanel="True"
        Content="Save" />

    <Button
        mosaic:RelativePanel.LeftOf="{x:Reference SaveBtn}"
        mosaic:RelativePanel.AlignBottomWithPanel="True"
        Content="Cancel" />
</mosaic:RelativePanel>
```

## Notes

- Reference sibling elements by `x:Name` using `{x:Reference ElementName}`.
- Circular dependencies are not detected at design time; ensure relationships form a DAG.
- `RelativePanelNode` and `RelativePanelGraph` are internal helpers; do not reference them directly.
