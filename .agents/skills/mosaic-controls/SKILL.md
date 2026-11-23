---
name: mosaic-controls
description: "Overview of all Mosaic UI for WPF controls. Use when: implementing a control, looking up properties/events, asking what controls exist, choosing the right control for a UI requirement."
argument-hint: "Optional: name of a specific control to focus on (e.g. 'ToggleSwitch', 'AutoCompleteBox')"
---

# Mosaic UI for WPF — Control Overview

Mosaic UI for WPF (`Mosaic.UI.Wpf`) is a lookless, theme-aware WPF control library. Controls are consumed via the canonical XML namespace:

```xml
xmlns:mosaic="http://schemas.apexgate.net/wpf/mosaic-ui"
```

or the CLR form:

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"
```

`VT52Terminal` is mapped to the canonical URI. For a CLR namespace declaration, use:
```xml
xmlns:vt52="clr-namespace:Mosaic.UI.Wpf.Controls.VT52Terminal;assembly=Mosaic.UI.Wpf"
```

The waveform visualizer controls are also mapped to the canonical URI, or can be referenced with:
```xml
xmlns:waveform="clr-namespace:Mosaic.UI.Wpf.Controls.WaveformVisualizer;assembly=Mosaic.UI.Wpf"
```

AvalonDock is shipped as a separate package/project and uses its own XAML URI:
```xml
xmlns:ad="https://github.com/blakepell/MosaicUIWpf"
```

For theme tokens (brushes, font sizes, etc.), declare:
```xml
xmlns:themes="http://schemas.apexgate.net/wpf/mosaic-ui"
```
and reference tokens like `{DynamicResource {x:Static themes:MosaicTheme.ControlTextForegroundBrush}}`.

---

## Control Inventory

For detailed property/event/example documentation, read the individual file listed in the **Details** column. The tables cover all public UI types under `src/Mosaic.UI.Wpf/Controls`, including companion containers, template primitives, and the abstract waveform base. Related APIs include `MessageBox`, `ToastManager`, and the separate AvalonDock integration.

Use these pages to choose a control, then verify exact signatures, defaults, and behavior in the listed source file. `src/Mosaic.UI.Wpf/AssemblyInfo.cs` is authoritative for XAML namespace mappings. Read `docs/ARCHITECTURE_BLUEPRINT.md` before non-trivial library changes.

| Control | Base Class | Category | Description | Details |
|---|---|---|---|---|
| `AccentButton` | `Button` | Input | Button with built-in accent color variants | [AccentButton.md](controls/AccentButton.md) |
| `AdaptiveImage` | `Image` | Media | Pixel-level HSL theme adaptation for images (light/dark mode) | [AdaptiveImage.md](controls/AdaptiveImage.md) |
| `AsteriskTextBlock` | `Control` | Display | Displays masked text with a configurable mask character (default: `●`) | [AsteriskTextBlock.md](controls/AsteriskTextBlock.md) |
| `AudioPlayer` | `Control` | Media | Audio transport control with playlist, seek slider, playback events, and MediaPlayer backing | [AudioPlayer.md](controls/AudioPlayer.md) |
| `AudioPlayerSpectrumAnalyzer` | `FrameworkElement` | Media | WASAPI loopback spectrum analyzer intended to pair with AudioPlayer | [AudioPlayer.md](controls/AudioPlayer.md) |
| `AutoCompleteBox` | `Selector` | Input | Searchable autocomplete combo box with async provider support | [AutoCompleteBox.md](controls/AutoCompleteBox.md) |
| `Avatar` | `Button` | Display | User avatar with image source or fallback initials | [Avatar.md](controls/Avatar.md) |
| `Badge` | `ContentControl` | Display | Small inline status/count badge | [Badge.md](controls/Badge.md) |
| `BindablePasswordBox` | `ContentControl` | Input | Password field with a bindable `Password` string property | [BindablePasswordBox.md](controls/BindablePasswordBox.md) |
| `ChatThread` | `UserControl` | Data | Chat conversation view with sent/received message templates | [ChatThread.md](controls/ChatThread.md) |
| `CheckBoxList` | `ListBox` | Input | Multi-select list that shows checkboxes beside each item | [CheckBoxList.md](controls/CheckBoxList.md) |
| `ClipBorder` | `Border` | Layout | Border that clips child content to its `CornerRadius` | [ClipBorder.md](controls/ClipBorder.md) |
| `ColorPicker` | `UserControl` | Input | Visual color picker with hex input and preset colors | [ColorPicker.md](controls/ColorPicker.md) |
| `ColumnChart` | `UserControl` | Data | Responsive column chart with observable items and click commands | [ColumnChart.md](controls/ColumnChart.md) |
| `CommandBox` | `TextEditor` | Input | AvalonEdit command input with command history, prompt, and block caret | [CommandBox.md](controls/CommandBox.md) |
| `ContentPanel` | `ContentControl` | Layout | Themed content container with header, footer, separator, and corner styling | [ContentPanel.md](controls/ContentPanel.md) |
| `CopyTextBox` | `Control` | Input | Text field with copy button and optional toast feedback | [CopyTextBox.md](controls/CopyTextBox.md) |
| `DatePicker` | `Control` | Input | Mosaic date picker with popup month calendar and nullable selected date | [DatePicker.md](controls/DatePicker.md) |
| `DateSpinner` | `Control` | Input | Nullable date selection with scrolling month/day/year wheels | [DateSpinner.md](controls/DateSpinner.md) |
| `DayCalendarView` | `Control` | Data | Day timeline with event selection, dragging, and deletion | [DayCalendarView.md](controls/DayCalendarView.md) |
| `DocumentContainer` | `TabControl` | Navigation | Closable, reorderable document tabs with active-document tracking | [DocumentContainer.md](controls/DocumentContainer.md) |
| `EditableTextBlock` | `Control` | Input | Inline editable text that toggles between view/edit mode | [EditableTextBlock.md](controls/EditableTextBlock.md) |
| `EnumComboBox` | `ComboBox` | Input | Enum member selection with display-name support | [EnumComboBox.md](controls/EnumComboBox.md) |
| `ExecutionControl` | `Control` | Input | Play, pause, and stop buttons with independent commands | [ExecutionControl.md](controls/ExecutionControl.md) |
| `FavoriteCheckBox` | `CheckBox` | Input | Symbol-based favorite/star toggle with configurable brushes | [FavoriteCheckBox.md](controls/FavoriteCheckBox.md) |
| `FileCard` | `Control` | Display | File card with shell icon, size, command, and optional opening | [FileCard.md](controls/FileCard.md) |
| `FileDropper` | `Control` | Input | File drag-and-drop target with file type validation and command/event hooks | [FileDropper.md](controls/FileDropper.md) |
| `Files` | `Control` | Data | Directory file list with shell icons, sorting, selection, watcher refresh, and activation event | [Files.md](controls/Files.md) |
| `FlipPanel` | `ContentControl` | Display | Animated flip panel with front and back content faces | [FlipPanel.md](controls/FlipPanel.md) |
| `FontAutoCompleteBox` | `AutoCompleteBox` | Input | Installed-font autocomplete with font previews | [FontControls.md](controls/FontControls.md) |
| `FontComboBox` | `ComboBox` | Input | Installed-font selector with font previews | [FontControls.md](controls/FontControls.md) |
| `FontStyleComboBox` | `ComboBox` | Input | Font style selector with style previews | [FontControls.md](controls/FontControls.md) |
| `FontWeightComboBox` | `ComboBox` | Input | Font weight selector with weight previews | [FontControls.md](controls/FontControls.md) |
| `GravatarImage` | `Image` | Media | Loads and displays a Gravatar avatar from an email address | [GravatarImage.md](controls/GravatarImage.md) |
| `HexColorTextBox` | `ComboBox` | Input | Editable hex color field (`#RGB`, `#RRGGBB`, `#AARRGGBB`) with shade drop-down | [HexColorTextBox.md](controls/HexColorTextBox.md) |
| `HorizontalListBox` | `ListBox` | Input | Horizontal selectable cells with configurable spacing and selection brushes | [HorizontalListBox.md](controls/HorizontalListBox.md) |
| `Hyperlink` | `ContentControl` | Navigation | Clickable link that opens a URI or executes an `ICommand` | [Hyperlink.md](controls/Hyperlink.md) |
| `ImageViewer` | `Control` | Media | Image viewer with zoom, pan, rotation, save, and clipboard commands | [ImageViewer.md](controls/ImageViewer.md) |
| `InertiaScrollViewer` | `ScrollViewer` | Layout | ScrollViewer with animated inertia/momentum on mouse wheel | [InertiaScrollViewer.md](controls/InertiaScrollViewer.md) |
| `InfoBubble` | `ContentControl` | Display | Content wrapper with an overlaid count, status, or dot indicator | [InfoBubble.md](controls/InfoBubble.md) |
| `InfoCard` | `ContentControl` | Display | Card with animated accent bar, title, header, body, and footer | [InfoCard.md](controls/InfoCard.md) |
| `InputWaveformVisualizer` | `WaveformVisualizerBase` | Media | Live waveform from a selectable Windows audio input device | [WaveformVisualizer.md](controls/WaveformVisualizer.md) |
| `IPv4TextBox` | `Control` | Input | Segmented IPv4 address editor with copy command | [IPv4TextBox.md](controls/IPv4TextBox.md) |
| `LabeledSeparator` | `ContentControl` | Display | Horizontal separator with an embedded text label | [LabeledSeparator.md](controls/LabeledSeparator.md) |
| `LineGraph` | `Control` | Data | Observable time-series line graph with axes, legend, and click commands | [LineGraph.md](controls/LineGraph.md) |
| `LoopbackWaveformVisualizer` | `WaveformVisualizerBase` | Media | Live waveform from the default Windows audio render device via WASAPI loopback | [WaveformVisualizer.md](controls/WaveformVisualizer.md) |
| `MarkdownEditor` | `UserControl` | Input | Markdown editor built on SyntaxEditor with toolbar, snippets, save helpers, and preview/copy actions | [MarkdownEditor.md](controls/MarkdownEditor.md) |
| `MarkdownViewer` | `Control` | Display | WPF-native Markdown renderer backed by a copyable FlowDocument/RichTextBox | [MarkdownViewer.md](controls/MarkdownViewer.md) |
| `MessageBox` | static class | Dialog | Themed drop-in replacement for System.Windows.MessageBox Show overloads | [MessageBox.md](controls/MessageBox.md) |
| `ModalDialog` | `ContentControl` | Dialog | Awaitable modal content overlay with dimmed and blurred host | [ModalDialog.md](controls/ModalDialog.md) |
| `NumericTextBox` | `TextBox` | Input | Text box that accepts only numeric input with configurable decimal places | [NumericTextBox.md](controls/NumericTextBox.md) |
| `PieChart` | `UserControl` | Data | Observable pie chart with legend and slice click commands | [PieChart.md](controls/PieChart.md) |
| `ProgressRing` | `Control` | Display | Indeterminate animated ring/spinner controlled by IsActive | [ProgressRing.md](controls/ProgressRing.md) |
| `PropertyGrid` | `Control` | Data | Object property inspector using `TypeDescriptor` with category grouping | [PropertyGrid.md](controls/PropertyGrid.md) |
| `PullToRefreshWrapper` | `ContentControl` | Layout | Mouse and touch pull-to-refresh wrapper for scrollable content | [PullToRefreshWrapper.md](controls/PullToRefreshWrapper.md) |
| `RadialColorPicker` | `Control` | Input | HSV wheel color picker with hex and RGBA editing | [RadialColorPicker.md](controls/RadialColorPicker.md) |
| `RadialColorPickerBox` | `Control` | Input | Compact color field with popup HSV picker | [RadialColorPickerBox.md](controls/RadialColorPickerBox.md) |
| `RadialProgressBar` | `ProgressBar` | Display | Circular progress bar with fill, pie, shape, and indeterminate modes | [RadialProgressBar.md](controls/RadialProgressBar.md) |
| `RelativePanel` | `Panel` | Layout | Arranges children relative to each other via attached properties | [RelativePanel.md](controls/RelativePanel.md) |
| `ScalingTextBlock` | `TextBlock` | Display | TextBlock that automatically lowers font size to fit available width | [ScalingTextBlock.md](controls/ScalingTextBlock.md) |
| `ScriptEditorControl` | `Control` | Input | Executable JavaScript editor with completion, run/stop, and save support | [ScriptEditorControl.md](controls/ScriptEditorControl.md) |
| `SearchBox` | `TextBox` | Input | Text box with watermark, clear button, and `SearchExecuted` event | [SearchBox.md](controls/SearchBox.md) |
| `SettingsItem` | `ContentControl` | Display | Settings row with icon, title, description, and slot for a control | [SettingsItem.md](controls/SettingsItem.md) |
| `ShadowPanel` | `ContentControl` | Layout | Content container with configurable drop shadow effect | [ShadowPanel.md](controls/ShadowPanel.md) |
| `ShadowTextBlock` | `TextBlock` | Display | TextBlock with automatically applied drop shadow | [ShadowTextBlock.md](controls/ShadowTextBlock.md) |
| `Shield` | `Control` | Display | GitHub-style shield/badge with label and value sections | [Shield.md](controls/Shield.md) |
| `SideMenu` | `UserControl` | Navigation | Collapsible navigation side menu with search and drag-drop reorder | [SideMenu.md](controls/SideMenu.md) |
| `SimpleStackPanel` | `Panel` | Layout | Efficient stack panel with uniform `Spacing` between children | [SimpleStackPanel.md](controls/SimpleStackPanel.md) |
| `SmallPanel` | `Panel` | Layout | Overlay panel that stacks children in the same bounds | [SmallPanel.md](controls/SmallPanel.md) |
| `SplitButton` | `ContentControl` | Input | Button with a separate drop-down chevron that opens a `ContextMenu` | [SplitButton.md](controls/SplitButton.md) |
| `SplitPanel` | `Control` | Layout | Two-pane resizable container with a draggable GridSplitter and bindable split ratio | [SplitPanel.md](controls/SplitPanel.md) |
| `SqliteQueryControl` | `Control` | Data | SQLite explorer, SQL editor, results grid, and export workbench | [SqliteQueryControl.md](controls/SqliteQueryControl.md) |
| `StopwatchDisplay` | `ContentControl` | Display | Real-time stopwatch with Start/Stop/Reset methods | [StopwatchDisplay.md](controls/StopwatchDisplay.md) |
| `StretchWindowButton` | `UserControl` | Windowing | Title-bar button that resizes and centers its host window | [StretchWindowButton.md](controls/StretchWindowButton.md) |
| `StringListEditor` | `ContentControl` | Input | Editable list of strings with add/remove and duplicate/validation support | [StringListEditor.md](controls/StringListEditor.md) |
| `SymbolRating` | `Control` | Input | Star/symbol rating control with hover preview and deselect | [SymbolRating.md](controls/SymbolRating.md) |
| `SyntaxEditor` | `TextEditor` | Input | AvalonEdit-based code editor with Mosaic themes, bundled syntax highlighting, search, JSON commands, and line editing commands | [SyntaxEditor.md](controls/SyntaxEditor.md) |
| `TabControl` | `TabControl` | Navigation | Mosaic-themed tab control with top/bottom active indicator | [TabControl.md](controls/TabControl.md) |
| `TableSizePicker` | `Control` | Input | Grid-based table row/column count selection with preview | [TableSizePicker.md](controls/TableSizePicker.md) |
| `TagBox` | `Control` | Input | Token/tag entry box with removable chips, duplicate control, and cancellable change events | [TagBox.md](controls/TagBox.md) |
| `ThemedTextBlock` | `TextBlock` | Display | Text foreground chosen from background luminance or palette | [ThemedTextBlock.md](controls/ThemedTextBlock.md) |
| `TimeSpinner` | `Control` | Input | Nullable time selection with scrolling hour/minute/meridiem wheels | [TimeSpinner.md](controls/TimeSpinner.md) |
| `ToastManager` | class | Dialog | Hosts and stacks toast notifications over an adorner surface | [ToastMessage.md](controls/ToastMessage.md) |
| `ToastMessage` | `UserControl` | Dialog | Notification card with severity, duration, and dismissal | [ToastMessage.md](controls/ToastMessage.md) |
| `ToggleButton` | `ToggleButton` | Input | Mosaic-themed toggle button with checked/unchecked background | [ToggleButton.md](controls/ToggleButton.md) |
| `ToggleSwitch` | `Control` | Input | Mobile-style on/off toggle switch with custom colors and MVVM | [ToggleSwitch.md](controls/ToggleSwitch.md) |
| `TrayIcon` | `Freezable` | Windowing | Windows notification-area icon with WPF context menu and commands | [TrayIcon.md](controls/TrayIcon.md) |
| `TwoPaneView` | `Control` | Layout | Adaptive two-pane layout (side-by-side or stacked) | [TwoPaneView.md](controls/TwoPaneView.md) |
| `TypingProgress` | `UserControl` | Display | Animated typing indicator for chat interfaces | [TypingProgress.md](controls/TypingProgress.md) |
| `ValidationSummaryPanel` | `ItemsControl` | Data | Validation error collector and display panel with auto-hide | [ValidationSummaryPanel.md](controls/ValidationSummaryPanel.md) |
| `VersionTextBlock` | `TextBlock` | Display | Displays version string from a configurable assembly source | [VersionTextBlock.md](controls/VersionTextBlock.md) |
| `VisualCaptureButton` | `AccentButton` | Display | Captures a target visual to the clipboard or an image file | [VisualCaptureButton.md](controls/VisualCaptureButton.md) |
| `VT52Terminal` | `TextEditor` | Terminal | Full ANSI/VT100/VT220/xterm terminal emulator (AvalonEdit-based) | [VT52Terminal.md](controls/VT52Terminal.md) |
| `Webcam` | `Control` | Media | Media Foundation webcam preview with device/mode selection and snapshots | [Webcam.md](controls/Webcam.md) |
| `WindowTitleBar` | `UserControl` | Windowing | Custom Mosaic window title bar with min/max/close and custom content slots | [WindowTitleBar.md](controls/WindowTitleBar.md) |

