# Font selectors

## FontAutoCompleteBox

**Base class:** `AutoCompleteBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Font/FontAutoCompleteBox.cs`

An `AutoCompleteBox` that suggests the font families installed on the system, rendering each suggestion in the font it names.

### API starting points

| Member | Purpose |
|---|---|
| `FontFamily? SelectedFontFamily` | Gets or sets the selected font family. A family that is not installed cannot be selected and results in no selection. |
| `string? SelectedFontName` | Gets or sets the name of the selected font family, for example Segoe UI. This is the convenient binding surface for settings that persist a font by name. |
| `bool ShowFontPreview` | Gets or sets a value indicating whether each suggested font name is rendered in its own font. Enabled by default. |
| `double PreviewFontSize` | Gets or sets the font size used to render the suggestion previews when `ShowFontPreview` is enabled. |

## FontComboBox

**Base class:** `ComboBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Font/FontComboBox.cs`

A `ComboBox` that lists every font family installed on the system.

### API starting points

| Member | Purpose |
|---|---|
| `FontFamily? SelectedFontFamily` | Gets or sets the selected font family. A family that is not installed cannot be selected and results in no selection. |
| `string? SelectedFontName` | Gets or sets the name of the selected font family, for example Segoe UI. This is the convenient binding surface for settings that persist a font by name. |
| `bool ShowFontPreview` | Gets or sets a value indicating whether each font name is rendered in its own font. |
| `double PreviewFontSize` | Gets or sets the font size used to render the previews when `ShowFontPreview` is enabled. |

## FontStyleComboBox

**Base class:** `ComboBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Font/FontStyleComboBox.cs`

A `ComboBox` that lists the `FontStyle` values, each one rendered in the style it names.

### API starting points

| Member | Purpose |
|---|---|
| `FontStyle SelectedFontStyle` | Gets or sets the selected font style. A style that is not one of the known styles cannot be selected and results in no selection. |
| `string? SelectedFontStyleName` | Gets or sets the name of the selected font style, for example Italic. This is the convenient binding surface for settings that persist a style by name. |
| `bool ShowStylePreview` | Gets or sets a value indicating whether each style name is rendered in its own style. |
| `double PreviewFontSize` | Gets or sets the font size used to render the previews when `ShowStylePreview` is enabled. |

## FontWeightComboBox

**Base class:** `ComboBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Font/FontWeightComboBox.cs`

A `ComboBox` that lists the standard `FontWeight` values, each one rendered in the weight it names.

### API starting points

| Member | Purpose |
|---|---|
| `FontWeight SelectedFontWeight` | Gets or sets the selected font weight. A weight that is not one of the standard weights cannot be selected and results in no selection. |
| `string? SelectedFontWeightName` | Gets or sets the name of the selected font weight, for example SemiBold. This is the convenient binding surface for settings that persist a weight by name. |
| `bool ShowWeightPreview` | Gets or sets a value indicating whether each weight name is rendered in its own weight. |
| `double PreviewFontSize` | Gets or sets the font size used to render the previews when `ShowWeightPreview` is enabled. |
