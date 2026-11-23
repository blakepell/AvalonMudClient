# AutoCompleteBox

**Base class:** `Selector` (CustomControl)\
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/AutoCompleteBox/AutoCompleteBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AutoCompleteBoxExample.xaml`

## Description

A searchable autocomplete input that shows a filtered drop-down list as the user types. Supports both synchronous `ItemsSource` collections and async `ItemsProvider` delegates. Fully MVVM-friendly with two-way bindings for `Text` and `SelectedItem`.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | The current text in the input box. Bind two-way. |
| `Watermark` | `string` | `null` | Placeholder text shown when the box is empty. |
| `ItemsSource` | `IEnumerable` | `null` | Synchronous list of items to filter. |
| `ItemsProvider` | `AutoCompleteBoxItemsProvider` | `null` | Async delegate `(string text, CancellationToken) => Task<IEnumerable>` for server-side search. |
| `SelectedItem` | `object` | `null` | The currently selected item. Bind two-way. |
| `DisplayMemberPath` | `string` | `null` | Property path on the item used to display text in the list. |
| `SelectedValuePath` | `string` | `null` | Property path on the item used as the value for `SelectedValue`. |
| `IsTextRequiredForSuggestions` | `bool` | `true` | When `false`, shows all items when the box is focused and empty. |
| `ItemTemplate` | `DataTemplate` | `null` | Custom data template for each drop-down item. |
| `LoadingContent` | `object` | `null` | Content displayed inside the drop-down while the async provider is loading. |
| `NoResultsContent` | `object` | `null` | Content displayed when the provider returns no results. |

## Events

| Event | Args | Description |
|---|---|---|
| `LookupFailed` | `AutoCompleteBoxLookupFailedEventArgs` | Raised when `ItemsProvider` throws an exception. |

## XAML Examples

### Synchronous list with display member path

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:AutoCompleteBox
    Width="240"
    DisplayMemberPath="Name"
    IsTextRequiredForSuggestions="False"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct, Mode=TwoWay}"
    Text="{Binding SearchText, Mode=TwoWay}"
    Watermark="Find a product" />
```

### Async provider with custom item template

```xml
<mosaic:AutoCompleteBox
    Width="240"
    ItemsProvider="{Binding AsyncLookup}"
    LoadingContent="Searching..."
    NoResultsContent="No matches"
    SelectedItem="{Binding SelectedResult, Mode=TwoWay}"
    Text="{Binding SearchText, Mode=TwoWay}"
    TextSearch.TextPath="Name"
    Watermark="Search database">
    <mosaic:AutoCompleteBox.ItemTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock FontWeight="SemiBold" Text="{Binding Name}" />
                <TextBlock Text="{Binding Category}" />
            </StackPanel>
        </DataTemplate>
    </mosaic:AutoCompleteBox.ItemTemplate>
</mosaic:AutoCompleteBox>
```

## Code-Behind: Async Provider

```csharp
// ViewModel property
public AutoCompleteBoxItemsProvider AsyncLookup => async (text, ct) =>
{
    var results = await _service.SearchAsync(text, ct);
    return results;
};
```

## Notes

- When using `DisplayMemberPath`, also set `TextSearch.TextPath` for the async provider path to ensure the selected item text appears correctly in the input after selection.
- `FilterMode` property controls client-side filtering (e.g., `Contains`, `StartsWith`).
- For plain `string` collections, omit `DisplayMemberPath` and `SelectedValuePath`.
