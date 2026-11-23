# Ribbon controls

## Ribbon

**Base class:** `Selector`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/Ribbon.cs`

A Microsoft Office–style command Ribbon. The Ribbon coordinates tab selection, the effective layout mode, minimization, size collapse, contextual tabs, the Backstage, the Quick Access Toolbar, key tips and focus; it never executes business logic itself.

**Demo:** `src/MosaicWpfDemo/Views/Examples/RibbonExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `event RoutedEventHandler CommandInvoked` | Occurs after any command within the Ribbon is invoked. |
| `static RibbonLayoutMode GetLayoutMode(…)` | Gets the effective layout mode of an element inside a Ribbon. |
| `static void SetLayoutMode(…)` | Sets the layout mode. Set it on the `Ribbon`. |
| `static UserInterfaceDensity GetDensity(…)` | Gets the effective density of an element inside a Ribbon. |
| `static void SetDensity(…)` | Sets the density. Set it on the `Ribbon`. |
| `static RoutedUICommand ToggleMinimizeCommand` | Toggles `IsMinimized`. Bound to Ctrl+F1. |
| `RibbonLayoutMode LayoutMode` | Gets or sets the layout mode (a user preference). |
| `UserInterfaceDensity Density` | Gets or sets the command density (a user preference independent of the layout mode). |
| `bool CanChangeLayoutMode` | Gets or sets a value indicating whether the user may switch between Classic and Simplified. |
| `bool IsMinimized` | Gets or sets a value indicating whether only the tab headers are shown (a user preference). Selecting a tab while minimized opens its commands transiently. |
| `bool IsMinimizable` | Gets or sets a value indicating whether the user may minimize the Ribbon. |
| `bool IsMinimizedPopupOpen` | Gets or sets a value indicating whether a minimized Ribbon's selected tab is shown transiently. |
| `bool IsCollapsed` | Gets a value indicating whether the whole Ribbon is hidden because the host is smaller than `CollapseThresholdSize`. This is a temporary constraint and never changes `IsMinimized`. |
| `bool IsCollapsible` | Gets or sets a value indicating whether the Ribbon hides itself below the threshold size. |
| `Size CollapseThresholdSize` | Gets or sets the host size (Ribbon width × window height, in DIPs) below which the Ribbon collapses. |
| `RibbonBackstage? Backstage` | Gets or sets the full-window File surface opened by the application button. |
| `object? ApplicationButtonContent` | Gets or sets the application button content. |
| `bool IsBackstageOpen` | Gets a value indicating whether the Backstage is open. |
| `ContentControl? QuickAccessToolbarHost` | Gets or sets an optional external host for the above-ribbon Quick Access Toolbar, for example in `WindowTitleBar.LeftContent`. Use an empty, dedicated host in the same window. When the location is Below, the toolbar returns to the Ribbon. Set to null to restore internal hosting. |
| `QuickAccessToolbarLocation QuickAccessToolbarLocation` | Gets or sets where the Quick Access Toolbar is shown. |
| `QuickAccessToolbarMode QuickAccessToolbarMode` | Gets or sets whether the Quick Access Toolbar is visible and whether the user may change that. |
| `bool QuickAccessToolbarShowLabels` | Gets or sets a value indicating whether Quick Access Toolbar buttons show their labels. |
| `bool IsQuickAccessToolbarVisible` | Gets a value indicating whether the Quick Access Toolbar is currently shown. |
| `object? TabRowToolbarContent` | Gets or sets optional content at the right end of the tab strip (for example Comments and Share buttons). |
| `object? FooterContent` | Gets or sets optional content below the command card (for example a message bar). |
| `bool ShowDisplayOptionsButton` | Gets or sets a value indicating whether the Ribbon display options button is shown. |
| `RibbonTabIndicatorAnimation TabIndicatorAnimation` | Gets or sets the animation the selected-tab indicator plays when the selected tab changes. Animations are skipped when Windows client-area animations are turned off. The default is `RibbonTabIndicatorAnimation.Expand`. |
| `bool AreKeyTipsEnabled` | Gets or sets a value indicating whether Alt / F10 shows key tips. |
| `FreezableCollection<RibbonContextualTabGroup> ContextualTabGroups` | Gets the contextual tab groups. Tabs join a group through `RibbonTabItem.ContextualTabGroupKey`. |
| `event EventHandler? IsMinimizedChanged;` | Occurs when `IsMinimized` changes. |
| `event EventHandler? IsCollapsedChanged;` | Occurs when `IsCollapsed` changes. |
| `event EventHandler? LayoutModeChanged;` | Occurs when `LayoutMode` changes. |
| `event EventHandler<QuickAccessToolbarItemAddingEventArgs>? QuickAccessToolbarItemAdding;` | Occurs before a command is added to the Quick Access Toolbar. Set Cancel to prevent it. |
| `event EventHandler<QuickAccessToolbarItemEventArgs>? QuickAccessToolbarItemAdded;` | Occurs after a command is added to the Quick Access Toolbar. |
| `event EventHandler<QuickAccessToolbarItemEventArgs>? QuickAccessToolbarItemRemoved;` | Occurs after a command is removed from the Quick Access Toolbar. |
| `event EventHandler? QuickAccessToolbarLocationChanged;` | Occurs when `QuickAccessToolbarLocation` changes. |
| `event EventHandler? QuickAccessToolbarModeChanged;` | Occurs when `QuickAccessToolbarMode` changes. |
| `event EventHandler? PreferencesChanged;` | Occurs when a persisted preference changes (mode, density, minimized state or Quick Access Toolbar), so hosts can save with `RibbonSerializer`. |
| `ReadOnlyObservableCollection<RibbonTabHeader> TabHeaders` | Gets the generated tab headers, ordinary tabs first and contextual tabs last. |
| `ReadOnlyObservableCollection<FrameworkElement> QuickAccessToolbarItems` | Gets the Quick Access Toolbar presentations, in order. |
| `IReadOnlyList<string> QuickAccessToolbarKeys` | Gets the ordered command keys in the Quick Access Toolbar. |
| `RibbonQuickAccessToolbar QuickAccessToolbar` | Gets the Quick Access Toolbar control. |
| `RibbonTabItem? SelectedTab` | Gets the selected tab container. |
| `KeyTipManager? KeyTips` | Gets the key tip service for this Ribbon, once loaded. |
| `void SelectTab(…)` | Selects a tab. When the Ribbon is minimized and `openWhenMinimized` is set, the tab's commands open transiently. |
| `bool SelectTab(…)` | Selects the tab whose `RibbonTabItem.Key` (or, when it has no key, header text) matches `key`. |
| `RibbonTabItem? FindTab(…)` | Finds a tab by its `RibbonTabItem.Key`, falling back to its header text, or returns null. |
| `void FocusSelectedTabContent(…)` | Moves keyboard focus into the selected tab's commands. |
| `void CloseTransientSurfaces(…)` | Closes every transient command surface: the minimized popup, collapsed-group popups, overflow menus and drop-downs of the selected tab and the Quick Access Toolbar. |
| `bool RestoreDocumentFocus(…)` | Returns keyboard focus to the element that is focused in the Ribbon's parent focus scope (normally the document), if it is still valid. |
| `void OpenBackstage(…)` | Opens the Backstage, if one is assigned. |
| `void CloseBackstage(…)` | Closes the Backstage, if it is open, and returns focus to the element focused before it opened. Unlike the back button and Escape, this works even when `RibbonBackstage.CanClose` is false. |
| `IEnumerable<IRibbonControl> EnumerateCommands(…)` | Enumerates every keyed command in the Ribbon (including unrealized tabs and drop-down items). The registry is derived from the logical tree on demand, so it never holds visual-parent relationships. |
| `IRibbonControl? FindCommand(…)` | Finds a command by key, or returns null. |
| `IReadOnlyList<string> ValidateKeys(…)` | Validates the identity registry and returns a diagnostic for each duplicate key. Duplicates are rejected for Quick Access Toolbar and persistence lookups (the first registration wins). |
| `bool CanAddToQuickAccessToolbar(…)` | Returns whether the command (a key or a control) can be added to the Quick Access Toolbar. |
| `bool CanRemoveFromQuickAccessToolbar(…)` | Returns whether the command (a key, control or Quick Access Toolbar presentation) is in the toolbar. |
| `bool AddToQuickAccessToolbar(…)` | Adds a command to the Quick Access Toolbar. Raises the cancellable `QuickAccessToolbarItemAdding` first; adding to a hidden toolbar reveals it. |
| `bool RemoveFromQuickAccessToolbar(…)` | Removes a command from the Quick Access Toolbar. |

