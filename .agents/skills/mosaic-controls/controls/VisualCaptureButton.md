# VisualCaptureButton

**Base class:** `AccentButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/VisualCaptureButton/VisualCaptureButton.cs`

A button that renders another element to an image and either copies it to the clipboard or saves it to a file, as selected by `Mode`. The element is identified by `TargetName` (resolved through the button's name scope) or bound directly via `Target`. If the target scrolls, its entire scroll extent is captured.

### API starting points

| Member | Purpose |
|---|---|
| `UIElement? Target` | Gets or sets the element to capture. Takes precedence over `TargetName`. |
| `string? TargetName` | Gets or sets the x:Name of the element to capture. The name is resolved through the button's name scope and, failing that, the name scopes of its logical ancestors. |
| `VisualCaptureMode Mode` | Gets or sets whether the image is copied to the clipboard or saved to a file when clicked. |
| `string? FilePath` | Gets or sets the file the image is written to when `Mode` is `VisualCaptureMode.SaveToFile`. When empty the user is prompted with a save dialog. |
| `string? DefaultFileName` | Gets or sets the file name pre-populated in the save dialog. |
| `Brush? CaptureBackground` | Gets or sets a brush painted behind the captured element. Leave `null` to keep unpainted areas transparent. |
| `bool CaptureScrollExtent` | Gets or sets whether a scrolling target is expanded so its full scroll extent is captured. |
| `event EventHandler<VisualCapturedEventArgs> Captured` | Occurs after the image has been copied or saved. Not raised when the user cancels the save dialog. |
| `event EventHandler<VisualCapturedEventArgs> CaptureFailed` | Occurs when the capture could not be completed, for example because the target could not be found or the clipboard was unavailable. |
| `UIElement? ResolveTarget(…)` | Resolves the element that will be captured from `Target` or `TargetName`. |
| `bool Capture(…)` | Performs the capture described by `Mode` immediately, exactly as a click would. |


## Example

```xml
<mosaic:VisualCaptureButton Target="{Binding ElementName=Preview}"
                            Content="Capture" />
```
