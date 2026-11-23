# EditableTextBlock

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/EditableTextBlock/EditableTextBlock.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/EditableTextBlockExample.xaml`

## Description

An inline editable control that displays as a `TextBlock` in view mode and switches to a `TextBox` in edit mode. The user triggers edit mode by double-clicking (configurable) or by calling `EditMode()` programmatically. Pressing Enter or losing focus commits the change and raises `TextUpdated`.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_TextBlock` | `TextBlock` | Displayed in view mode. |
| `PART_TextBox` | `TextBox` | Activated in edit mode. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | The displayed/edited text value. Bind two-way. |
| `TextBlockForegroundColor` | `Brush` | theme default | Foreground brush for the view-mode TextBlock. |
| `TrimOnTextUpdated` | `bool` | `true` | Trims whitespace from the value when committing. |
| `IsDoubleClickToEditEnabled` | `bool` | `true` | Whether a double-click triggers edit mode. |
| `IsFilePath` | `bool` | `false` | Shows a `…` browse button and handles file-path display. |
| `TextBoxPadding` | `Thickness` | `0` | Padding inside the edit-mode TextBox. |

## Events

| Event | Args | Description |
|---|---|---|
| `TextUpdated` | `RoutedEventArgs` | Raised when the user commits a change (Enter / focus lost). |

## Methods

| Method | Description |
|---|---|
| `ViewMode()` | Switch the control to read-only view mode. |
| `EditMode()` | Switch the control to inline edit mode programmatically. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:EditableTextBlock
    Text="{Binding ItemName, Mode=TwoWay}"
    TextUpdated="EditableTextBlock_TextUpdated" />

<!-- Read-only (no double-click) -->
<mosaic:EditableTextBlock
    Text="{Binding DisplayName}"
    IsDoubleClickToEditEnabled="False" />
```

## Notes

- The control reverts to view mode automatically when focus is lost.
- `TrimOnTextUpdated=True` prevents leading/trailing whitespace from being saved.
- When `IsFilePath=True`, the control adds a browse button that opens an `OpenFileDialog`.
