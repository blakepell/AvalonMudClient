# EnumComboBox

**Base class:** `ComboBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/EnumComboBox/EnumComboBox.cs`

A `ComboBox` that populates itself from the declared members of an enum type assigned to `EnumType`.

**Demo:** `src/MosaicWpfDemo/Views/Examples/EnumComboBoxExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `Type? EnumType` | Gets or sets the enum type whose declared members populate the drop-down. Nullable enum types are resolved to their underlying enum type. Setting null clears the generated items; assigning a non-enum type throws an `ArgumentException`. |


## Usage notes

Bind `SelectedValue` to the enum value; `SelectedItem` is an `EnumComboBoxItem` wrapper. Set `EnumType` rather than `ItemsSource`. Labels prefer `[EnumDisplayName]`, then `[Description]`, then the member name. Nullable enums are supported; flags enums show only declared members.

## Example

```xml
<mosaic:EnumComboBox EnumType="{x:Type local:OrderStatus}"
                     SelectedValue="{Binding Status, Mode=TwoWay}" />
```