---

## Behaviors

Behaviors live in the `Mosaic.UI.Wpf.Behaviors` namespace and add functionality to **existing** WPF controls without subclassing them — reach for one before authoring a new control. Most are `Behavior<T>` (attached inside `<i:Interaction.Behaviors>`); `GridViewSortBehavior`, `WindowChromeBehavior`, and `BrushModifier` are attached-property helpers (applied as `behaviors:Foo.Bar="..."`). The namespace is also mapped to the canonical Mosaic URI, so `mosaic:`-prefixed usage works too. See [Behaviors.md](controls/Behaviors.md) for full property/usage details.

```xml
xmlns:behaviors="clr-namespace:Mosaic.UI.Wpf.Behaviors;assembly=Mosaic.UI.Wpf"
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
```

| Behavior | Attaches to | Description | Details |
|---|---|---|---|
| `FocusBehavior` | `Control` | Focuses the control when it loads | [Behaviors.md](controls/Behaviors.md) |
| `BlinkingBehavior` | `FrameworkElement` | Blinks an element by animating its opacity | [Behaviors.md](controls/Behaviors.md) |
| `FrameworkElementZoomFontSizeOnMouseWheelBehavior` | `FrameworkElement` | Ctrl+MouseWheel font-size zoom (clamped min/max) | [Behaviors.md](controls/Behaviors.md) |
| `BrushModifier` | `Control`/`Border`/`Panel` | Lightens/darkens the background brush by an HSL percentage | [Behaviors.md](controls/Behaviors.md) |
| `TextBoxCopyBehavior` | `Button` | Copies a target `TextBox`'s (selected) text to the clipboard on click | [Behaviors.md](controls/Behaviors.md) |
| `TextBoxClearTextOnEscapeBehavior` | `TextBoxBase` | Clears the text box when Escape is pressed | [Behaviors.md](controls/Behaviors.md) |
| `BlockCaretBehavior` | `TextBox` | Terminal-style full-width block caret | [Behaviors.md](controls/Behaviors.md) |
| `ItemsControlAutoScrollBehavior` | `ItemsControl` | Auto-scrolls to the newest item when the collection changes | [Behaviors.md](controls/Behaviors.md) |
| `ItemsControlFilterBehavior` | `TextBox` | Debounced text filter over a target `ItemsControl` | [Behaviors.md](controls/Behaviors.md) |
| `DataGridFilterBehavior` | `TextBox` | Debounced full-text filter over a target `DataGrid` | [Behaviors.md](controls/Behaviors.md) |
| `DataGridLastColumnFillBehavior` | `DataGrid` | Stretches the last column to fill remaining width | [Behaviors.md](controls/Behaviors.md) |
| `GridViewLastColumnFillBehavior` | `ListView` | Auto-sizes the last `GridView` column | [Behaviors.md](controls/Behaviors.md) |
| `GridViewSortBehavior` | `ListView`/`GridViewColumn` | Click-header sorting for a `GridView` | [Behaviors.md](controls/Behaviors.md) |
| `ListViewDeleteBehavior` | `ListView` | Deletes selected items on the Delete key (optional confirm) | [Behaviors.md](controls/Behaviors.md) |
| `OpenContextMenuBehavior` | `ButtonBase` | Opens the button's `ContextMenu` on click | [Behaviors.md](controls/Behaviors.md) |
| `OpenWindowBehavior` | `FrameworkElement` | Opens a `Window` of a given type on click (modal/modeless) | [Behaviors.md](controls/Behaviors.md) |
| `CloseWindowOnEscapeBehavior` | `Window` | Closes the window on Escape | [Behaviors.md](controls/Behaviors.md) |
| `WindowChromeBehavior` | `Window` | Custom borderless chrome, themed brushes, rounded corners | [Behaviors.md](controls/Behaviors.md) |
| `AvalonTextEditorBindingBehavior` | AvalonEdit `TextEditor` | Bindable text, selection, selected text, and caret offset | [AvalonEditBehaviors.md](controls/AvalonEditBehaviors.md) |
| `AvalonEditPropertiesBehavior` | AvalonEdit `TextEditor` | Binds caret brush and hyperlink enablement | [AvalonEditBehaviors.md](controls/AvalonEditBehaviors.md) |
| `AvalonEditVtTerminalBehavior` | AvalonEdit `TextEditor` | Applies a retro VT/CRT visual skin | [AvalonEditBehaviors.md](controls/AvalonEditBehaviors.md) |
| `AvalonEditCopyBehavior` | `Button` | Copies a target `TextEditor`'s (selected) text to the clipboard on click | [AvalonEditBehaviors.md](controls/AvalonEditBehaviors.md) |

