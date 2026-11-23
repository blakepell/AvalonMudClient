# SearchBox

**Base class:** `TextBox`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SearchBox/SearchBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SearchBoxExample.xaml`

## Description

A `TextBox` subclass designed for search input. It adds a watermark placeholder, a clear (×) button when text is present, a `HasText` flag, and a `SearchExecuted` routed event raised when the user presses Enter.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_ClearButton` | `Button` | The × clear button shown when text is non-empty. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Watermark` | `string` | `null` | Placeholder text shown when the box is empty and unfocused. |
| `HasText` | `bool` (read-only) | `false` | `true` when `Text` is non-empty. Use in triggers/styles. |
| `ClearTextOnEnter` | `bool` | `false` | Clears the text after `SearchExecuted` fires on Enter. |
| `FocusControlOnEnter` | `UIElement` | `null` | Moves keyboard focus to this element after Enter is pressed. |
| `SelectedBorderBrush` | `Brush` | theme accent | Border brush used when the box has keyboard focus. |

## Events

| Event | Args | Description |
|---|---|---|
| `SearchExecuted` | `RoutedEventArgs` | Raised when the user presses Enter. Read `Text` at this point. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:SearchBox
    Width="240"
    Watermark="Search…"
    ClearTextOnEnter="False"
    SearchExecuted="SearchBox_SearchExecuted"
    Text="{Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

## Code-Behind

```csharp
private void SearchBox_SearchExecuted(object sender, RoutedEventArgs e)
{
    var query = ((SearchBox)sender).Text;
    ViewModel.Search(query);
}
```

## Notes

- The clear button visibility is driven by `HasText`; no extra trigger is needed.
- `Text` updates on every keystroke by default (standard `TextBox` behavior). Set `UpdateSourceTrigger=PropertyChanged` for live filtering.
- Press Enter to fire `SearchExecuted`; clicking the clear button empties `Text` and fires `SearchExecuted` with an empty string.
