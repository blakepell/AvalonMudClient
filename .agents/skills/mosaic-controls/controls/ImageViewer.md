# ImageViewer

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ImageViewer/ImageViewer.cs`

An image viewer modeled after the Windows Photos viewer: the image opens fitted to the available space, the mouse wheel zooms around the cursor, click-and-drag pans when the image is larger than the viewport, double-click toggles between "fit" and actual size, and a status bar shows the resolution, file size and a zoom slider.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ImageViewerExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `static RoutedUICommand ZoomInCommand` | Zooms in one step around the center of the viewport. |
| `static RoutedUICommand ZoomOutCommand` | Zooms out one step around the center of the viewport. |
| `static RoutedUICommand ZoomToFitCommand` | Fits the entire image into the viewport. |
| `static RoutedUICommand ActualSizeCommand` | Shows the image at its actual size (100%). |
| `static RoutedUICommand RotateRightCommand` | Rotates the image 90 degrees clockwise. |
| `static RoutedUICommand RotateLeftCommand` | Rotates the image 90 degrees counter-clockwise. |
| `static RoutedUICommand SaveAsCommand` | Prompts for a location and saves the image as displayed (including rotation). |
| `static RoutedUICommand CopyPathCommand` | Copies the full path of the image file to the clipboard. Only available when the image was loaded from disk. |
| `static RoutedUICommand CopyImageCommand` | Copies the image as displayed (including rotation) to the clipboard. |
| `string? ImagePath` | Gets or sets the path of the image to display. Accepts a file system path, an absolute URI or a pack/relative resource URI (for example /Assets/photo.jpg). |
| `ImageSource? Source` | Gets or sets the image being displayed. This is set automatically when `ImagePath` is used, but may also be assigned directly. |
| `double Zoom` | Gets or sets the zoom level where 1.0 is 100% (one image pixel per screen pixel). Setting this value turns off `IsZoomToFit`. |
| `double MinZoom` | Gets or sets the minimum allowed `Zoom`. |
| `double MaxZoom` | Gets or sets the maximum allowed `Zoom`. |
| `bool IsZoomToFit` | Gets or sets whether the zoom tracks the viewport so the whole image is always visible. Images smaller than the viewport are shown at their actual size rather than being enlarged. Any manual zoom turns this off. |
| `double Rotation` | Gets or sets the display rotation in degrees. Values are normalized to 0, 90, 180 or 270. The rotation only affects how the image is shown; the source is not modified. |
| `bool ShowStatusBar` | Gets or sets whether the status bar (resolution, file size and zoom controls) is shown. |
| `bool HasImage` | Gets whether an image is currently loaded. |
| `int PixelWidth` | Gets the width of the loaded image in pixels. |
| `int PixelHeight` | Gets the height of the loaded image in pixels. |
| `long? FileSize` | Gets the size in bytes of the image loaded through `ImagePath`, or null when unknown. |
| `string? FilePath` | Gets the full path of the image file when the image was loaded from disk through `ImagePath`; null when it came from a resource, a remote URI or was set via `Source`. |
| `string ResolutionText` | Gets the display text for the image resolution, e.g. 1920 × 1080 px. |
| `string FileSizeText` | Gets the display text for the file size, e.g. 1.24 MB. |
| `event RoutedPropertyChangedEventHandler<double> ZoomChanged` | Occurs when the `Zoom` level changes. |
| `event RoutedEventHandler ImageLoaded` | Occurs after a new image has been loaded and its metadata is available. |
| `event EventHandler<ImageViewerFailedEventArgs> ImageFailed` | Occurs when the image referenced by `ImagePath` could not be loaded. |
| `event EventHandler<ImageViewerFailedEventArgs> OperationFailed` | Occurs when a save or clipboard operation started from a command (context menu or keyboard) fails. `ImageViewerFailedEventArgs.Operation` identifies which one. |
| `void LoadImage(…)` | Loads the image at the specified file path or resource URI. Equivalent to setting `ImagePath`. |
| `void ZoomIn(…)` | Zooms in one step, keeping the center of the viewport stationary. |
| `void ZoomOut(…)` | Zooms out one step, keeping the center of the viewport stationary. |
| `void ZoomToFit(…)` | Fits the whole image into the viewport and keeps it fitted as the control resizes. |
| `void ActualSize(…)` | Shows the image at its actual size (100%), keeping the center of the viewport stationary. |
| `void RotateRight(…)` | Rotates the displayed image 90 degrees clockwise. |
| `void RotateLeft(…)` | Rotates the displayed image 90 degrees counter-clockwise. |
| `void ZoomTo(…)` | Sets the zoom level while keeping the image point under `anchor` stationary. |
| `static string FormatFileSize(…)` | Formats a byte count using binary units, e.g. 1.24 MB. |
| `void SaveImage(…)` | Saves the image as displayed (including `Rotation`) to `path`. The format is chosen from the extension (.png, .jpg/.jpeg, .bmp, .gif, .tif/.tiff). When the image is not rotated and the extension matches the file it was loaded from, the original bytes are written unchanged so no quality or metadata is lost. |
| `void CopyImageToClipboard(…)` | Copies the image as displayed (including `Rotation`) to the clipboard. Both a standard bitmap and a PNG (which preserves transparency in apps that support it) are placed on the clipboard. |
| `void CopyPathToClipboard(…)` | Copies `FilePath` to the clipboard as text. |


## Example

```xml
<mosaic:ImageViewer ImagePath="{Binding ImagePath}" />
```