| `AvalonEditBlockCaretBehavior` | `TextEditor` | Attaches to an AvalonEdit `TextEditor` and swaps the default thin line caret for a solid block caret. The `IsEnabled` dependency property controls which caret style is shown and can be toggled at runtime (including via data binding). The character sitting underneath the block is re-drawn on top of it in a contrasting color so that it stays legible. | [AvalonEditBehaviors.md](controls/AvalonEditBehaviors.md) |

| `ImageMouseWheelZoomBehavior` | `Image` | Adds mouse-wheel zooming to an `Image` by applying a render scale transform. | [Behaviors.md](controls/Behaviors.md) |

| `OpenFolderBehavior` | `FrameworkElement` | Behavior to open a folder in Windows Explorer when attached to a ButtonBase or MenuItem. | [Behaviors.md](controls/Behaviors.md) |

| `VisualCaptureContextMenuBehavior` | `FrameworkElement` | Adds "Copy as Image" and "Save as Image..." items to the context menu of the element the behavior is attached to. When the element has no context menu one is created; when it already has one the items are appended after a separator. By default the attached element itself is captured, but a different element can be chosen with `Target` or `TargetName`. Scrolling elements are captured at their full scroll extent. | [Behaviors.md](controls/Behaviors.md) |

| `WindowCloseConfirmationBehavior` | `Window` | A behavior that prompts the user with a confirmation message when attempting to close a window. | [Behaviors.md](controls/Behaviors.md) |

