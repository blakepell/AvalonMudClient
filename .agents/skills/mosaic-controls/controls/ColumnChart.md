# ColumnChart

**Base class:** `UserControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ColumnChart/ColumnChart.xaml.cs`

A responsive column (bar) chart that renders an `ObservableCollection{T}` of `ColumnChartItem` onto a canvas. The chart repaints whenever it is resized, whenever the collection changes, and whenever a property on any item in the collection changes.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ColumnChartExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `ObservableCollection<ColumnChartItem> Items` | Gets or sets the collection of column items to be displayed in the chart. The chart subscribes to the collection and to each item, repainting when either changes. |
| `Brush ColumnBrush` | Gets or sets the brush used to fill columns that do not specify their own `ColumnChartItem.ColumnBrush`. Defaults to the Mosaic theme accent brush. |
| `Brush StrokeBrush` | Gets or sets the brush used to draw the axis and lines. Defaults to the Mosaic theme control border brush. |
| `double StrokeThickness` | Gets or sets the thickness of the axis and lines. |
| `int IntervalCount` | Gets or sets the number of intervals to be displayed on the Y-axis. |
| `Thickness InnerPadding` | Gets or sets the inner padding of the chart area, which is the space reserved around the plot for the axis labels and column name. |
| `ICommand? ColumnClickCommand` | Gets or sets a command invoked when a column is clicked. The clicked `ColumnChartItem` is passed as the command parameter, which is the MVVM equivalent of handling `ColumnClicked`. |
| `event EventHandler<ColumnChartItemEventArgs> ColumnClicked` | Occurs when a column is clicked. The `ColumnChartItemEventArgs.Item` on the event args is the `ColumnChartItem` that backs the clicked column. |
| `void Redraw(…)` | Clears the canvas and repaints the chart at the control's current size. |


## Usage notes

Populate `Items` with `ColumnChartItem` instances. Collection changes and item property changes trigger redraws.
