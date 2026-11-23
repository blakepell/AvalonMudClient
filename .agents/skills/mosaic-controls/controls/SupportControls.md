# Support Controls

These are public WPF controls used mainly by Mosaic native style dictionaries and low-level templates. They are available from `Mosaic.UI.Wpf.Controls`, but most application code should use the higher-level controls in the main inventory.

## WDScrollViewer

**Base class:** `ScrollViewer`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ScrollViewer/WDScrollViewer.cs`

`WDScrollViewer` adds optional animated mouse-wheel scrolling.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsScrollAnimation` | `bool` | `false` | Enables animated wheel scrolling. |

| Method | Description |
|---|---|
| `AnimateScroll(double toValue, Action onCompleted = null)` | Animates vertical scroll offset to `toValue` over 800 ms. |

## SliderRepeatButton

**Base class:** `RepeatButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/SliderRepeatButton/SliderRepeatButton.cs`

`SliderRepeatButton` is used by native slider templates to expose radius/orientation metadata.

| Property | Type | Default | Description |
|---|---|---|---|
| `RadiusOrientation` | `RadiusOrientation` | `null` metadata | Indicates which side or corner should receive radius treatment. |

`RadiusOrientation` values: `Down`, `Up`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomRight`, and `BottomLeft`.

## SystemDropShadowChrome

**Base class:** `Decorator`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/SystemDropShadowChrome/SystemDropShadowChrome.cs`

`SystemDropShadowChrome` renders a WPF-style shadow silhouette around its child content. It is used in native menu, toolbar, and combo box templates.

| Property | Type | Default | Description |
|---|---|---|---|
| `Color` | `Color` | `#71000000` | Shadow color. |
| `CornerRadius` | `CornerRadius` | `0` | Corner radius used when rendering shadow corners. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:WDScrollViewer IsScrollAnimation="True">
    <!-- scrolling content -->