> **File name ≠ class name** for four behaviors: `AvalonEditBindingBehavior.cs` → `AvalonTextEditorBindingBehavior`, `TextBoxClearOnEscapeBehavior.cs` → `TextBoxClearTextOnEscapeBehavior`, `CloseWindowOnEscape.cs` → `CloseWindowOnEscapeBehavior`, `ButtonOpenContextMenu.cs` → `OpenContextMenuBehavior`. Use the class name in XAML.

## Integration Packages

These are not main-assembly controls, but are part of the Mosaic usage surface and have demo examples.

| Item | Kind | Namespace / URI | Description | Details |
|---|---|---|---|---|
| `DockingManager` / `MosaicTheme` | Separate package/project | `https://github.com/blakepell/MosaicUIWpf` | Mosaic-themed AvalonDock fork/integration for IDE-style document and tool-window docking | [AvalonDock.md](controls/AvalonDock.md) |

## Ribbon family

Ribbon belongs to the main assembly and uses the canonical `mosaic:` URI. Read [Ribbon.md](controls/Ribbon.md) for tabs, groups, command controls, backstage, quick access, key tips, and template primitives.

| Control / component | Base Class | Description | Details |
|---|---|---|---|
| `Ribbon` | `Selector` | A Microsoft Office–style command Ribbon. The Ribbon coordinates tab selection, the effective layout mode, minimization, size collapse, contextual tabs, the Backstage, the Quick Access Toolbar, key tips and focus; it never executes business logic itself. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonApplicationButton` | `Button` | The File tab of the Ribbon tab strip. Opens the `Ribbon.Backstage`. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonBackstageTabItem` | `HeaderedContentControl` | A selectable page in the `RibbonBackstage` navigation pane (for example Home, New, Open). Its `ContentControl.Content` is shown in the Backstage content area when selected. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonBackstageButton` | `Button` | A command in the `RibbonBackstage` navigation pane (for example Save or Close). By default clicking it closes the Backstage after the command runs. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonBackstage` | `Selector` | A selectable page in the `RibbonBackstage` navigation pane (for example Home, New, Open). Its `ContentControl.Content` is shown in the Backstage content area when selected. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonButton` | `Button` | A Ribbon command button with large, medium and small presentations. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonToggleButton` | `System.Windows.Controls.Primitives.ToggleButton` | A Ribbon toggle command (for example Bold) whose checked state is shared by every presentation. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonCheckBox` | `CheckBox` | A Ribbon check box (for example View › Ruler). Check boxes are always laid out as rows. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonComboBox` | `ComboBox` | A Ribbon combo box (for example font name or size). Editors keep keyboard focus while in use. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonTextBox` | `TextBox` | A Ribbon text box with an optional label. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonControlGroup` | `ItemsControl` | Lays out related commands (for example Bold, Italic, Underline) as one row unit. Children are shown icon-only unless their `RibbonItem.VariantBehaviorProperty` says otherwise. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonSeparator` | `Control` | A vertical separator between columns of commands in a group. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonGallery` | `ListBox` | A Ribbon gallery (for example Styles). At the large variant a row of items is shown inline with line-scroll and expand buttons; at smaller variants it becomes a drop-down button. The expanded view lists the same data items. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonGroup` | `HeaderedItemsControl` | A labeled group of commands within a `RibbonTabItem` (for example Clipboard or Font). The containing tab assigns the group's `Variant`; at `VariantSize.Collapsed` the group is shown as a single button that opens its content in a popup. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonChromeButton` | `System.Windows.Controls.Primitives.ToggleButton` | The small button used for a group's dialog launcher and for overflow and display-option buttons. It participates in key tips using its `System.Windows.Automation.AutomationProperties.NameProperty`. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonIconPresenter` | `FrameworkElement` | Renders a Ribbon command icon from an `ImageSource` or, when no image is assigned, from a font glyph. A missing image and glyph render nothing but keep the reserved size so labels stay aligned. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonScreenTip` | `ToolTip` | A rich Ribbon tooltip with a title, explanation, optional shortcut text and an optional footer. Ribbon commands create one automatically from their label, `RibbonItem.DescriptionProperty` and `RibbonItem.KeyGestureTextProperty`; assign your own to `FrameworkElement.ToolTip` to replace it. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonMenuItem` | `MenuItem` | A menu entry inside a Ribbon drop-down, overflow menu or Backstage list. Behaves like a `MenuItem` and adds Ribbon identity, icons and key tip support. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonMenuButton` | `ItemsControl` | A Ribbon command that opens a drop-down of `RibbonMenuItem`s, galleries or other content. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonSplitButton` | `RibbonMenuButton` | A Ribbon command with a primary action and a separate drop-down (for example Paste). Clicking the primary surface raises `Click` and executes `Command`; opening the drop-down never invokes the primary action. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonGroupItemsPanel` | `Panel` | Lays out the commands of one `RibbonGroup`. In Classic mode large commands take a full column and other commands stack up to three per column; in Simplified mode commands form one row and overflowed commands are skipped. Used as the group's items panel. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonGroupsPanel` | `Panel` | The adaptive layout engine for a tab's groups. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonContextMenu` | `ContextMenu` | A context menu styled with the Ribbon theme tokens, used for the Ribbon's display options and Quick Access Toolbar menus. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonQuickAccessToolbar` | `ItemsControl` | The Quick Access Toolbar. It hosts independently parented presentations of Ribbon commands; their checked and enabled state is bound to the source commands, so it never diverges. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonContainerPanel` | `Panel` | Hosts a `Ribbon` and the application's document content. Ribbons are stacked at the top and every other child fills the remaining area; the Backstage covers the whole panel. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonContextualTabGroup` | `Freezable` | Associates contextual tabs (for example Picture Format) with an activation state. Tabs reference a group through `RibbonTabItem.ContextualTabGroupKey`; they are shown only while `IsActive` is true. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonTabItem` | `HeaderedItemsControl` | A task-oriented Ribbon tab containing `RibbonGroup`s. Its header is presented in the Ribbon's tab strip; its groups are laid out by `RibbonGroupsPanel`. | [Ribbon.md](controls/Ribbon.md) |
| `RibbonTabHeader` | `ContentControl` | The header of a `RibbonTabItem` in the tab strip. Generated by the `Ribbon`. | [Ribbon.md](controls/Ribbon.md) |

