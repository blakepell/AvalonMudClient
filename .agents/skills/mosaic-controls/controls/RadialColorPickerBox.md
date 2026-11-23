# RadialColorPickerBox

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/RadialColorPicker/RadialColorPickerBox.cs`

A compact color field that shows a swatch and an editable hex text box, with a drop-down button that opens a `RadialColorPicker` for full HSV selection. The hex value can be typed directly to set the color, or picked from the wheel in the drop-down. Mosaic theme aware.

### API starting points

| Member | Purpose |
|---|---|
| `Color SelectedColor` | Gets or sets the currently selected color (including its alpha channel). |
| `Brush SelectedBrush` | Gets or sets a `SolidColorBrush` representation of the selected color. |
| `string HexValue` | Gets or sets the hex string (normalized to #AARRGGBB) of the selected color. |
| `bool IsDropDownOpen` | Gets or sets a value indicating whether the color picker drop-down is open. |
| `bool ShowAlpha` | Gets or sets a value indicating whether the alpha (transparency) editor is shown in the drop-down picker. |
| `double WheelDiameter` | Gets or sets the diameter, in pixels, of the radial hue wheel shown in the drop-down. |
| `CornerRadius CornerRadius` | Gets or sets the corner radius of the control's outer border. |
| `event EventHandler<ColorChangedEventArgs>? ColorChanged;` | Occurs when the selected color changes. |


## Example

```xml
<mosaic:RadialColorPickerBox SelectedColor="{Binding Color, Mode=TwoWay}" />
```
