# DayCalendarView

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DayCalendarView/DayCalendarView.cs`

Displays one day as a scrollable, continuously positioned timeline of calendar events.

**Demo:** `src/MosaicWpfDemo/Views/Examples/DayCalendarViewExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `DateTime SelectedDate` | Gets or sets the date represented by the timeline. |
| `IEnumerable? ItemsSource` | Gets or sets the event objects displayed by the calendar. |
| `object? SelectedItem` | Gets or sets the selected source event object. |
| `DataTemplate? EventTemplate` | Gets or sets the optional template whose data context is the original source event object. |
| `ICommand? EventCommand` | Gets or sets the command executed with the original source event when an event is activated. |
| `ICommand? EventTimeChangedCommand` | Gets or sets the command executed with a `CalendarEventTimeChangedEventArgs` drag proposal. |
| `ICommand? EventDeletingCommand` | Gets or sets the command executed with a `CalendarEventDeletingEventArgs` delete proposal. |
| `double HourHeight` | Gets or sets the number of device-independent pixels used for one hour. |
| `double TimeColumnWidth` | Gets or sets the width of the time-label column. |
| `double EventSpacing` | Gets or sets the gap between adjacent event columns. |
| `Brush EventBackground` | Gets or sets the default event-card background. |
| `Brush EventBorderBrush` | Gets or sets the default event-card border brush. |
| `Thickness EventBorderThickness` | Gets or sets the default event-card border thickness. |
| `CornerRadius EventCornerRadius` | Gets or sets the default event-card corner radius. |
| `Thickness EventPadding` | Gets or sets the default event-card content padding. |
| `bool ShowQuarterHourLines` | Gets or sets a value that indicates whether 15-minute grid lines are shown. |
| `bool ShowCurrentTimeIndicator` | Gets or sets a value that indicates whether today's current-time line is shown. |
| `TimeSpan DragSnapInterval` | Gets or sets the interval used to snap drag proposals. |
| `bool AllowEventDragging` | Gets or sets a value that indicates whether source events can be dragged. |
| `bool AllowCrossDayEvents` | Gets or sets a value that indicates whether a dragged event may end after the displayed day. |
| `TimeSpan InitialScrollTime` | Gets or sets the time shown near the top when the calendar initially loads. |
| `bool ScrollToInitialTimeOnLoad` | Gets or sets a value that indicates whether the initial time is scrolled into view on load. |
| `string StartDatePath` | Gets or sets the property path that resolves each event's start. |
| `string EndDatePath` | Gets or sets the property path that resolves each event's end. |
| `string TitlePath` | Gets or sets the property path that resolves each event's title. |
| `string DescriptionPath` | Gets or sets the property path that resolves each event's optional description. |
| `string BackgroundPath` | Gets or sets the property path that resolves each event's optional background brush. |
| `string IsReadOnlyPath` | Gets or sets the property path that resolves whether each event cannot be moved by dragging. |
| `string CanDeletePath` | Gets or sets the property path that resolves whether each event can be deleted. |
| `Brush HourLineBrush` | Gets or sets the brush used for hour separators. |
| `Brush QuarterHourLineBrush` | Gets or sets the brush used for quarter-hour separators. |
| `Brush CurrentTimeBrush` | Gets or sets the brush used for the current-time and drag-time guides. |
| `Brush TimeForeground` | Gets or sets the brush used for time labels. |
| `Brush TimeColumnBackground` | Gets or sets the background brush used by the protected time-label column. |
| `event EventHandler<CalendarEventTimeChangedEventArgs> EventTimeChanged` | Occurs when dragging proposes a new time range for an event. |
| `event EventHandler<CalendarEventDeletingEventArgs> EventDeleting` | Occurs before a focused event is deleted, allowing a handler to cancel the removal. |
| `void ScrollToTime(…)` | Scrolls the timeline so the specified time is near its top edge. |
| `void ScrollToCurrentTime(…)` | Scrolls the timeline to the current local time. |
| `bool DeleteEvent(…)` | Raises `EventDeleting` and executes `EventDeletingCommand` for the specified source event, removing it from `ItemsSource` when the proposal is not cancelled. |


## Usage notes

Use `CalendarEvent` items or set the member-path properties for your own model. `EventTemplate` customizes event content; dragging and deletion expose command/event hooks. `SelectedDate` chooses the displayed day.

## Example

```xml
<mosaic:DayCalendarView SelectedDate="{Binding Date}"
                        ItemsSource="{Binding Events}" />
```