## Support Controls

These public controls exist for native style dictionaries and low-level template composition. Prefer the higher-level controls in the main inventory unless you are editing Mosaic's native styles or need this exact primitive.

| Control | Base Class | Primary Use | Details |
|---|---|---|---|
| `SliderRepeatButton` | `RepeatButton` | Slider track repeat buttons with corner orientation metadata | [SupportControls.md](controls/SupportControls.md) |
| `SystemDropShadowChrome` | `Decorator` | WPF-style chrome shadow rendering for popups and native templates | [SupportControls.md](controls/SupportControls.md) |
| `WDScrollViewer` | `ScrollViewer` | Native `TreeView` template scrolling with optional wheel animation | [SupportControls.md](controls/SupportControls.md) |

| `CheckBoxListItem` | `ListBoxItem` | The item container used by `CheckBoxList`. | [SupportControls.md](controls/SupportControls.md) |
| `WDBorder` | `Border` | Provides a border that exposes a content clipping geometry. | [SupportControls.md](controls/SupportControls.md) |
| `CalendarBox` | `ListBox` | Represents the scrollable grid of day cells displayed inside the `DatePicker` popup. | [SupportControls.md](controls/SupportControls.md) |
| `CalendarBoxItem` | `ListBoxItem` | Represents a single day cell within a `CalendarBox`. | [SupportControls.md](controls/SupportControls.md) |
| `CalendarSwitch` | `System.Windows.Controls.Primitives.ToggleButton` | Represents the toggle button that opens and closes the calendar popup inside a `DatePicker`. | [SupportControls.md](controls/SupportControls.md) |
| `ChevronButton` | `Button` | Represents a navigation button styled with a chevron arrow used to move between months in a `DatePicker`. | [SupportControls.md](controls/SupportControls.md) |
| `DayOfWeek` | `Label` | Represents a label that displays an abbreviated day-of-week header (such as "Sun", "Mon") in the calendar grid. | [SupportControls.md](controls/SupportControls.md) |
| `DateSpinnerSelector` | `ListBox` | A single vertical wheel used by `DateSpinner` to choose a month, a day, or a year. The selected entry sits in a highlighted band in the middle of the control with neighbouring values visible above and below it. | [SupportControls.md](controls/SupportControls.md) |
| `DateSpinnerSelectorItem` | `ListBoxItem` | The container generated for each entry in a `DateSpinnerSelector`. It exists so the wheel's entries can be styled independently of ordinary `ListBoxItem` instances. | [SupportControls.md](controls/SupportControls.md) |
| `CalendarEventPresenter` | `ContentControl` | Presents one source item inside a `DayCalendarView` timeline. | [SupportControls.md](controls/SupportControls.md) |
| `DayTimelinePanel` | `Panel` | Arranges calendar-event presenters using continuous time coordinates and draws a 24-hour timeline. | [SupportControls.md](controls/SupportControls.md) |
| `DocumentTabItem` | `System.Windows.Controls.TabItem` | Represents the tab container generated for a `Document`. | [SupportControls.md](controls/SupportControls.md) |
| `HorizontalListBoxItem` | `ListBoxItem` | The item container used by `HorizontalListBox`. Renders as a single cell whose fill color communicates whether it is toggled on or off. | [SupportControls.md](controls/SupportControls.md) |
| `Arc` | `Shape` | A `Shape` that draws a circular arc, pie, or ring of shapes between a start and end angle. It is the rendering primitive used by `RadialProgressBar`. | [SupportControls.md](controls/SupportControls.md) |
| `SideMenuItemControl` | `UserControl` | Represents a control for displaying and interacting with a single item in a side menu. | [SupportControls.md](controls/SupportControls.md) |
| `TabItem` | `System.Windows.Controls.TabItem` | Represents a single tab item within a `System.Windows.Controls.TabControl`. | [SupportControls.md](controls/SupportControls.md) |
| `WaveformVisualizerBase` | `FrameworkElement` | Provides shared rendering and capture lifecycle behavior for waveform visualizers. | [SupportControls.md](controls/SupportControls.md) |

