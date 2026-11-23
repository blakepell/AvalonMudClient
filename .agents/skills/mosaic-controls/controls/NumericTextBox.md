# NumericTextBox

**Base class:** `TextBox`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/NumericTextBox/NumericTextBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/NumericTextBoxExample.xaml`

## Description

A `TextBox` subclass that restricts input to numeric characters only (digits, an optional leading minus, and an optional decimal separator). Non-numeric key presses are swallowed. `DecimalPlaces` limits the number of digits allowed after the decimal point.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `DecimalPlaces` | `int` | `0` | Number of decimal places allowed. `0` = integer-only. |

All standard `TextBox` properties (`Text`, `IsReadOnly`, `MaxLength`, etc.) apply normally.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Integer only -->
<mosaic:NumericTextBox Width="100" Text="{Binding Quantity, Mode=TwoWay}" />

<!-- Two decimal places -->
<mosaic:NumericTextBox
    Width="120"
    DecimalPlaces="2"
    Text="{Binding Price, Mode=TwoWay}" />
```

## Notes

- `DecimalPlaces` is a CLR property (not a `DependencyProperty`), so it cannot be bound directly.
- The control does not expose `Minimum`/`Maximum` properties; add validation or converter logic in the ViewModel if range limits are required.
- Paste operations are also filtered to strip non-numeric characters.
