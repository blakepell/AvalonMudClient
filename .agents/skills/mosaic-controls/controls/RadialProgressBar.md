# RadialProgressBar

**Base class:** `ProgressBar`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/RadialProgressBar/RadialProgressBar.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/RadialProgressBarExample.xaml`

## Description

A circular progress bar that renders progress as a filled arc, pie, or a ring of discrete shapes. It supports normal `ProgressBar` values plus radial geometry and indeterminate animation settings.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `InnerBackgroundBrush` | `Brush` | `Transparent` | Fill inside the arc. |
| `OuterBackgroundBrush` | `Brush` | `Transparent` | Unfilled track brush. |
| `ArcWidth` | `double` | `10` | Progress arc thickness. |
| `ArcBackgroundWidth` | `double` | `0` | Track thickness; `0` auto-sizes. |
| `ArcRotationDegree` | `double` | `270` | Start angle. |
| `ArcMode` | `ArcMode` | `Fill` | `Fill`, `Pie`, or `Shape`. |
| `ArcDirection` | `SweepDirection` | `Clockwise` | Sweep direction. |
| `ShapeModeWidth` | `double` | `1` | Shape width in `Shape` mode. |
| `ShapeModeStep` | `int` | `3` | Angular spacing in `Shape` mode. |
| `ShapeModeShape` | `ArcShape` | `Rectangle` | Shape used in `Shape` mode. |
| `ShapeModeUseFade` | `bool` | `true` | Fades trailing shapes. |
| `ShapeRotationAdjustment` | `double` | `0` | Extra per-shape rotation. |
| `IndeterminateSpeedRatio` | `double` | `1` | Indeterminate animation speed multiplier. |

```xml
<mosaic:RadialProgressBar
    Width="96"
    Height="96"
    Minimum="0"
    Maximum="100"
    Value="{Binding PercentComplete}"
    ArcWidth="8" />
```
