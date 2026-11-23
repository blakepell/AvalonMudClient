# SplitPanel

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SplitPanel/SplitPanel.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SplitPanelExample.xaml`

## Description

A two-pane container separated by a draggable `GridSplitter`. `SplitterPosition` is a bindable ratio from `0.0` to `1.0` and updates live while dragging.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Pane1` | `object` | `null` | First pane content, top or left. |
| `Pane2` | `object` | `null` | Second pane content, bottom or right. |
| `SplitterPosition` | `double` | `0.5` | Two-way fraction of space allocated to `Pane1`. |
| `Orientation` | `Orientation` | `Vertical` | Vertical stacks top/bottom; Horizontal places left/right. |
| `SplitterThickness` | `double` | `6` | Splitter thickness in DIPs. |
| `MinPaneSize` | `double` | `0` | Minimum size each pane can shrink to. |

## Events

| Event | Type | Description |
|---|---|---|
| `SplitterPositionChanged` | `RoutedPropertyChangedEventHandler<double>` | Raised for binding/code changes and user drags. |

```xml
<mosaic:SplitPanel Orientation="Horizontal" SplitterPosition="{Binding Split, Mode=TwoWay}">
    <mosaic:SplitPanel.Pane1>
        <TextBlock Text="Left" />
    </mosaic:SplitPanel.Pane1>
    <mosaic:SplitPanel.Pane2>
        <TextBlock Text="Right" />
    </mosaic:SplitPanel.Pane2>
</mosaic:SplitPanel>
```