---

## Theme Tokens

Reference theme brushes via `DynamicResource` using `MosaicTheme` static keys:

```xml
xmlns:themes="http://schemas.apexgate.net/wpf/mosaic-ui"

<!-- Foreground / background -->
Foreground="{DynamicResource {x:Static themes:MosaicTheme.ControlTextForegroundBrush}}"
Background="{DynamicResource {x:Static themes:MosaicTheme.ControlBackgroundBrush}}"

<!-- Border -->
BorderBrush="{DynamicResource {x:Static themes:MosaicTheme.ControlBorderBrush}}"

<!-- Accent / status -->
Fill="{DynamicResource {x:Static themes:MosaicTheme.AccentBrush}}"
Fill="{DynamicResource {x:Static themes:MosaicTheme.SuccessBrush}}"
Fill="{DynamicResource {x:Static themes:MosaicTheme.WarningBrush}}"
Fill="{DynamicResource {x:Static themes:MosaicTheme.ErrorBrush}}"
Fill="{DynamicResource {x:Static themes:MosaicTheme.InfoBrush}}"

<!-- Font sizes -->
FontSize="{DynamicResource {x:Static themes:MosaicTheme.FontSizeSmall}}"
FontSize="{DynamicResource {x:Static themes:MosaicTheme.FontSizeMedium}}"
FontSize="{DynamicResource {x:Static themes:MosaicTheme.FontSizeLarge}}"
```

