# CopyTextBox

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/CopyTextBox/CopyTextBox.cs`

A Mosaic styled text box with an attached copy button that places the box's text onto the clipboard. The button is docked to the right edge of the box and shares the box's border, and an optional `ToastMessage` can be shown reporting whether the copy succeeded.

**Demo:** `src/MosaicWpfDemo/Views/Examples/CopyTextBoxExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `string Text` | Gets or sets the text displayed in the control and copied to the clipboard. |
| `string Watermark` | Gets or sets the placeholder text shown when the control is empty and unfocused. |
| `bool IsReadOnly` | Gets or sets a value indicating whether the text can be edited by the user. |
| `CornerRadius CornerRadius` | Gets or sets the corner radius of the control's outer border. |
| `Brush? SelectedBorderBrush` | Gets or sets the border brush used while the control has keyboard focus. |
| `Geometry? CopyIcon` | Gets or sets the geometry rendered inside the copy button. Defaults to a copy glyph and can be replaced with any icon geometry. |
| `object? CopyButtonToolTip` | Gets or sets the tool tip shown when hovering over the copy button. |
| `bool ShowToast` | Gets or sets a value indicating whether a `ToastMessage` is shown reporting whether the copy succeeded. This is off by default. |
| `ToastManager? ToastManager` | Gets or sets the `ToastNotifier` used to display notifications. When null the control falls back to `ToastNotifier.Default` and then to a manager created over the containing window's content. |
| `ToastQuadrant ToastQuadrant` | Gets or sets the quadrant the toast notification is displayed in. |
| `TimeSpan? ToastDuration` | Gets or sets how long the toast notification stays open. When null the toast remains open until the user closes it. |
| `string ToastSuccessTitle` | Gets or sets the title of the toast shown after a successful copy. |
| `string ToastSuccessMessage` | Gets or sets the body of the toast shown after a successful copy. |
| `string ToastErrorTitle` | Gets or sets the title of the toast shown when the copy fails. |
| `string ToastErrorMessage` | Gets or sets the body of the toast shown when the copy fails. When left at its default the operating system's error message is appended. |
| `ICommand? Command` | Gets or sets a command executed after the copy has been attempted. |
| `object? CommandParameter` | Gets or sets the parameter passed to `Command`. When null the control's `Text` is passed instead. |
| `event EventHandler<TextCopiedEventArgs> TextCopied` | Occurs after a copy has been attempted, reporting whether it succeeded. |
| `bool Copy(…)` | Places the control's `Text` onto the clipboard, raises `TextCopied` and, when `ShowToast` is set, displays a toast notification reporting the outcome. |
| `void SelectAllText(…)` | Selects all of the text and moves keyboard focus into the text box. |


## Example

```xml
<mosaic:CopyTextBox Text="{Binding Token}" IsReadOnly="True" />
```
