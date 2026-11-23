# LineGraph

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/LineGraph/LineGraph.cs`

A line graph that plots one or more `LineGraphSeries` of time stamped values. Values run up the Y-axis on the left and time runs along the X-axis at the bottom, divided into minutes, hours, days, weeks, months or years (chosen automatically unless `TimeUnit` is set). Each series has its own colour and caption, shown in the legend beneath the plot, and an optional `Title` sits above it. The graph repaints whenever it is resized, whenever the series collection changes, and whenever a series or one of its points changes.

**Demo:** `src/MosaicWpfDemo/Views/Examples/LineGraphExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `ObservableCollection<LineGraphSeries> Series` | Gets or sets the collection of series (lines) to plot. The graph subscribes to the collection and to each series, repainting when either changes. |
| `string? Title` | Gets or sets the title shown centred above the plot. When null or blank the title row collapses and the plot reclaims the space. |
| `double TitleFontSize` | Gets or sets the font size of the `Title`. |
| `LineGraphTimeUnit TimeUnit` | Gets or sets the unit the X-axis is divided into. `LineGraphTimeUnit.Auto` picks the unit that best fits the span of time covered by the plotted points. |
| `string? TimeLabelFormat` | Gets or sets a `DateTime` format string for the X-axis labels. When null a format that suits the effective `TimeUnit` is used (for example "HH:mm" for minutes and "yyyy" for years). |
| `string? ValueLabelFormat` | Gets or sets a numeric format string for the Y-axis labels and point tooltips. When null the number of decimal places is derived from the axis step so labels such as 0.25 and 1,000 both read cleanly. |
| `int IntervalCount` | Gets or sets the approximate number of intervals on the Y-axis. The axis is snapped to round numbers, so the actual count can differ by one or two. |
| `double? MinimumValue` | Gets or sets a fixed lower bound for the Y-axis. When null the axis starts at zero, or at the smallest plotted value when any value is negative. |
| `double? MaximumValue` | Gets or sets a fixed upper bound for the Y-axis. When null the axis ends at the largest plotted value, rounded up to a round number. |
| `double LineThickness` | Gets or sets the thickness of each plotted line. A series can override this with `LineGraphSeries.LineThickness`. |
| `double PointRadius` | Gets or sets the radius of the marker drawn at each point when `ShowPoints` is true. |
| `bool ShowPoints` | Gets or sets whether a marker is drawn at each point. Markers are what receive `PointClicked`, so hiding them also disables point clicks. |
| `bool ShowGridLines` | Gets or sets whether horizontal grid lines are drawn across the plot at each Y-axis interval. |
| `bool ShowLegend` | Gets or sets whether the legend of series captions is shown beneath the plot. |
| `Brush StrokeBrush` | Gets or sets the brush used to draw the axes and grid lines. Defaults to the Mosaic theme control border brush. |
| `double StrokeThickness` | Gets or sets the thickness of the axes and grid lines. |
| `Brush AxisForeground` | Gets or sets the brush used for the axis labels. Defaults to the Mosaic theme secondary text brush so the labels sit back from the plotted lines. |
| `ICommand? SeriesClickCommand` | Gets or sets a command invoked when a line (or its legend entry) is clicked. The clicked `LineGraphSeries` is passed as the command parameter, which is the MVVM equivalent of handling `SeriesClicked`. |
| `ICommand? PointClickCommand` | Gets or sets a command invoked when a point is clicked. The clicked `LineGraphPoint` is passed as the command parameter, which is the MVVM equivalent of handling `PointClicked`. |
| `event EventHandler<LineGraphSeriesEventArgs> SeriesClicked` | Occurs when a line, or its entry in the legend, is clicked. The `LineGraphSeriesEventArgs.Series` on the event args is the `LineGraphSeries` that backs the clicked line. |
| `event EventHandler<LineGraphPointEventArgs> PointClicked` | Occurs when a point marker is clicked. The event args carry both the `LineGraphPointEventArgs.Point` and the `LineGraphPointEventArgs.Series` it belongs to. A point click does not also raise `SeriesClicked`. |
| `void Redraw(…)` | Clears the plot and repaints the graph at its current size. Changes to the series, their points and the graph's properties repaint automatically, so this rarely needs to be called directly. |


## Usage notes

Populate `Series` with `LineGraphSeries` containing `LineGraphPoint` values. `TimeUnit` controls the time axis; series and point changes trigger redraws.
