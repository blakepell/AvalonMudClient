# ContentPanel

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ContentPanel/ContentPanel.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ContentPanelExample.xaml`

## Description

A themed content container with header, content, footer, separators, corner radius, and configurable header/footer colors. Use it for framed content sections where `InfoCard` is too opinionated.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `Info` | Header title text. |
| `HeaderContent` | `object` | `null` | Custom content hosted in the header. |
| `FooterContent` | `object` | `null` | Custom content hosted in the footer. |
| `SeparatorVisibility` | `Visibility` | `Visible` | Shows/hides separators. |
| `HeaderVisibility` | `Visibility` | `Visible` | Shows/hides the header area. |
| `CornerRadius` | `CornerRadius` | `5` | Panel corner radius. |
| `HeaderBackground` / `HeaderForeground` | `Brush` | theme | Header colors. |
| `FooterBackground` / `FooterForeground` | `Brush` | theme | Footer colors. |
| `ShowChevron` | `bool` | `false` | Shows a chevron on the left of the header that expands/collapses the panel. |
| `IsOpen` | `bool` | `true` | Whether the content and footer are expanded. Binds two-way by default. |
| `AnimationDuration` | `Duration` | `0:0:0.18` | Length of the expand/collapse animation. |

## Events

| Event | Description |
|---|---|
| `Opened` | Bubbling routed event raised when the panel expands. |
| `Closed` | Bubbling routed event raised when the panel collapses. |

```xml
<mosaic:ContentPanel Title="Details">
    <TextBlock Text="Panel body" />
    <mosaic:ContentPanel.FooterContent>
        <TextBlock Text="Footer" />
    </mosaic:ContentPanel.FooterContent>
</mosaic:ContentPanel>
```

Collapsible, starting closed and bound to a view model:

```xml
<mosaic:ContentPanel
    Title="Details"
    ShowChevron="True"
    IsOpen="{Binding DetailsExpanded}">
    <TextBlock Text="Panel body" />
</mosaic:ContentPanel>
```
