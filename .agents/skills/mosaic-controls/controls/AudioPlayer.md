# AudioPlayer

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/AudioPlayer/AudioPlayer.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AudioPlayerExample.xaml`

## Description

An audio player with previous/play/stop/next transport controls, a seek slider, playlist navigation, and routed playback events. Playback uses WPF `MediaPlayer`, so supported formats follow Windows Media Foundation.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Source` | `Uri?` | `null` | Active audio file. |
| `Playlist` | `ObservableCollection<Uri>` | empty | Tracks navigated by Previous/Next. |
| `CurrentIndex` | `int` | `-1` | Active playlist index. |
| `Position` | `TimeSpan` | `Zero` | Two-way playback position; setting seeks. |
| `Duration` | `TimeSpan` | `Zero` | Read-only active track length. |
| `Volume` | `double` | `0.5` | Playback volume, clamped 0.0-1.0. |
| `IsPlaying` | `bool` | `false` | Read-only playback state. |
| `AutoPlay` | `bool` | `false` | Starts playback when a new source loads. |
| `AutoAdvance` | `bool` | `true` | Advances to next playlist track at end. |
| `CornerRadius` | `CornerRadius` | `6` | Outer border radius. |

## Members

Methods: `Play()`, `Pause()`, `Stop()`, `Next()`, `Previous()`, `Seek(TimeSpan)`.

Events: `PlaybackStarted`, `PlaybackStopped`, `MediaOpened`, `MediaEnded`, `TrackChanged`.

## AudioPlayerSpectrumAnalyzer

`AudioPlayerSpectrumAnalyzer` is a paired `FrameworkElement` that captures the default Windows output device through WASAPI loopback and renders FFT frequency bars.

Key properties: `IsActive`, `BandCount`, `Background`, `BarBrush`, `IntensityBrush`, `UseColorIntensity`, `ShowPeaks`, `PeakBrush`, `BarSpacing`, `Sensitivity`, `CornerRadius`. Event: `OnError`.

```xml
<mosaic:AudioPlayer Source="{Binding TrackUri}" />
<mosaic:AudioPlayerSpectrumAnalyzer Height="140" IsActive="True" BandCount="32" />
```
