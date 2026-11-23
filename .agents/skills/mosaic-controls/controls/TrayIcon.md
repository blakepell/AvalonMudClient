# TrayIcon

**Base class:** `Freezable`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/TrayIcon/TrayIcon.cs`

Displays an icon in the Windows notification area (system tray) with a genuine WPF `System.Windows.Controls.ContextMenu`, click events and commands. No Windows Forms or System.Drawing dependency.

**Demo:** `src/MosaicWpfDemo/Views/Examples/TrayIconExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `ImageSource? Icon` | Gets or sets the image shown in the notification area. Any `ImageSource` works, including PNG/ICO resources and vector `DrawingImage`s; it is rendered at the shell's small-icon size for the current DPI. When `null` (or not renderable), the executable's own icon is used. |
| `string? ToolTipText` | Gets or sets the native tooltip shown when hovering the icon. Text longer than 127 characters is truncated. |
| `ContextMenu? ContextMenu` | Gets or sets the WPF context menu opened when the user right-clicks the icon or presses Shift+F10 / the Menu key on it. |
| `bool IsVisible` | Gets or sets whether the icon is registered in the notification area. This is the same state that `Show` and `Hide` change. Always `false` after disposal. |
| `bool IsRegistered` | Gets whether the shell has actually accepted the icon. This can be `false` while `IsVisible` is `true` if Explorer is not running yet (for example at logon); registration is retried automatically when the taskbar is created. |
| `object? DataContext` | Gets or sets the data context the `ContextMenu` is bound to (unless the menu sets its own). |
| `ICommand? LeftClickCommand` | Gets or sets the command executed after `LeftClick`. |
| `ICommand? DoubleClickCommand` | Gets or sets the command executed after `DoubleClick`. |
| `ICommand? RightClickCommand` | Gets or sets the command executed after `RightClick` and before the context menu opens. |
| `object? CommandParameter` | Gets or sets the parameter passed to `LeftClickCommand`, `DoubleClickCommand` and `RightClickCommand`. |
| `event EventHandler<TrayIconEventArgs>? LeftClick;` | Occurs when the icon is left-clicked, or activated with Enter/Space. Delayed by the double-click time when double-click is also handled. |
| `event EventHandler<TrayIconEventArgs>? DoubleClick;` | Occurs when the icon is double-clicked with the left button. |
| `event EventHandler<TrayIconEventArgs>? RightClick;` | Occurs when the user requests the icon's context menu with a right click or from the keyboard, before the menu opens. Set `TrayIconEventArgs.Handled` to suppress `RightClickCommand` and the menu. |
| `event EventHandler<TrayIconEventArgs>? MouseEnter;` | Occurs when the mouse cursor moves onto the icon. |
| `event EventHandler<TrayIconEventArgs>? MouseLeave;` | Occurs when the mouse cursor leaves the icon. |
| `void Show(…)` | Registers the icon in the notification area. Equivalent to setting `IsVisible` to `true`. |
| `void Hide(…)` | Removes the icon from the notification area. Equivalent to setting `IsVisible` to `false`. The instance can be shown again later. |
| `void ShowContextMenu(…)` | Opens `ContextMenu` next to the tray icon, as if the user had requested it from the keyboard. Does nothing if there is no menu or the icon is not shown. |
| `void Dispose(…)` | Removes the icon and releases all native resources. Safe to call repeatedly. |
| `void BeginInit(…)` | Defers native registration until `EndInit` (used by the XAML parser). |
| `void EndInit(…)` | Applies any deferred registration. |


## Usage notes

This is a `Freezable`, not a visual element. Keep an instance in resources or application code, configure its icon and menu, and dispose it when the owner shuts down. Use `IsRegistered` to inspect shell registration.
