# ColorPicker

**Base class:** `UserControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ColorPicker/ColorPicker.xaml.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ColorPickerExample.xaml`

## Description

A visual color picker control with a hex input field, a color-canvas gradient picker, HSB sliders, and a preset color palette. The selected color is exposed as both a `SolidColorBrush` and a `Color`.

`ColorPicker` is a `UserControl` and cannot be re-templated.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `CornerRadius` | `CornerRadius` | `0` | Corner radius for the control's border. |
| `SelectedBrush` | `SolidColorBrush` | `null` | The currently selected color as a brush. Bindable. |
| `SelectedColor` | `Color` | `Colors.Black` | The currently selected color value. Bindable. |
| `HexValue` | `string` | `"#FF000000"` | The hex string representation of the selected color. Bindable. |

## Events

| Event | Args | Description |
|---|---|---|
| `ColorChanged` | `ColorChangedEventArgs` | Raised when the user selects a different color. `ColorChangedEventArgs` carries `OldColor` and `NewColor`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:ColorPicker
    CornerRadius="4"
    ColorChanged="ColorPicker_ColorChanged"
    SelectedColor="{Binding CurrentColor, Mode=TwoWay}" />
```

## Code-Behind

```csharp
private void ColorPicker_ColorChanged(object sender, ColorChangedEventArgs e)
{
    // e.OldColor, e.NewColor
    MyBrush = new SolidColorBrush(e.NewColor);
}
```

## Notes

- `SelectedBrush` and `SelectedColor` are kept in sync automatically.
- The preset palette is populated from `ColorPickerItem` objects defined in the template.
- A popup hosts the full picker UI; the collapsed state shows only a color swatch and the hex value.
