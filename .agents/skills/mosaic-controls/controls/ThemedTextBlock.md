# ThemedTextBlock

**Base class:** `TextBlock`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ThemedTextBlock/ThemedTextBlock.cs`

A `TextBlock` that selects a light or dark foreground from an explicit `BackgroundBrush` or the nearest supported visual ancestor background.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ThemedTextBlockExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `Brush? BackgroundBrush` | Gets or sets an explicit background brush used to calculate the foreground. When null, the nearest supported ancestor background is used. |
| `double LuminanceThreshold` | Gets or sets the relative-luminance value at or below which the light foreground is selected. Raising the value favors the light foreground. |
| `Brush LightForegroundBrush` | Gets or sets the foreground used on backgrounds at or below `LuminanceThreshold`. |
| `Brush DarkForegroundBrush` | Gets or sets the foreground used on backgrounds above `LuminanceThreshold`. |
| `string? PaletteFamily` | Gets or sets the optional Mosaic palette family used for a family-specific foreground switch point. Set this together with `PaletteShade`. |
| `int PaletteShade` | Gets or sets the optional Mosaic palette shade used with `PaletteFamily`. A value of zero disables palette-specific selection. |
