# InertiaScrollViewer

**Base class:** `ScrollViewer`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/InertiaScrollViewer/InertiaScrollViewer.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/InertiaScrollViewerExample.xaml`

## Description

Extends the standard `ScrollViewer` to add smooth animated (inertia/momentum) scrolling on mouse wheel. When `IsScrollAnimation` is `true`, each wheel event triggers a cubic-eased `DoubleAnimation` instead of an instant jump. The companion `InertiaScrollViewerBehavior` provides the `VerticalOffset` attached property used by the animation.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsScrollAnimation` | `bool` | `true` | Enables the animated inertia scrolling. |
| `WheelScrollDistance` | `double` | `320` | Distance in pixels added to the inertial destination per standard wheel detent. |
| `AnimationDurationMilliseconds` | `int` | `800` | Duration of each scroll animation in ms. |
| `DesiredFrameRate` | `int` | `40` | Frame rate cap for the animation. |

All standard `ScrollViewer` properties apply (`HorizontalScrollBarVisibility`, `VerticalScrollBarVisibility`, `CanContentScroll`, etc.).

## Methods

| Method | Description |
|---|---|
| `AnimateScroll(double toValue, Action? onCompleted = null)` | Programmatically animates to a vertical offset. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:InertiaScrollViewer
    AnimationDurationMilliseconds="600"
    WheelScrollDistance="360"
    VerticalScrollBarVisibility="Auto">
    <StackPanel>
        <!-- content items -->
    </StackPanel>
</mosaic:InertiaScrollViewer>
```

## Notes

- Animated mouse-wheel scrolling is enabled by default. Set `IsScrollAnimation="False"` to make the control behave exactly like a standard `ScrollViewer`.
- Repeated wheel events accumulate against the pending destination, producing visible momentum instead of restarting from each intermediate frame.
- Mouse wheel events are marked `Handled=true` when animation is active, which prevents parent scrollers from also scrolling.
- The easing function is `CubicEase` with `EasingMode.EaseOut` — adjust `AnimationDurationMilliseconds` to tune the feel.
