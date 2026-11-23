# RadialColorPicker

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/RadialColorPicker/RadialColorPicker.cs`

A radial (HSV wheel) color picker. Colors are selected by clicking the hue ring that runs the full gamut around the perimeter; the inner square shows every shade of the selected hue for fine tuning. A side panel exposes the hex value (with a copy button) plus editable red, green, blue, and alpha (transparency) channels. The control is Mosaic theme aware.

### API starting points

| Member | Purpose |
|---|---|
| `Color SelectedColor` | Gets or sets the currently selected color (including its alpha channel). |
| `Brush SelectedBrush` | Gets or sets a `SolidColorBrush` representation of the selected color. |
| `string HexValue` | Gets or sets the hex string (normalized to #AARRGGBB) of the selected color. |
| `double WheelDiameter` | Gets or sets the diameter, in device-independent pixels, of the radial hue wheel. |
| `double RingThickness` | Gets or sets the thickness, in device-independent pixels, of the hue ring. |
| `bool ShowAlpha` | Gets or sets a value indicating whether the alpha (transparency) editor is shown. |
| `CornerRadius CornerRadius` | Gets or sets the corner radius of the control's outer border. |
| `event EventHandler<ColorChangedEventArgs>? ColorChanged;` | Occurs when the selected color changes. |


## Example

```xml
<mosaic:RadialColorPicker SelectedColor="{Binding Color, Mode=TwoWay}" />
```
