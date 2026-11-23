# Waveform Visualizers

**Base class:** `WaveformVisualizerBase : FrameworkElement`  
**Namespace:** `Mosaic.UI.Wpf.Controls.WaveformVisualizer`  
**Sources:**  
`src/Mosaic.UI.Wpf/Controls/WaveformVisualizer/WaveformVisualizerBase.cs`  
`src/Mosaic.UI.Wpf/Controls/WaveformVisualizer/LoopbackWaveformVisualizer.cs`  
`src/Mosaic.UI.Wpf/Controls/WaveformVisualizer/InputWaveformVisualizer.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/LoopbackWaveformVisualizerExample.xaml`

## Description

Live audio waveform controls backed by Windows WASAPI capture. `LoopbackWaveformVisualizer` captures the default render/output device, while `InputWaveformVisualizer` captures a selectable recording/input device.

## Controls

| Control | Description |
|---|---|
| `LoopbackWaveformVisualizer` | Displays waveform data from the default Windows audio render device and follows default device changes. |
| `InputWaveformVisualizer` | Displays waveform data from a selected Windows audio input device. |

## Shared Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsListening` | `bool` | `true` | Starts or stops audio capture while the control is loaded. |
| `WaveformBrush` | `Brush` | deep sky blue | Brush used to draw the waveform line. |

## InputWaveformVisualizer Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `InputDevices` | `ReadOnlyObservableCollection<AudioInputDevice>` | discovered devices | Read-only collection of active input devices. |
| `SelectedInputDevice` | `AudioInputDevice` | default input device entry | Device captured by the visualizer. Changing it restarts capture. |

## Events

| Event | Type | Description |
|---|---|---|
| `OnError` | `EventHandler<Exception>` | Raised when device enumeration, capture, or rendering fails. Handler exceptions are swallowed. |

## XAML Example

```xml
xmlns:waveform="clr-namespace:Mosaic.UI.Wpf.Controls.WaveformVisualizer;assembly=Mosaic.UI.Wpf"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
xmlns:themes="http://schemas.apexgate.net/wpf/mosaic-ui"

<waveform:LoopbackWaveformVisualizer
    Height="80"
    IsListening="True"
    WaveformBrush="{DynamicResource {x:Static themes:MosaicTheme.AccentBrush}}" />

<ComboBox
    ItemsSource="{Binding InputDevices, ElementName=InputVisualizer}"
    SelectedItem="{Binding SelectedInputDevice, ElementName=InputVisualizer}" />

<waveform:InputWaveformVisualizer
    x:Name="InputVisualizer"
    Height="80"
    IsListening="True" />
```

## Code Example

```csharp
await inputVisualizer.RefreshInputDevicesAsync();
inputVisualizer.IsListening = true;
```

## Notes

- These controls live in `Mosaic.UI.Wpf.Controls.WaveformVisualizer`; `AssemblyInfo.cs` maps that namespace to the canonical Mosaic XAML URI.
- Capture starts on load when `IsListening` is `true` and stops on unload.
- `InputWaveformVisualizer.RefreshInputDevices()` starts a non-blocking refresh; `RefreshInputDevicesAsync()` can be awaited.