## RibbonApplicationButton

**Base class:** `Button`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonBackstage.cs`

The File tab of the Ribbon tab strip. Opens the `Ribbon.Backstage`.

## RibbonBackstageTabItem

**Base class:** `HeaderedContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonBackstage.cs`

A selectable page in the `RibbonBackstage` navigation pane (for example Home, New, Open). Its `ContentControl.Content` is shown in the Backstage content area when selected.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsSelected` | Gets or sets a value indicating whether the page is selected. |
| `string? Glyph` | Gets or sets the navigation icon glyph. |

## RibbonBackstageButton

**Base class:** `Button`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonBackstage.cs`

A command in the `RibbonBackstage` navigation pane (for example Save or Close). By default clicking it closes the Backstage after the command runs.

### API starting points

| Member | Purpose |
|---|---|
| `string? Glyph` | Gets or sets the navigation icon glyph. |
| `bool ClosesBackstage` | Gets or sets a value indicating whether clicking the button closes the Backstage. |

## RibbonBackstage

**Base class:** `Selector`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonBackstage.cs`

A selectable page in the `RibbonBackstage` navigation pane (for example Home, New, Open). Its `ContentControl.Content` is shown in the Backstage content area when selected.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsSelected` | Gets or sets a value indicating whether the page is selected. |
| `string? Glyph` | Gets or sets the navigation icon glyph. |

## RibbonButton

**Base class:** `Button`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonButton.cs`

