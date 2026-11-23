# TwoPaneView

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/TwoPaneView/TwoPaneView.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/TwoPaneViewExample.xaml`

## Description

An adaptive two-column/two-row layout control that displays two panes side-by-side (wide layout) or stacked vertically (tall/narrow layout) depending on available space. Mirrors the WinUI `TwoPaneView` semantic for porting WinUI/MAUI designs to WPF.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Pane1` | `object` | `null` | Content of the primary (left/top) pane. |
| `Pane2` | `object` | `null` | Content of the secondary (right/bottom) pane. |
| `Pane1Length` | `GridLength` | `*` | Column/row width of pane 1 in the split layout. |
| `Pane2Length` | `GridLength` | `*` | Column/row width of pane 2 in the split layout. |
| `MinWideModeWidth` | `double` | `641` | Minimum control width (px) to activate wide (side-by-side) mode. |
| `MinTallModeHeight` | `double` | `0` | Minimum control height (px) to activate tall (stacked) mode. |
| `PanePriority` | `TwoPaneViewPriority` (enum) | `Pane1` | Which pane is shown when only one pane fits (`Pane1` or `Pane2`). |
| `ModeState` | `TwoPaneViewMode` (read-only) | — | Current layout mode: `SinglePane`, `Wide`, or `Tall`. |

## Events

| Event | Description |
|---|---|
| `ModeChanged` | Raised when `ModeState` transitions (e.g., wide → single). |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:TwoPaneView
    MinWideModeWidth="700"
    Pane1Length="300"
    Pane2Length="*">

    <mosaic:TwoPaneView.Pane1>
        <Border Background="{DynamicResource {x:Static themes:MosaicTheme.ControlBackgroundBrush}}">
            <ListBox ItemsSource="{Binding Items}"
                     SelectedItem="{Binding SelectedItem, Mode=TwoWay}" />
        </Border>
    </mosaic:TwoPaneView.Pane1>

    <mosaic:TwoPaneView.Pane2>
        <ContentPresenter Content="{Binding SelectedItem}" />
    </mosaic:TwoPaneView.Pane2>

</mosaic:TwoPaneView>
```

## Notes

- When the control is narrower than `MinWideModeWidth`, only the pane specified by `PanePriority` is shown.
- `Pane1Length` and `Pane2Length` accept `Auto`, pixel values (`300`), or star ratios (`2*`).
- Listen to `ModeChanged` to adapt other UI elements (e.g., show a back button in single-pane mode).
