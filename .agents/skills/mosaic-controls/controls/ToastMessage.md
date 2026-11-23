# ToastMessage

**Base class:** `UserControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Toast/ToastMessage.xaml.cs`

A single toast notification card. Toasts either auto-dismiss after `Duration` elapses or, when no duration is provided, stay open until closed via the close button (or `Dismiss`). Display is normally handled by `ToastManager`.

### API starting points

| Member | Purpose |
|---|---|
| `ToastSeverity Severity` | The severity of the toast. |
| `TimeSpan? Duration` | How long the toast stays open, or null if it stays open until dismissed. |
| `event EventHandler<ToastDismissedEventArgs>? Dismissed;` | Raised once when the toast has been dismissed for any reason. |
| `void Dismiss(…)` | Dismisses the toast with a short exit animation and raises `Dismissed`. |


## ToastManager

**Base class:** `undefined`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Toast/ToastManager.cs`

Displays `ToastMessage` notifications in an adorner that overlays a host element. Toasts stack within their `ToastQuadrant` (newest closest to the corner) and remaining toasts shift to fill the gap when one closes.

### API starting points

| Member | Purpose |
|---|---|
| `static ToastManager? Default` | The default application wide instance, available after `Initialize` is called. |
| `static ToastManager Initialize(…)` | Creates the `Default` instance over the provided element. Typically called once with the main window's root content element. |
| `UIElement AdornedElement` | The element whose adorner layer hosts the toasts. |
| `int ActiveCount` | The number of toasts currently being displayed. |
| `static ToastManager? ForElement(…)` | Gets the manager for the top most toast surface above `element`, creating it on first use and sharing it with every other caller on the same surface. Returns null when the element is not yet in a tree that can host toasts. |
| `static UIElement? FindHost(…)` | Finds the top most element above `element` that can host toasts: the root content of its window, or the content of the outermost `AdornerDecorator` when the element is not in a window. This deliberately does not rely on `Window.GetWindow` alone. Elements hosted inside an `Adorner` (a `ModalDialog`'s content, for instance) do not inherit the window service property, so `Window.GetWindow` returns null for them; walking the visual tree escapes the adorner and finds the real surface. |
| `event EventHandler? ToastShown;` | Raised when a toast is shown. |
| `event EventHandler? AllDismissed;` | Raised when the last open toast has been dismissed. |
| `ToastMessage Show(…)` | Shows a toast notification. |
| `void DismissAll(…)` | Dismisses every open toast. |
| `Grid Root` | The panel the quadrant stacks are placed in. |

Use `ToastManager.ForElement(element)` for the shared surface manager, or `Initialize(host)` to configure `Default`. The host must have an adorner layer. Construct a `ToastMessage(title, message, severity, duration)`; a null duration keeps it open until dismissed.