</mosaic:WDScrollViewer>
```

## Notes

- These controls are mostly referenced from `Themes/Native/*.xaml`.
- `WDScrollViewer` is used by the native `TreeView` template.
- `SliderRepeatButton` is used by the native `Slider` template.
- `SystemDropShadowChrome` is used by native popup/chrome templates.

# Additional support controls

## CheckBoxListItem

**Base class:** `ListBoxItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/CheckBoxList/CheckBoxListItem.cs`

The item container used by `CheckBoxList`.

## WDBorder

**Base class:** `Border`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/ClipBorder/WDBorder.cs`

Provides a border that exposes a content clipping geometry.

### API starting points

| Member | Purpose |
|---|---|
| `Geometry ContentClip` | Gets or sets the geometry used to clip the border content. |

## CalendarBox

**Base class:** `ListBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/CalendarBox.cs`

Represents the scrollable grid of day cells displayed inside the `DatePicker` popup.

## CalendarBoxItem

**Base class:** `ListBoxItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/CalendarBoxItem.cs`

Represents a single day cell within a `CalendarBox`.

### API starting points

| Member | Purpose |
|---|---|
| `string? DateFormat` | Gets or sets the date formatted as yyyyMMdd, used as the selection key in `CalendarBox`. |
| `DateTime Date;` | The calendar date this item represents. |
| `bool IsCurrentMonth` | Gets or sets a value that indicates whether this day falls within the month currently shown in the calendar. |

## CalendarSwitch

**Base class:** `System.Windows.Controls.Primitives.ToggleButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/CalendarSwitch.cs`

Represents the toggle button that opens and closes the calendar popup inside a `DatePicker`.

## ChevronButton

**Base class:** `Button`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/ChevronButton.cs`

Represents a navigation button styled with a chevron arrow used to move between months in a `DatePicker`.

## DayOfWeek

**Base class:** `Label`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/DayOfWeek.cs`

Represents a label that displays an abbreviated day-of-week header (such as "Sun", "Mon") in the calendar grid.

## DateSpinnerSelector

**Base class:** `ListBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DateSpinner/DateSpinnerSelector.cs`

A single vertical wheel used by `DateSpinner` to choose a month, a day, or a year. The selected entry sits in a highlighted band in the middle of the control with neighbouring values visible above and below it.

### API starting points

| Member | Purpose |
|---|---|
| `IReadOnlyList<DateSpinnerItem>? Values` | The values the wheel offers, in ascending order. The wheel owns the spacer padding around them, so this collection should contain nothing but real values. |
| `int? SelectedNumber` | The `DateSpinnerItem.Value` of the centred entry, or `null` when the wheel has no selection. Setting a value that the wheel does not offer clears the selection. |
| `double ItemHeight` | The fixed height of a single entry. A uniform height is what makes the wheel's scroll arithmetic exact, so entries are not allowed to size to their content. |
| `int VisibleItemCount` | How many entries are on screen at once. Coerced to an odd number of at least three so that exactly one entry can sit in the centre band with the same number of neighbours above and below it. |
| `double ViewportHeight` | `ItemHeight` multiplied by `VisibleItemCount`. The default style binds the wheel's height to this so the viewport always holds a whole number of entries. |
| `bool IsScrollAnimationEnabled` | Whether the wheel eases into its new position using the `InertiaScrollViewer` in its template. Set to `false` to move instantly, which is the appropriate behavior when the user has asked the system to reduce motion. |
| `int ScrollAnimationDuration` | The length of the easing animation in milliseconds. Kept deliberately short, a value wheel should feel responsive rather than decorative. |
| `event EventHandler? ValueChanged;` | Occurs when `SelectedNumber` changes, whether from user interaction or from a programmatic assignment. The owning `DateSpinner` guards against reentrancy on its side. |
| `bool MoveSelection(…)` | Moves the selection by `delta` entries, skipping over values that fall outside the spinner's permitted range and stopping at the ends rather than wrapping. |
| `bool MoveToEnd(…)` | Selects the first or last value the spinner permits. |

## DateSpinnerSelectorItem

**Base class:** `ListBoxItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DateSpinner/DateSpinnerSelector.cs`

The container generated for each entry in a `DateSpinnerSelector`. It exists so the wheel's entries can be styled independently of ordinary `ListBoxItem` instances.

## CalendarEventPresenter

**Base class:** `ContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DayCalendarView/CalendarEventPresenter.cs`

Presents one source item inside a `DayCalendarView` timeline.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsSelected` | Gets a value that indicates whether this event is the calendar's selected item. |
| `bool IsReadOnly` | Gets a value that indicates whether this event cannot be moved by dragging. |
| `bool CanDelete` | Gets a value that indicates whether this event may be deleted from the calendar. |
| `string DisplayTitle` | Gets or sets the resolved title displayed by the default event template. |
| `string DisplayDescription` | Gets or sets the resolved description displayed by the default event template. |
| `string DisplayTimeText` | Gets or sets the resolved time range displayed by the default event template. |
| `CornerRadius CornerRadius` | Gets or sets the corner radius of the event card. |

## DayTimelinePanel

**Base class:** `Panel`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DayCalendarView/DayTimelinePanel.cs`

Arranges calendar-event presenters using continuous time coordinates and draws a 24-hour timeline.

### API starting points

| Member | Purpose |
|---|---|
| `double HourHeight` | Gets or sets the vertical size allocated to each hour. |
| `double TimeColumnWidth` | Gets or sets the width reserved for time labels. |
| `double EventSpacing` | Gets or sets the gap between adjacent event columns. |
| `bool ShowQuarterHourLines` | Gets or sets a value that indicates whether 15-minute subdivisions are drawn. |
| `Brush HourLineBrush` | Gets or sets the brush used for hour separators. |
| `Brush QuarterHourLineBrush` | Gets or sets the brush used for quarter-hour separators. |
| `Brush TimeForeground` | Gets or sets the brush used for timeline labels. |
| `Brush CurrentTimeBrush` | Gets or sets the brush used for current-time and drag-time guides. |
| `Brush TimeColumnBackground` | Gets or sets the background brush used by the time-label well. |
| `FontFamily FontFamily` | Gets or sets the font family used for time labels and drag indicators. |
| `double FontSize` | Gets or sets the font size used for time labels and drag indicators. |

## DocumentTabItem

**Base class:** `System.Windows.Controls.TabItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/DocumentContainer/DocumentTabItem.cs`

Represents the tab container generated for a `Document`.

### API starting points

| Member | Purpose |
|---|---|
| `Document? Document` | Gets or sets the document represented by this tab item. |
| `bool CanClose` | Gets or sets a value that indicates whether the close button is displayed. |

## HorizontalListBoxItem

**Base class:** `ListBoxItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/HorizontalListBox/HorizontalListBoxItem.cs`

The item container used by `HorizontalListBox`. Renders as a single cell whose fill color communicates whether it is toggled on or off.

### API starting points

| Member | Purpose |
|---|---|
| `CornerRadius CornerRadius` | The corner radius of the cell. The default style binds this to the owning `HorizontalListBox.ItemCornerRadius`. |

## Arc

**Base class:** `Shape`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/RadialProgressBar/Arc.cs`

A `Shape` that draws a circular arc, pie, or ring of shapes between a start and end angle. It is the rendering primitive used by `RadialProgressBar`.

### API starting points

| Member | Purpose |
|---|---|
| `double ShapeRotationAdjustment` | Gets or sets additional shape rotation adjustment in degrees for Shape mode. Default value is 0d. |
| `bool ShapeModeUseFade` | Gets or sets shape fading effect for Shape mode. Default value is True. |
| `ArcShape ShapeModeShape` | Gets or sets default shape for Shape mode. Default value is Rectangle. |
| `double ShapeModeWidth` | Gets or sets shape width for Shape mode. Default value is 1d. |
| `int ShapeModeStep` | Gets or sets step in degrees for Shape mode. Degree range is 0 - 360. Default step is 3. |
| `ArcMode ArcMode` | Gets or sets the mode of the progress bar |
| `Thickness ProgressBorderThickness` | Gets or sets the thickness of the fill border |
| `Brush ProgressFillBrush` | Gets or sets the brush for the fill part |
| `Brush ProgressBackgroundBrush` | Gets or sets the brush used to paint the background track behind the fill. |
| `Brush ProgressBorderBrush` | Gets or sets the fill border brush. |
| `double StartAngle` | Gets or sets the angle, in degrees, at which the arc fill begins. |
| `double EndAngle` | Gets or sets the angle, in degrees, at which the arc fill ends. |
| `SweepDirection Direction` | Gets or sets progress bar fill direction |
| `double OriginRotationDegrees` | Gets or sets the initial rotation of the arc a certain number of degree in the direction. 270 - clockwise, 90 - counterclockwise. |
| `bool IsIndeterminate` | Gets or sets the Indeterminate animation state |
| `double IndeterminateSpeedRatio` | Gets or sets speed ration for Indeterminate state animation. Default value is 1. |
| `double ArcBackgroundWidth` | Gets or sets progress bar background circle width. Default value is 0 - auto size. |
| `Point StartPoint;` | The point at which the shape is drawn. |
| `double Angle;` | The rotation, in degrees, applied to the shape. |

## SideMenuItemControl

**Base class:** `UserControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/SideMenu/SideMenuItemControl.xaml.cs`

Represents a control for displaying and interacting with a single item in a side menu.

### API starting points

| Member | Purpose |
|---|---|
| `SideMenuItem? MenuItem` | Gets or sets the side menu item associated with this control. |
| `SideMenu? SideMenu` | Gets or sets the `SideMenu` associated with the control. |
| `bool Compact` | Gets or sets a value indicating whether the menu item is displayed in compact mode (icon above text). |

## TabItem

**Base class:** `System.Windows.Controls.TabItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/TabControl/TabControl.cs`

Represents a single tab item within a `System.Windows.Controls.TabControl`.

### API starting points

| Member | Purpose |
|---|---|
| `double HeaderFontSize` | Gets or sets the font size used by the tab header. |
| `FontFamily HeaderFontFamily` | Gets or sets the font family used by the tab header. |
| `FontStyle HeaderFontStyle` | Gets or sets the font style used by the tab header. |
| `FontWeight HeaderFontWeight` | Gets or sets the font weight used by the tab header. |
| `TabActiveIndicatorPlacement ActiveIndicatorPlacement` | Gets or sets the placement of the active indicator within a tab control. |

## WaveformVisualizerBase

**Base class:** `FrameworkElement`\
**Namespace:** `Mosaic.UI.Wpf.Controls.WaveformVisualizer`\
**Source:** `src/Mosaic.UI.Wpf/Controls/WaveformVisualizer/WaveformVisualizerBase.cs`

Provides shared rendering and capture lifecycle behavior for waveform visualizers.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsListening` | Gets or sets a value that indicates whether the visualizer captures audio. |
| `Brush WaveformBrush` | Gets or sets the brush used to draw the waveform. |
| `event EventHandler<Exception>? OnError;` | Occurs when audio capture, device enumeration, or waveform rendering fails. |
