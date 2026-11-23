# StringListEditor

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/StringListEditor/StringListEditor.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/StringListEditorExample.xaml`

## Description

An interactive editor for a flat list of strings. Users type in a `TextBox` and press Enter (or an Add button) to append items; items can be deleted via a remove button inside the `ListBox`. Validates input and optionally prevents duplicates.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<string>` | empty | The underlying string list. Bind two-way for ViewModel integration. |
| `AllowDuplicates` | `bool` | `false` | When `false`, duplicate entries are rejected. |
| `MaxLength` | `int` | `0` | Maximum character length for each entry (0 = unlimited). |
| `Validate` | `Func<string, bool>` | `null` | Optional predicate; items failing validation are not added. |
| `TextBoxMargin` | `Thickness` | `5,5,5,5` | Margin around the input `TextBox`. |
| `ListBoxMargin` | `Thickness` | `5,0,5,5` | Margin around the items `ListBox`. |

## Methods

| Method | Description |
|---|---|
| `AddItem()` | Programmatically adds the current text-box content as a new item. |
| `DeleteItem(string)` | Removes the first matching item from the list. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:StringListEditor
    Items="{Binding Tags, Mode=TwoWay}"
    AllowDuplicates="False"
    MaxLength="50" />
```

## Code-Behind (Validation)

```csharp
// Only allow items matching a simple rule
stringListEditor.Validate = input => !string.IsNullOrWhiteSpace(input) && input.Length >= 2;
```

## Notes

- `Items` must be an `ObservableCollection<string>` for live updates to propagate back to bindings.
- Pressing **Enter** inside the `TextBox` triggers `AddItem()`.
- Empty or whitespace-only strings are always rejected regardless of `AllowDuplicates` or `Validate`.
