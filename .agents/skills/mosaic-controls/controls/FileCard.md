# FileCard

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/FileCard/FileCard.cs`

A clickable card that represents a single file on disk, showing the operating system's icon for the file type alongside the file name and its formatted size.

**Demo:** `src/MosaicWpfDemo/Views/Examples/FileCardExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `string? FilePath` | Gets or sets the full path of the file the card represents. Only the file name portion is rendered; the full path is shown as the tooltip on the file name. |
| `string? DisplayName` | Gets or sets an optional name shown on the card in place of the file name portion of `FilePath`. |
| `bool IsTintEnabled` | Gets or sets a value indicating whether the card background is tinted with the dominant color of the file's icon. When false the card uses the theme's control background color unaltered. |
| `CornerRadius CornerRadius` | Gets or sets the corner radius of the card. |
| `double IconSize` | Gets or sets the rendered width and height of the file icon. |
| `ICommand? Command` | Gets or sets the command invoked when the card is clicked. When `CommandParameter` has not been set the command receives the card's `FilePath`. |
| `object? CommandParameter` | Gets or sets the parameter passed to `Command`. Leave this unset to have the card pass its `FilePath` instead. |
| `bool OpenFileOnClick` | Gets or sets a value indicating whether clicking the card opens `FilePath` through the operating system shell. The default is false. |
| `string FileName` | Gets the name the card displays: `DisplayName` when one has been supplied, otherwise the file name portion of `FilePath`. |
| `string FileSizeText` | Gets the formatted file size (for example 2.1 MB), or an empty string when the file does not exist. |
| `ImageSource? Icon` | Gets the shell icon for the file type, or the error glyph when the file is missing. |
| `bool FileExists` | Gets a value indicating whether `FilePath` was found on disk the last time it was read. |
| `Brush? CardBackground` | Gets the brush the card paints itself with: the theme's control background color, optionally tinted with the icon's dominant hue. |
| `bool IsPressed` | Gets a value indicating whether the card is currently being pressed. The default template uses this to lower the card. |
| `event EventHandler<Exception>? OnError;` | Occurs when `OpenFileOnClick` is enabled and the operating system shell cannot open the file. Missing files are ignored and do not raise this event. |
| `event RoutedEventHandler Click` | Occurs when the card is clicked with the mouse or activated from the keyboard. |
| `void Refresh(…)` | Re-reads `FilePath` from disk and updates the displayed name, size, icon, and tint. |
| `void RaiseClick(…)` | Raises the `Click` routed event, executes `Command` with the card's file path (or `CommandParameter` when one has been supplied), and optionally opens the file through the operating system shell. |


## Example

```xml
<mosaic:FileCard FilePath="{Binding Path}" />
```