A Ribbon command button with large, medium and small presentations.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key used by the Quick Access Toolbar and persistence. |
| `string? Label` | Gets or sets the display label. |
| `ImageSource? LargeImage` | Gets or sets the 32×32 image used by the large variant. |
| `ImageSource? SmallImage` | Gets or sets the 16×16 image used by the medium and small variants. |
| `string? Glyph` | Gets or sets a font glyph used when no image is assigned. |
| `Brush? GlyphBrush` | Gets or sets an optional glyph color. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize PreferredVariant` | Gets or sets the most expanded variant used when space allows. |
| `VariantSize Variant` | Gets the container-assigned variant. |
| `void PerformClick(…)` | Activates the button as if clicked, honoring `UIElement.IsEnabled`. |

## RibbonToggleButton

**Base class:** `System.Windows.Controls.Primitives.ToggleButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonButton.cs`

A Ribbon toggle command (for example Bold) whose checked state is shared by every presentation.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the display label. |
| `ImageSource? LargeImage` | Gets or sets the large image. |
| `ImageSource? SmallImage` | Gets or sets the small image. |
| `string? Glyph` | Gets or sets a font glyph used when no image is assigned. |
| `Brush? GlyphBrush` | Gets or sets an optional glyph color. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize PreferredVariant` | Gets or sets the most expanded variant used when space allows. |
| `VariantSize Variant` | Gets the container-assigned variant. |
| `void PerformClick(…)` | Toggles the button as if clicked, honoring `UIElement.IsEnabled`. |

## RibbonCheckBox

**Base class:** `CheckBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonButton.cs`

A Ribbon check box (for example View › Ruler). Check boxes are always laid out as rows.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the display label. |
| `string? Description` | Gets or sets the screen tip description. |
| `void PerformClick(…)` | Toggles the check box as if clicked, honoring `UIElement.IsEnabled`. |

## RibbonComboBox

**Base class:** `ComboBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonEditors.cs`

