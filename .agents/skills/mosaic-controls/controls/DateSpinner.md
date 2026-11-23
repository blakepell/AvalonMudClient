# DateSpinner

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DateSpinner/DateSpinner.cs`

A date entry control that chooses a date with three coordinated scrolling wheels for month, day, and year rather than with a calendar grid. The closed control shows a compact entry surface with one field per date component; clicking it opens a drop down containing the wheels.

**Demo:** `src/MosaicWpfDemo/Views/Examples/DateSpinnerExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `static RoutedUICommand OpenCommand` | Opens the drop down. Bound to the entry surface by the default template. |
| `static RoutedUICommand CloseCommand` | Closes the drop down without committing the temporary date. |
| `static RoutedUICommand ClearCommand` | Clears `SelectedDate`. Bound to the clear button by the default template. |
| `static RoutedUICommand ApplyCommand` | Commits the temporary date and closes the drop down. Bound to the Apply button by the default template. |
| `static RoutedUICommand CancelCommand` | Restores the date that was selected when the drop down opened and closes it. Bound to the Cancel button by the default template. |
| `DateTime? SelectedDate` | The selected date, or `null` when the control is unset. Always normalized to midnight and always coerced into `MinimumDate` to `MaximumDate`. |
| `DateTime MinimumDate` | The earliest selectable date, inclusive. Defaults to one hundred years before today. |
| `DateTime MaximumDate` | The latest selectable date, inclusive. Defaults to one hundred years after today. An assignment earlier than `MinimumDate` is coerced up to it rather than throwing, so the invariant MinimumDate &lt;= MaximumDate always holds. |
| `bool IsDropDownOpen` | Whether the wheel drop down is open. Read only, use `Open` and `Close` or the matching routed commands. |
| `DateSpinnerCommitMode CommitMode` | When edits made on the wheels are written back to `SelectedDate`. Defaults to `DateSpinnerCommitMode.Explicit`, matching the accept/dismiss model of the WinUI 3 picker. |
| `DateSpinnerLightDismissBehavior LightDismissBehavior` | What clicking outside an open drop down does with the temporary date. Defaults to `DateSpinnerLightDismissBehavior.Cancel`. |
| `bool IsMonthVisible` | Whether the month field is shown. At least one field must remain visible, so an attempt to hide the last visible field is coerced back to `true`. |
| `bool IsDayVisible` | Whether the day field is shown. Hiding it preserves the day component of the selected or temporary date. |
| `bool IsYearVisible` | Whether the year field is shown. Hiding it preserves the year component of the selected or temporary date. |
| `DateSpinnerMonthDisplayMode MonthDisplayMode` | How months are rendered when `MonthFormat` has not been set. Defaults to the culture's full month name. |
| `string? MonthFormat` | An optional .NET date format string applied to month values, for example MMM. Overrides `MonthDisplayMode`. A format string the runtime cannot honor falls back to the full month name rather than throwing. |
| `string? DayFormat` | An optional .NET date format string applied to day values, for example dd. Defaults to the localized number with no padding. |
| `string? YearFormat` | An optional .NET date format string applied to year values, for example yy. Defaults to a four digit year. |
| `CultureInfo? Culture` | The culture supplying month names, number formatting, and the order the fields are laid out in. Defaults to `CultureInfo.CurrentUICulture` when `null`. |
| `object? Header` | Optional content rendered above the entry surface. When it is a string it also becomes the control's accessible name. |
| `DataTemplate? HeaderTemplate` | The template used to render `Header`. |
| `string? MonthPlaceholderText` | The text shown in the month field when nothing is selected. Falls back to the `MonthPlaceholderTextKey` resource so it can be localized centrally. |
| `string? DayPlaceholderText` | The text shown in the day field when nothing is selected. Falls back to the `DayPlaceholderTextKey` resource. |
| `string? YearPlaceholderText` | The text shown in the year field when nothing is selected. Falls back to the `YearPlaceholderTextKey` resource. |
| `bool IsClearButtonVisible` | Whether the entry surface shows a clear button. The button is only enabled while a date is selected and the control is editable. |
| `bool IsReadOnly` | Whether the control is read only. The selected date stays visible and the control stays focusable, but the drop down cannot be opened and the value cannot be changed or cleared. Setting this while the drop down is open closes it. |
| `double ItemHeight` | The height of a single entry on the wheels. |
| `int VisibleItemCount` | How many entries each wheel shows at once. Coerced by the wheels to an odd number of at least three. |
| `bool IsScrollAnimationEnabled` | Whether the wheels ease into position using the `InertiaScrollViewer` in their template. Set to `false` for instant movement when motion should be reduced. |
| `IReadOnlyList<DateSpinnerItem>? MonthItems` | The entries offered by the month wheel. Rebuilt only when the set of months changes; a change that only affects which months are selectable mutates the existing entries in place. |
| `IReadOnlyList<DateSpinnerItem>? DayItems` | The entries offered by the day wheel, sized to the length of the temporary month. |
| `IReadOnlyList<DateSpinnerItem>? YearItems` | The entries offered by the year wheel, covering `MinimumDate` to `MaximumDate`. The wheel virtualizes, so a wide range does not create a container per year. |
| `string MonthText` | The month field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `string DayText` | The day field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `string YearText` | The year field's text on the closed entry surface, or its placeholder when nothing is selected. |
| `bool HasSelectedDate` | Whether a date is currently selected. The template uses this to render placeholders differently from real values, so the unset state does not rely on color alone. |
| `int MonthFieldIndex` | The month field's position, zero based, in the culture's field order. The template binds it to a grid column so the layout follows the culture without any logic in the template. |
| `int DayFieldIndex` | The day field's position, zero based, in the culture's field order. |
| `int YearFieldIndex` | The year field's position, zero based, in the culture's field order. |
| `GridLength FirstFieldColumnWidth` | The width of the first culture-ordered field column. The template collapses it when its field is hidden. |
| `GridLength SecondFieldColumnWidth` | The width of the second culture-ordered field column. The template collapses it when its field is hidden. |
| `GridLength ThirdFieldColumnWidth` | The width of the third culture-ordered field column. The template collapses it when its field is hidden. |
| `Visibility MonthSeparatorVisibility` | Whether the divider drawn to the left of the month field is shown. Collapsed when the month is the first visible field, so the entry surface reads as one control rather than three. |
| `Visibility DaySeparatorVisibility` | Whether the divider drawn to the left of the day field is shown. |
| `Visibility YearSeparatorVisibility` | Whether the divider drawn to the left of the year field is shown. |
| `event EventHandler<DateSpinnerDateChangedEventArgs> SelectedDateChanged` | Occurs when `SelectedDate` changes to a different calendar day. Assigning a value that normalizes or coerces to the day already selected raises nothing. |
| `event RoutedEventHandler DropDownOpened` | Occurs after the wheel drop down opens. |
| `event RoutedEventHandler DropDownClosed` | Occurs after the wheel drop down closes, however it was closed. |
| `CultureInfo EffectiveCulture` | The effective culture, which is `Culture` when set and `CultureInfo.CurrentUICulture` otherwise. |
| `void Open(…)` | Opens the wheel drop down. Does nothing when the control is disabled, read only, or already open. |
| `void Close(…)` | Closes the wheel drop down without committing the temporary date. In `DateSpinnerCommitMode.Explicit` mode the temporary date is discarded; in `DateSpinnerCommitMode.Immediate` mode the value has already been written through and is kept. |
| `void Apply(…)` | Commits the temporary date to `SelectedDate` and closes the drop down. Does nothing when the drop down is not open. |
| `void Cancel(…)` | Restores the date that was selected when the drop down opened and closes it. Does nothing when the drop down is not open. |
| `void Clear(…)` | Clears the selection by setting `SelectedDate` to `null`. Does nothing when the control is read only or disabled. |


## Example

```xml
<mosaic:DateSpinner SelectedDate="{Binding Date, Mode=TwoWay}" />
```
