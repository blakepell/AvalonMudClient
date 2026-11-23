# StopwatchDisplay

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/StopwatchDisplay/StopwatchDisplay.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/StopwatchDisplayExample.xaml`

## Description

A real-time stopwatch display control. Shows elapsed time in `HH:mm:ss` format (optionally with milliseconds). The internal timer fires at a configurable interval and updates the `ElapsedTime` read-only property.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ShowMilliseconds` | `bool` | `false` | When `true`, appends `.fff` milliseconds to the display. |
| `ElapsedTime` | `TimeSpan` (read-only) | `00:00:00` | The current elapsed duration. |
| `IsRunning` | `bool` (read-only) | `false` | `true` while the stopwatch is counting. |

## Methods

| Method | Description |
|---|---|
| `Start()` | Starts the stopwatch. No-op if already running. |
| `Stop()` | Pauses the stopwatch without resetting. |
| `Reset()` | Resets `ElapsedTime` to zero and stops the timer. |
| `SetInterval(int milliseconds)` | Changes the update interval (default 100 ms for smooth display). |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:StopwatchDisplay
    x:Name="Stopwatch"
    FontSize="36"
    ShowMilliseconds="True" />
```

## Code-Behind

```csharp
private void StartButton_Click(object sender, RoutedEventArgs e) => Stopwatch.Start();
private void StopButton_Click(object sender, RoutedEventArgs e) => Stopwatch.Stop();
private void ResetButton_Click(object sender, RoutedEventArgs e) => Stopwatch.Reset();
```

## Notes

- `ElapsedTime` and `IsRunning` are dependency properties and can be bound (read-only).
- `SetInterval(16)` gives ~60 fps updates for smooth millisecond display at the cost of higher CPU usage.
- The control uses `DispatcherTimer` internally, so `Start()`/`Stop()`/`Reset()` must be called from the UI thread.