A Ribbon combo box (for example font name or size). Editors keep keyboard focus while in use.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the label. It is always the automation name, and is displayed when `ShowLabel` is true. |
| `ImageSource? SmallImage` | Gets or sets the small image shown before the label. |
| `string? Glyph` | Gets or sets a font glyph shown before the label. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize Variant` | Gets the container-assigned variant. The label is hidden at `VariantSize.Small`. |
| `double InputWidth` | Gets or sets the width of the editable box, excluding the label. `double.NaN` sizes it to its content. |
| `bool ShowLabel` | Gets or sets a value indicating whether the label is displayed beside the box. |

## RibbonTextBox

**Base class:** `TextBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonEditors.cs`

A Ribbon text box with an optional label.

### API starting points

| Member | Purpose |
|---|---|
| `ICommand? Command` | Gets or sets the command executed when the user presses Enter (for example a search or "Go to" box). The parameter is `CommandParameter` when set, otherwise the current text. |
| `object? CommandParameter` | Gets or sets a fixed command parameter. When null, `TextBox.Text` is passed. |
| `IInputElement? CommandTarget` | Gets or sets the routed command target. |
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the label. |
| `string? Glyph` | Gets or sets a font glyph shown before the label. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize Variant` | Gets the container-assigned variant. The label is hidden at `VariantSize.Small`. |
| `double InputWidth` | Gets or sets the width of the text area, excluding the label. |

## RibbonControlGroup

**Base class:** `ItemsControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonEditors.cs`

Lays out related commands (for example Bold, Italic, Underline) as one row unit. Children are shown icon-only unless their `RibbonItem.VariantBehaviorProperty` says otherwise.

## RibbonSeparator

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonEditors.cs`

A vertical separator between columns of commands in a group.

## RibbonGallery

**Base class:** `ListBox`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonGallery.cs`

