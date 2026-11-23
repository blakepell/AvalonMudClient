# CheckBoxList

**Base class:** `ListBox`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/CheckBoxList/CheckBoxList.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/CheckBoxListExample.xaml`

## Description

A `ListBox` variant that defaults to `SelectionMode.Multiple` and renders each item with a checkbox beside it (via `CheckBoxListItem`). All standard `ListBox`/`ItemsControl` properties and events apply.

## Key Properties

Inherits all `ListBox` properties. Notable defaults:

| Property | Default | Notes |
|---|---|---|
| `SelectionMode` | `Multiple` | Overridden from `Single`. Can still be changed per-instance. |
| `ItemsSource` | `null` | Bind your collection here. |
| `SelectedItems` | `IList` | Read the selection here (read-only in WPF; use `SelectionChanged` event). |

## Events

| Event | Description |
|---|---|
| `SelectionChanged` | Raised when the checked selection changes (inherited from `ListBox`). |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- String list -->
<mosaic:CheckBoxList
    ItemsSource="{Binding Options}"
    SelectionChanged="CheckBoxList_SelectionChanged" />

<!-- Object list with display member -->
<mosaic:CheckBoxList
    DisplayMemberPath="Name"
    ItemsSource="{Binding Categories}" />
```

## Code-Behind: Reading Selections

```csharp
private void CheckBoxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var list = (CheckBoxList)sender;
    var selected = list.SelectedItems.Cast<string>().ToList();
}
```

## Notes

- `CheckBoxListItem` is the container type; it extends `ListBoxItem` and renders the checkbox in the template.
- Because `SelectedItems` is not a `DependencyProperty` in standard WPF, you cannot bind it two-way directly. Use `SelectionChanged` or a behavior to synchronize selections to your ViewModel.
