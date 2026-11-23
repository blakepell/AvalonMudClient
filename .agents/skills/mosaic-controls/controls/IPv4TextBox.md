# IPv4TextBox

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/IPv4TextBox/IPv4TextBox.cs`

Provides a single, themed editor for a four-segment IPv4 address.

**Demo:** `src/MosaicWpfDemo/Views/Examples/IPv4TextBoxExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `static RoutedUICommand CopyAddressCommand` | Gets the command that copies the complete value of `Text` to the clipboard. |
| `string Text` | Gets or sets the last complete, valid IPv4 address. Invalid assignments are ignored. |


## Example

```xml
<mosaic:IPv4TextBox Text="{Binding Address, Mode=TwoWay}" />
```