A Ribbon gallery (for example Styles). At the large variant a row of items is shown inline with line-scroll and expand buttons; at smaller variants it becomes a drop-down button. The expanded view lists the same data items.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the label shown on the drop-down button variants. |
| `ImageSource? LargeImage` | Gets or sets the large image. |
| `ImageSource? SmallImage` | Gets or sets the small image. |
| `string? Glyph` | Gets or sets a font glyph used when no image is assigned. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize PreferredVariant` | Gets or sets the most expanded variant (defaults to the inline `VariantSize.Large` gallery). |
| `VariantSize Variant` | Gets the container-assigned variant. |
| `ICommand? Command` | Gets or sets the command executed when the user picks an item (a click or Enter on an inline item, or a commit in the expanded view). The parameter is `CommandParameter` when set, otherwise the picked item. Programmatic selection changes never execute it. |
| `object? CommandParameter` | Gets or sets a fixed command parameter. When null, the picked item is passed. |
| `IInputElement? CommandTarget` | Gets or sets the routed command target. |
| `double ItemWidth` | Gets or sets the width of each gallery cell. Must be a finite value greater than zero. |
| `double ItemHeight` | Gets or sets the height of each gallery cell. Must be a finite value greater than zero. |
| `int InlineColumns` | Gets or sets the number of columns shown inline at the large variant (at least one). |
| `int DropDownColumns` | Gets or sets the number of columns in the expanded view (at least one). |
| `double InlineWidth` | Gets the width of the inline item area (columns × item width). |
| `double DropDownItemsWidth` | Gets the width of the expanded item area (drop-down columns × item width). |
| `bool IsDropDownOpen` | Gets or sets a value indicating whether the expanded view is open. |
| `double MaxDropDownHeight` | Gets or sets the maximum height of the expanded item list (greater than zero; infinity removes the limit). |
| `object? DropDownFooter` | Gets or sets content (typically `RibbonMenuItem`s) shown under the expanded list. |
| `object? HighlightedItem` | Gets the item currently highlighted for preview, or null. |
| `event RoutedPropertyChangedEventHandler<object?> ItemHighlighted` | Occurs when the highlighted (previewed) item changes. NewValue is null when the highlight ends, at which point a live preview should revert to the selection. |

## RibbonGroup

**Base class:** `HeaderedItemsControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonGroup.cs`

A labeled group of commands within a `RibbonTabItem` (for example Clipboard or Font). The containing tab assigns the group's `Variant`; at `VariantSize.Collapsed` the group is shown as a single button that opens its content in a popup.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the group identity referenced by `RibbonGroupVariant.GroupKey`. When unset, the header text is used. |
| `ImageSource? LargeImage` | Gets or sets the image shown on the collapsed group button. |
| `string? Glyph` | Gets or sets a font glyph shown on the collapsed group button when no image is assigned. |
| `VariantSize Variant` | Gets the size the containing tab assigned to the group. Container managed. |
| `bool IsCollapsible` | Gets or sets a value indicating whether the group may be reduced to a collapsed button. |
| `RibbonOverflowTarget ChildOverflowTarget` | Gets or sets where this group's commands go when they overflow in Simplified mode. |
| `bool IsDropDownOpen` | Gets or sets a value indicating whether a collapsed group's popup is open. |
| `ICommand? LauncherCommand` | Gets or sets the command run by the dialog launcher in the group's corner. |
| `object? LauncherCommandParameter` | Gets or sets the launcher command parameter. |
| `bool ShowLauncher` | Gets or sets a value indicating whether the launcher is shown without a `LauncherCommand` (handle `LauncherClick` instead). |
| `bool HasLauncher` | Gets a value indicating whether the dialog launcher is shown. |
| `object? LauncherToolTip` | Gets or sets the launcher's tooltip, for example a `RibbonScreenTip` describing the dialog. |
| `bool HasOverflowItems` | Gets a value indicating whether Simplified mode moved commands to this group's overflow drop-down. |
| `bool IsOverflowOpen` | Gets or sets a value indicating whether the group overflow drop-down is open. |
| `ReadOnlyObservableCollection<FrameworkElement> OverflowItems` | Gets the generated presentations of this group's overflowed commands (Simplified mode, group target). |
| `event RoutedEventHandler LauncherClick` | Occurs when the dialog launcher is clicked. |
| `void InvokeLauncher(…)` | Runs the dialog launcher. |
| `bool IsInlineEmpty` | Gets a value indicating whether every command of the group moved to the tab overflow menu in Simplified mode, in which case the group takes no space. |

## RibbonChromeButton

**Base class:** `System.Windows.Controls.Primitives.ToggleButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonGroup.cs`

The small button used for a group's dialog launcher and for overflow and display-option buttons. It participates in key tips using its `System.Windows.Automation.AutomationProperties.NameProperty`.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsToggle` | Gets or sets a value indicating whether the button toggles (drop-down openers) or acts as a push button (launchers). |

## RibbonIconPresenter

**Base class:** `FrameworkElement`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonIconPresenter.cs`

Renders a Ribbon command icon from an `ImageSource` or, when no image is assigned, from a font glyph. A missing image and glyph render nothing but keep the reserved size so labels stay aligned.

### API starting points

| Member | Purpose |
|---|---|
| `static readonly FontFamily DefaultGlyphFontFamily` | The default glyph font: Segoe Fluent Icons on Windows 11 with a Segoe MDL2 Assets fallback. |
| `ImageSource? Source` | Gets or sets the image to show. Takes precedence over `Glyph`. |
| `ImageSource? FallbackSource` | Gets or sets an image used when `Source` is unset (for example the small image scaled up when a large image is missing). |
| `string? Glyph` | Gets or sets the font glyph shown when no image is available. |
| `Brush? GlyphBrush` | Gets or sets an optional glyph color; the inherited foreground is used when unset. |
| `double IconSize` | Gets or sets the square icon size in device-independent units. |
| `bool HasIcon` | Gets a value indicating whether the presenter has an image or glyph to show. |

## RibbonScreenTip

**Base class:** `ToolTip`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonIconPresenter.cs`

A rich Ribbon tooltip with a title, explanation, optional shortcut text and an optional footer. Ribbon commands create one automatically from their label, `RibbonItem.DescriptionProperty` and `RibbonItem.KeyGestureTextProperty`; assign your own to `FrameworkElement.ToolTip` to replace it.

### API starting points