---

## App Setup

Consumer apps wire Mosaic by:
1. Using `MosaicApp<TSettings, TViewModel>` as the `Application` base in `App.xaml`.
2. Merging a `ThemeManager` into application resources.
3. Placing `mosaic:WindowTitleBar` + `WindowChromeBehavior` in `MainWindow`.

See the `mosaic-setup-project` skill for the full wiring procedure.

---

## Quick Tips

- **Always use `DynamicResource`** for theme tokens — switching themes requires live updates.
- **CustomControls** (most controls) support re-templating; `UserControl`-based controls (`ColorPicker`, `ChatThread`, `ColumnChart`, `PieChart`, `MarkdownEditor`, `SideMenu`, `ToastMessage`, `TypingProgress`, `WindowTitleBar`, `StretchWindowButton`) do not.
- **`VT52Terminal`** supports the canonical Mosaic URI; its CLR namespace is `Mosaic.UI.Wpf.Controls.VT52Terminal`.
- **`InputWaveformVisualizer`** and **`LoopbackWaveformVisualizer`** live in `Mosaic.UI.Wpf.Controls.WaveformVisualizer`, but are mapped to the canonical Mosaic XAML URI.
- **AvalonDock** lives in `Mosaic.UI.Wpf.AvalonDock` / package `MosaicUIWpf.AvalonDock` and uses `xmlns:ad="https://github.com/blakepell/MosaicUIWpf"`.
- **`AdaptiveImage`** requires `ThemeManager` in the DI container (`AppServices`).
- **`PropertyGrid`** reads `[Category]`, `[Description]`, and `[PropertyGridAttribute]` attributes on the target object.
- **`SideMenu`** uses `ContentTypeIsSingleton` + `AppServices` DI for singleton view navigation in the demo.
- Public support controls such as `WDScrollViewer`, `SliderRepeatButton`, and `SystemDropShadowChrome` are primarily used by native theme dictionaries; see [SupportControls.md](controls/SupportControls.md).

## Keeping the inventory current

After adding or renaming controls, update the appropriate table and linked reference page. From the repository root, run:

```powershell
python .agents/skills/mosaic-controls/scripts/check_inventory.py
```

The checker follows public control base classes, includes XAML code-behind, and reports missing inventory entries and broken local Markdown links. It does not verify API descriptions or defaults; compare those with source. AvalonDock is indexed as a separate integration rather than enumerating its entire upstream control surface.
