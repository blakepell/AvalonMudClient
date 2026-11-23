# Webcam

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Webcam/Webcam.cs`

Displays a live webcam feed captured through Windows Media Foundation.

**Demo:** `src/MosaicWpfDemo/Views/Examples/WebcamExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `WebcamDevice? Device` | Gets or sets the camera to display. Changing it while running switches to the new camera; setting it to `null` stops the preview until a device is set again. |
| `WebcamResolution? Resolution` | Gets or sets the requested capture mode, normally one of `AvailableResolutions`. `null` (the default) selects the best mode near 1920 × 1080 @ 30 fps automatically. If the device does not offer the requested mode, the closest one is used; see `ActualResolution`. |
| `Stretch Stretch` | Gets or sets how the video is scaled to fill the control. The default is `System.Windows.Media.Stretch.Uniform`. |
| `bool Mirror` | Gets or sets whether the preview is flipped horizontally, which feels natural for front-facing cameras. Mirroring is a render transform; frame data and `CaptureFrameAsync` are unaffected. |
| `bool AutoStart` | Gets or sets whether the camera starts automatically when the control is loaded and a `Device` is set. Failures are reported through `Error`. |
| `WebcamState State` | Gets the lifecycle state of the camera. |
| `bool IsRunning` | Gets whether the camera is open and delivering frames (`State` is `WebcamState.Running`). |
| `IReadOnlyList<WebcamResolution> AvailableResolutions` | Gets the capture modes offered by the current `Device`, largest first. Populated when the camera starts and cleared when a different device is selected. Use `WebcamDeviceManager.GetResolutionsAsync` to query a device that is not running. |
| `WebcamResolution? ActualResolution` | Gets the capture mode actually negotiated with the device, or `null` when not running. The frame size is in camera pixels and is independent of display DPI. |
| `static RoutedUICommand StartCommand` | Starts the target `Webcam`. Can execute when a device is set and the camera is stopped. |
| `static RoutedUICommand StopCommand` | Stops the target `Webcam`. Can execute while the camera is starting or running. |
| `event RoutedPropertyChangedEventHandler<WebcamState> StateChanged` | Occurs when `State` changes. |
| `event EventHandler? Started;` | Occurs on the UI thread when the camera has started and frames are flowing. Raised again after each successful switch to a different device or resolution. |
| `event EventHandler? Stopped;` | Occurs on the UI thread when the camera has been stopped and all of its resources released. An unexpected stop raises `Error` instead and leaves `State` at `WebcamState.Error`. |
| `event EventHandler<WebcamErrorEventArgs>? Error;` | Occurs on the UI thread when the camera fails to start or stops unexpectedly (for example because it was disconnected). Resources are already released when this is raised. |
| `event EventHandler<WebcamFrameEventArgs>? FrameReceived;` | Occurs on the UI thread after each displayed frame. Frames skipped because the UI was busy are not reported. Costs nothing when there are no subscribers; see `WebcamFrameEventArgs`. |
| `Task StartAsync(…)` | Opens `Device` and starts the preview. Calling it while starting or running does nothing. |
| `Task StopAsync(…)` | Stops the preview and releases the camera. Calling it while already stopped does nothing. When the returned task completes the device is free for other applications. |
| `async Task<BitmapSource?> CaptureFrameAsync(…)` | Captures the frame currently shown in the preview without interrupting the camera. |


## Usage notes

Enumerate devices with `WebcamDeviceManager.GetDevices()`, bind `Device`, then await `StartAsync()` or enable `AutoStart`. `Resolution` requests a mode; inspect `ActualResolution` for the negotiated mode. Await `StopAsync()` to release the camera. Unloading suspends capture, and loading resumes it if still started.

## Example

```xml
<mosaic:Webcam Device="{Binding SelectedCamera}" AutoStart="True" />
```
