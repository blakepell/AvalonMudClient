# ExecutionControl

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ExecutionControl/ExecutionControl.cs`

A transport style control that exposes Play, Pause and Stop buttons suitable for a tool bar. Each button binds to its own command and is automatically disabled (and visually muted) when that command reports it cannot execute.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ExecutionControlExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `ICommand? PlayCommand` | Gets or sets the command invoked when the play button is clicked. The play button is disabled whenever this command reports that it cannot execute. |
| `object? PlayCommandParameter` | Gets or sets the parameter passed to `PlayCommand`. |
| `ICommand? PauseCommand` | Gets or sets the command invoked when the pause button is clicked. The pause button is disabled whenever this command reports that it cannot execute. |
| `object? PauseCommandParameter` | Gets or sets the parameter passed to `PauseCommand`. |
| `ICommand? StopCommand` | Gets or sets the command invoked when the stop button is clicked. The stop button is disabled whenever this command reports that it cannot execute. |
| `object? StopCommandParameter` | Gets or sets the parameter passed to `StopCommand`. |
| `bool ShowPlayButton` | Gets or sets a value indicating whether the play button is shown. |
| `bool ShowPauseButton` | Gets or sets a value indicating whether the pause button is shown. |
| `bool ShowStopButton` | Gets or sets a value indicating whether the stop button is shown. |
| `object? PlayToolTip` | Gets or sets the tool tip shown for the play button. |
| `object? PauseToolTip` | Gets or sets the tool tip shown for the pause button. |
| `object? StopToolTip` | Gets or sets the tool tip shown for the stop button. |
| `double IconSize` | Gets or sets the rendered width and height of each transport icon. |
| `Thickness ButtonPadding` | Gets or sets the padding applied around each transport icon. |
| `Orientation Orientation` | Gets or sets the direction the transport buttons are laid out in. |
| `Brush? PlayBrush` | Gets or sets the brush used to draw the play icon when it is enabled. |
| `Brush? PauseBrush` | Gets or sets the brush used to draw the pause icon when it is enabled. |
| `Brush? StopBrush` | Gets or sets the brush used to draw the stop icon when it is enabled. |
| `Brush? DisabledBrush` | Gets or sets the brush used to draw an icon whose command cannot currently execute. |
| `event RoutedEventHandler PlayClick` | Occurs when the play button is clicked. |
| `event RoutedEventHandler PauseClick` | Occurs when the pause button is clicked. |
| `event RoutedEventHandler StopClick` | Occurs when the stop button is clicked. |


## Example

```xml
<mosaic:ExecutionControl PlayCommand="{Binding RunCommand}"
                         StopCommand="{Binding StopCommand}" />
```
