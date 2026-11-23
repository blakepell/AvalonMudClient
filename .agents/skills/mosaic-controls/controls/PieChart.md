# PieChart

**Base class:** `UserControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/PieChart/PieChart.xaml.cs`

A pie chart that renders an `ObservableCollection{T}` of `PieCategory` onto a canvas. Each slice is sized by its `PieCategory.Value` relative to the total of every value in the collection, so the values do not have to add up to any particular number. The chart repaints whenever it is resized, whenever the collection changes, and whenever a property on any category in the collection changes.

**Demo:** `src/MosaicWpfDemo/Views/Examples/PieChartExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `ObservableCollection<PieCategory> Categories` | Gets or sets the collection of pie categories to be displayed in the chart. The chart subscribes to the collection and to each category, repainting when either changes. |
| `Brush StrokeBrush` | Gets or sets the brush used to draw the lines that separate the pie slices. |
| `double StrokeThickness` | Gets or sets the thickness of the lines that separate the pie slices. |
| `LegendPosition LegendPosition` | Gets or sets the position of the pie chart legend. |
| `ICommand? SliceClickCommand` | Gets or sets a command invoked when a slice is clicked. The clicked `PieCategory` is passed as the command parameter, which is the MVVM equivalent of handling `SliceClicked`. |
| `event EventHandler<PieCategoryEventArgs> SliceClicked` | Occurs when a slice is clicked. The `PieCategoryEventArgs.Category` on the event args is the `PieCategory` that backs the clicked slice. |
| `void Redraw(…)` | Clears the canvas and repaints the pie at the control's current size. |


## Usage notes

Populate `Categories` with `PieCategory` instances. Values determine relative slice sizes and do not need to sum to 100.
