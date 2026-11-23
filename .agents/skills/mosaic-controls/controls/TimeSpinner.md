# TimeSpinner

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/TimeSpinner/TimeSpinner.cs`

A time entry control that chooses a time of day with three coordinated scrolling wheels for hour, minute, and AM or PM rather than with a clock face or a text mask. The closed control shows a compact entry surface with one field per component; clicking it opens a drop down containing the wheels.

**Demo:** `src/MosaicWpfDemo/Views/Examples/TimeSpinnerExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `static RoutedUICommand OpenCommand` | Opens the drop down. Bound to the entry surface by the default template. |
| `static RoutedUICommand CloseCommand` | Closes the drop down without committing the temporary time. |
| `static RoutedUICommand ClearCommand` | Clears `SelectedTime`. Bound to the clear button by the default template. |
| `static RoutedUICommand ApplyCommand` | Commits the temporary time and closes the drop down. Bound to the Apply button by the default template. |
| `static RoutedUICommand CancelCommand` | Restores the time that was selected when the drop down opened and closes it. Bound to the Cancel button by the default template. |
| `TimeOnly? SelectedTime` | The selected time of day, or `null` when the control is unset. Always normalized to whole minutes and always snapped onto the nearest value the wheels offer, which means inside `MinimumTime` to `MaximumTime` and on `MinuteInterval`. |
| `TimeSpan? SelectedTimeSpan` | `SelectedTime` expressed as a `TimeSpan` measured from midnight, for models that store a time of day that way. Writing either property updates the other, so bind one or the other rather than both. |
| `TimeOnly MinimumTime` | The earliest selectable time, inclusive. Defaults to midnight. |
| `TimeOnly MaximumTime` | The latest selectable time, inclusive. Defaults to 11:59 PM. An assignment earlier than `MinimumTime` is coerced up to it rather than throwing, so the invariant MinimumTime &lt;= MaximumTime always holds. |
| `int MinuteInterval` | The step between the minutes the wheel offers. One, the default, offers every minute; fifteen offers 00, 15, 30, and 45. Coerced into 1 through 60. |
| `bool IsDropDownOpen` | Whether the wheel drop down is open. Read only, use `Open` and `Close` or the matching routed commands. |
| `TimeSpinnerCommitMode CommitMode` | When edits made on the wheels are written back to `SelectedTime`. Defaults to `TimeSpinnerCommitMode.Explicit`, matching the accept/dismiss model of the WinUI 3 picker. |
| `TimeSpinnerLightDismissBehavior LightDismissBehavior` | What clicking outside an open drop down does with the temporary time. Defaults to `TimeSpinnerLightDismissBehavior.Cancel`. |
| `bool IsMinuteVisible` | Whether the minute field is shown. Hiding it turns the control into an hour picker and preserves the minute component of the selected or temporary time. The hour and AM/PM fields are always shown, because a time missing either of them cannot be read. |
| `TimeSpinnerMeridiemDisplayMode MeridiemDisplayMode` | How the AM and PM designators are cased. Defaults to whatever the culture publishes. |
| `string? HourFormat` | An optional .NET time format string applied to hour values, for example hh for a padded hour. Defaults to the unpadded number. A format string the runtime cannot honor falls back to that default rather than throwing. |
| `string? MinuteFormat` | An optional .NET time format string applied to minute values. Defaults to two digits. |
| `CultureInfo? Culture` | The culture supplying the AM and PM designators and number formatting. Defaults to `CultureInfo.CurrentUICulture` when `null`. |
| `object? Header` | Optional content rendered above the entry surface. When it is a string it also becomes the control's accessible name. |
| `DataTemplate? HeaderTemplate` | The template used to render `Header`. |
| `string? HourPlaceholderText` | The text shown in the hour field when nothing is selected. Falls back to the `HourPlaceholderTextKey` resource so it can be localized centrally. |
| `string? MinutePlaceholderText` | The text shown in the minute field when nothing is selected. Falls back to the `MinutePlaceholderTextKey` resource. |
| `string? MeridiemPlaceholderText` | The text shown in the AM/PM field when nothing is selected. Falls back to the `MeridiemPlaceholderTextKey` resource. |
| `bool IsClearButtonVisible` | Whether the entry surface shows a clear button. The button is only enabled while a time is selected and the control is editable. |
| `bool IsReadOnly` | Whether the control is read only. The selected time stays visible and the control stays focusable, but the drop down cannot be opened and the value cannot be changed or cleared. Setting this while the drop down is open closes it. |
| `double ItemHeight` | The height of a single entry on the wheels. |
| `int VisibleItemCount` | How many entries each wheel shows at once. Coerced by the wheels to an odd number of at least three. |
| `bool IsScrollAnimationEnabled` | Whether the wheels ease into position using the `InertiaScrollViewer` in their template. Set to `false` for instant movement when motion should be reduced. |
| `IReadOnlyList<DateSpinnerItem>? HourItems` | The entries offered by the hour wheel, ordered 12 then 1 through 11 so that scrolling down moves forward in time. All twelve are always present; a change that only affects which of them are selectable mutates the existing entries in place. |
| `IReadOnlyList<DateSpinnerItem>? MinuteItems` | The entries offered by the minute wheel, sized by `MinuteInterval`. |
| `IReadOnlyList<DateSpinnerItem>? MeridiemItems` | The two entries offered by the AM/PM wheel, carrying the numeric values of `TimeSpinnerMeridiem`. |
| `string HourText` | The hour field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `string MinuteText` | The minute field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `string MeridiemText` | The AM/PM field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `bool HasSelectedTime` | Whether a time is currently selected. The template uses this to render placeholders differently from real values, so the unset state does not rely on color alone. |
| `GridLength MinuteColumnWidth` | The width of the minute column. Collapses to zero when `IsMinuteVisible` is `false` so the remaining fields close the gap instead of leaving a hole. |
| `event EventHandler<TimeSpinnerTimeChangedEventArgs> SelectedTimeChanged` | Occurs when `SelectedTime` changes to a different minute. Assigning a value that normalizes or snaps to the minute already selected raises nothing. |
| `event RoutedEventHandler DropDownOpened` | Occurs after the wheel drop down opens. |
| `event RoutedEventHandler DropDownClosed` | Occurs after the wheel drop down closes, however it was closed. |
| `CultureInfo EffectiveCulture` | The effective culture, which is `Culture` when set and `CultureInfo.CurrentUICulture` otherwise. |
| `void Open(…)` | Opens the wheel drop down. Does nothing when the control is disabled, read only, or already open. |
| `void Close(…)` | Closes the wheel drop down without committing the temporary time. In `TimeSpinnerCommitMode.Explicit` mode the temporary time is discarded; in `TimeSpinnerCommitMode.Immediate` mode the value has already been written through and is kept. |
| `void Apply(…)` | Commits the temporary time to `SelectedTime` and closes the drop down. Does nothing when the drop down is not open. |
| `void Cancel(…)` | Restores the time that was selected when the drop down opened and closes it. Does nothing when the drop down is not open. |
| `void Clear(…)` | Clears the selection by setting `SelectedTime` to `null`. Does nothing when the control is read only or disabled. |


## Usage notes

`SelectedTime` is `TimeOnly?`; `SelectedTimeSpan` provides an alternative binding surface. Configure bounds and `MinuteInterval` along with commit and light-dismiss behavior. The wheel implementation shares `DateSpinnerSelector`.

## Example

```xml
<mosaic:TimeSpinner SelectedTime="{Binding Time, Mode=TwoWay}" />
```
