# ModalDialog

**Base class:** `ContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ModalDialog/ModalDialog.cs`

A lookless modal dialog that displays its content centered in the adorner layer of a host element. While the dialog is open the host element (and everything beneath it) is blurred and dimmed; the dialog itself renders in the adorner layer and stays sharp. Await `ShowAsync` to get the result passed to `Close`.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ModalDialogExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `string Title` | Gets or sets the title displayed in the dialog header. |
| `string Description` | Gets or sets the optional secondary text displayed under the title. |
| `CornerRadius CornerRadius` | Gets or sets the radius used to round the dialog corners. |
| `bool ShowCloseButton` | Gets or sets whether the header close (X) button is shown. Clicking it closes the dialog with a null result. |
| `bool CloseOnEscape` | Gets or sets whether pressing Escape closes the dialog with a null result. |
| `bool CloseOnBackdropClick` | Gets or sets whether clicking the dimmed backdrop closes the dialog with a null result. |
| `Brush BackdropBrush` | Gets or sets the brush painted over the blurred host while the dialog is open. |
| `double BlurRadius` | Gets or sets the blur radius applied to the host element while the dialog is open. Set to zero to disable the blur and keep only the dimmed backdrop. |
| `event RoutedEventHandler Opened` | Raised after the dialog has been added to the adorner layer and displayed. |
| `event RoutedEventHandler Closed` | Raised after the dialog has been dismissed and removed from the adorner layer. |
| `bool IsOpen` | Gets whether the dialog is currently displayed. |
| `static ModalDialog? FindHost(…)` | Walks up the visual tree from an element inside the dialog content and returns the containing `ModalDialog`, or null when the element is not hosted in one. Useful for buttons inside dialog content that need to call `Close`. |
| `Task<bool?> ShowAsync(…)` | Shows the dialog modally over the provided host element and returns a task that completes with the value passed to `Close` when the dialog is dismissed. The host element (typically the window's root content) is blurred and dimmed while the dialog is open; the dialog itself renders sharp in the adorner layer above it. |
| `void Close(…)` | Closes the dialog, un-blurs the host and completes the task returned by `ShowAsync` with the provided result. Safe to call when already closed. |
| `void Detach(…)` | Removes the dialog from the adorner's tree so it can be shown again later. |


## Usage notes

Create a dialog, set its `Content`, then await `dialog.ShowAsync(hostElement)`. Call `dialog.Close(result)` to complete the task. The host needs an adorner layer; the demo shows the host arrangement.