| Member | Purpose |
|---|---|
| `string? Title` | Gets or sets the bold heading. |
| `string? Description` | Gets or sets the explanatory text. |
| `string? GestureText` | Gets or sets the shortcut text shown beside the title, for example (Ctrl+B). |
| `object? Footer` | Gets or sets optional footer content shown under a separator (for example "Press F1 for more help"). |
| `DataTemplate? FooterTemplate` | Gets or sets the template used to display `Footer`. |

## RibbonMenuItem

**Base class:** `MenuItem`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonMenuButton.cs`

A menu entry inside a Ribbon drop-down, overflow menu or Backstage list. Behaves like a `MenuItem` and adds Ribbon identity, icons and key tip support.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the label used for Quick Access Toolbar and automation when `HeaderedItemsControl.Header` is not text. |
| `ImageSource? SmallImage` | Gets or sets the 16×16 image. |
| `string? Glyph` | Gets or sets a font glyph used when no image is assigned. |
| `Brush? GlyphBrush` | Gets or sets an optional glyph color. |
| `string? Description` | Gets or sets an optional second line of explanatory text displayed under the header. |
| `void PerformClick(…)` | Activates the item as if clicked, honoring `UIElement.IsEnabled`. |

## RibbonMenuButton

**Base class:** `ItemsControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonMenuButton.cs`

A Ribbon command that opens a drop-down of `RibbonMenuItem`s, galleries or other content.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the stable command key. |
| `string? Label` | Gets or sets the display label. |
| `ImageSource? LargeImage` | Gets or sets the large image. |
| `ImageSource? SmallImage` | Gets or sets the small image. |
| `string? Glyph` | Gets or sets a font glyph used when no image is assigned. |
| `Brush? GlyphBrush` | Gets or sets an optional glyph color. |
| `string? Description` | Gets or sets the screen tip description. |
| `VariantSize PreferredVariant` | Gets or sets the most expanded variant used when space allows. |
| `VariantSize Variant` | Gets the container-assigned variant. |
| `bool IsDropDownOpen` | Gets or sets a value indicating whether the drop-down is open. |
| `double MaxDropDownHeight` | Gets or sets the maximum height of the drop-down before it scrolls (greater than zero; infinity removes the limit). |
| `event RoutedEventHandler DropDownOpened` | Occurs when the drop-down opens. |
| `event RoutedEventHandler DropDownClosed` | Occurs when the drop-down closes. |

## RibbonSplitButton

**Base class:** `RibbonMenuButton`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonMenuButton.cs`

A Ribbon command with a primary action and a separate drop-down (for example Paste). Clicking the primary surface raises `Click` and executes `Command`; opening the drop-down never invokes the primary action.

### API starting points

| Member | Purpose |
|---|---|
| `ICommand? Command` | Gets or sets the primary-action command. |
| `object? CommandParameter` | Gets or sets the primary-action command parameter. |
| `IInputElement? CommandTarget` | Gets or sets the routed command target for the primary action. |
| `bool IsCheckable` | Gets or sets a value indicating whether the primary action toggles `IsChecked` (for example Bullets or Highlight in Word). The toggle happens before `Command` executes. |
| `bool IsChecked` | Gets or sets a value indicating whether the primary action is checked. Shown only when `IsCheckable` is true; can be bound to view model state either way. |
| `event RoutedEventHandler Click` | Occurs when the primary action surface (not the drop-down indicator) is activated. |
| `void PerformClick(…)` | Invokes the primary action, honoring command availability. |

## RibbonGroupItemsPanel

**Base class:** `Panel`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonPanels.cs`

Lays out the commands of one `RibbonGroup`. In Classic mode large commands take a full column and other commands stack up to three per column; in Simplified mode commands form one row and overflowed commands are skipped. Used as the group's items panel.

## RibbonGroupsPanel

**Base class:** `Panel`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonPanels.cs`

The adaptive layout engine for a tab's groups.

### API starting points

| Member | Purpose |
|---|---|
| `int LastTransitionCount` | Gets the number of layout transitions applied in the last measure pass (diagnostics). |
| `void ScrollBy(…)` | Scrolls the group strip horizontally by `delta` units. |
| `void InvalidateWidthCache(…)` | Discards cached widths, for example after content changed in a way that does not change the width of the currently displayed variant. |

