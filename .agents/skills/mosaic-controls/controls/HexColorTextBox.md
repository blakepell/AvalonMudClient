# HexColorTextBox

**Base class:** `ComboBox`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/HexColorTextBox/HexColorTextBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ColorPickerExample.xaml`

## Description

A `ComboBox`-based control that allows typing hex color strings (`#RGB`, `#RRGGBB`, `#AARRGGBB`) or named colors, and optionally shows a drop-down list of color shades. `HexColor` and `Color` are kept in sync automatically.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `HexColor` | `string` | `"#FF000000"` | The hex string (normalized to `#AARRGGBB`) displayed and edited by the user. Bindable two-way. |
| `Color` | `Color` | `Colors.Black` | The `Color` struct corresponding to `HexColor`. Bindable two-way. |
| `ColorShades` | `ObservableCollection<Color>` | `null` | Optional collection of shades shown in the drop-down list. |
| `CornerRadius` | `CornerRadius` | `0` | Corner radius for the control border. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Basic hex input -->
<mosaic:HexColorTextBox
    Width="160"
    HexColor="{Binding ThemeColor, Mode=TwoWay}" />

<!-- With shade drop-down -->
<mosaic:HexColorTextBox
    Width="160"
    Color="{Binding SelectedColor, Mode=TwoWay}"
    ColorShades="{Binding BlueShades}" />
```

## Notes

- Typing an invalid hex string is silently ignored; the control reverts to the previous valid value when focus leaves.
- `#RGB` shorthand is expanded to `#FFRRGGBB` automatically.
- Named color strings (e.g., `"CornflowerBlue"`) are also accepted and converted.
- Bind either `HexColor` (string) or `Color` (struct) depending on your ViewModel's type.
