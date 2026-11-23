# HorizontalListBox

**Base class:** `ListBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/HorizontalListBox/HorizontalListBox.cs`

A `ListBox` variant that lays its items out horizontally as a row of cells which toggle on and off when clicked. Unlike `CheckBoxList` there is no checkbox glyph; selection is conveyed entirely by the cell's fill color.

**Demo:** `src/MosaicWpfDemo/Views/Examples/HorizontalListBoxExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `double ItemSpacing` | The amount of space rendered between each cell. |
| `CornerRadius ItemCornerRadius` | The corner radius applied to each cell. Use a radius of 0 with an `ItemSpacing` of 0 for a flush, segmented appearance. |
| `double ItemMinWidth` | The minimum width of each cell. Keeps short values such as day abbreviations evenly sized. |
| `Thickness ItemPadding` | The padding applied inside each cell. |
| `Brush? SelectedBackground` | The background brush used by a cell that is toggled on. When null the theme accent brush is used. |
| `Brush? SelectedForeground` | The foreground brush used by a cell that is toggled on. When null the theme selection foreground is used. |


## Example

```xml
<mosaic:HorizontalListBox ItemsSource="{Binding Options}" />
```