## RibbonContextMenu

**Base class:** `ContextMenu`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonPresentationFactory.cs`

A context menu styled with the Ribbon theme tokens, used for the Ribbon's display options and Quick Access Toolbar menus.

### API starting points

| Member | Purpose |
|---|---|
| `static void BindVisuals(…)` | Binds the shared visual identity (label, images, glyph, description) of a source to a presentation. |

## RibbonQuickAccessToolbar

**Base class:** `ItemsControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonQuickAccessToolbar.cs`

The Quick Access Toolbar. It hosts independently parented presentations of Ribbon commands; their checked and enabled state is bound to the source commands, so it never diverges.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsMenuOpen` | Gets or sets a value indicating whether the customize menu is open. |
| `Ribbon Ribbon` | Gets the Ribbon that owns the toolbar. |

## RibbonContainerPanel

**Base class:** `Panel`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonQuickAccessToolbar.cs`

Hosts a `Ribbon` and the application's document content. Ribbons are stacked at the top and every other child fills the remaining area; the Backstage covers the whole panel.

## RibbonContextualTabGroup

**Base class:** `Freezable`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonTabItem.cs`

Associates contextual tabs (for example Picture Format) with an activation state. Tabs reference a group through `RibbonTabItem.ContextualTabGroupKey`; they are shown only while `IsActive` is true.

### API starting points

| Member | Purpose |
|---|---|
| `string? Key` | Gets or sets the identity tabs reference. |
| `string? Header` | Gets or sets the context name exposed to automation and tooltips (for example "Picture Tools"). |
| `Brush? Brush` | Gets or sets the accent used for the group's tab headers. When unset the theme accent is used. |
| `bool IsActive` | Gets or sets a value indicating whether the context is active and its tabs are shown. |

## RibbonTabItem

**Base class:** `HeaderedItemsControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonTabItem.cs`

A task-oriented Ribbon tab containing `RibbonGroup`s. Its header is presented in the Ribbon's tab strip; its groups are laid out by `RibbonGroupsPanel`.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsSelected` | Gets or sets a value indicating whether the tab is selected. |
| `string? Key` | Gets or sets the tab identity used by `Ribbon.FindTab` and by persisted preferences. When unset, the header text is used. |
| `string? ContextualTabGroupKey` | Gets or sets the `RibbonContextualTabGroup.Key` this tab belongs to; null for ordinary tabs. |
| `Collection<RibbonGroupVariant> GroupVariants` | Gets the ordered Classic group transitions. Empty means right-to-left default reduction. |
| `Collection<RibbonControlVariant> ControlVariants` | Gets the ordered Simplified command transitions applied before the default reduction. |
| `ReadOnlyObservableCollection<FrameworkElement> OverflowItems` | Gets the generated presentations of commands in the tab overflow menu (Simplified mode). |
| `bool HasOverflowItems` | Gets a value indicating whether any command is in the tab overflow menu. |
| `bool IsOverflowOpen` | Gets or sets a value indicating whether the tab overflow menu is open. |
| `bool CanScrollLeft` | Gets a value indicating whether groups are scrolled out of view to the left. |
| `bool CanScrollRight` | Gets a value indicating whether groups extend beyond the right edge. |
| `Ribbon? Ribbon` | Gets the owning Ribbon. |

## RibbonTabHeader

**Base class:** `ContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Ribbon/RibbonTabItem.cs`

The header of a `RibbonTabItem` in the tab strip. Generated by the `Ribbon`.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsSelected` | Gets a value indicating whether the header's tab is selected. |
| `Brush? ContextualBrush` | Gets the accent of the header's contextual tab group, or null for ordinary tabs. |
| `bool HasContextualBrush` | Gets a value indicating whether a contextual accent is assigned (otherwise the theme accent is used). |
| `bool IsContextual` | Gets a value indicating whether the header belongs to a contextual tab. |
| `bool IsFirstContextual` | Gets a value indicating whether this is the first header of a contextual group (draws a divider). |
| `RibbonTabItem Tab` | Gets the tab this header represents. |
