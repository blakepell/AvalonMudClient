# Mosaic UI for WPF — Behaviors

**Namespace:** `Mosaic.UI.Wpf.Behaviors`
**Sources:** `src/Mosaic.UI.Wpf/Behaviors/*.cs`

Behaviors add functionality to **existing** WPF controls without subclassing them. They are the escape hatch for the "I like the stock `TextBox`/`DataGrid`/`Window` but want one more capability" case. Reach for a behavior before writing a new control.

There are two flavors:

- **`Behavior<T>`** (from `Microsoft.Xaml.Behaviors`) — attached inside `<i:Interaction.Behaviors>`. Most Mosaic behaviors are these.
- **Attached-property helpers** (static classes) — applied directly as `behaviors:Foo.Bar="..."` on the target element. `GridViewSortBehavior`, `WindowChromeBehavior`, and `BrushModifier` are these.

### Namespaces

```xml
<!-- The behaviors CLR namespace -->
xmlns:behaviors="clr-namespace:Mosaic.UI.Wpf.Behaviors;assembly=Mosaic.UI.Wpf"

<!-- Required for Behavior<T> (Interaction.Behaviors) forms -->
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
```

The `Mosaic.UI.Wpf.Behaviors` CLR namespace is also mapped to the canonical Mosaic XAML URI (`http://schemas.apexgate.net/wpf/mosaic-ui`), so behaviors can also be reached through the `mosaic:` prefix (the demo uses `<mosaic:FocusBehavior />`, for example). Requires `Microsoft.Xaml.Behaviors.Wpf`.

> **File name ≠ class name** for four behaviors: `AvalonEditBindingBehavior.cs` → `AvalonTextEditorBindingBehavior`, `TextBoxClearOnEscapeBehavior.cs` → `TextBoxClearTextOnEscapeBehavior`, `CloseWindowOnEscape.cs` → `CloseWindowOnEscapeBehavior`, `ButtonOpenContextMenu.cs` → `OpenContextMenuBehavior`. Use the class name in XAML.

---

## Inventory

| Behavior | Kind | Attaches to | Purpose |
|---|---|---|---|
| `FocusBehavior` | `Behavior<Control>` | `Control` | Focus the control on load |
| `BlinkingBehavior` | `Behavior<FrameworkElement>` | `FrameworkElement` | Blink (animate opacity) an element |
| `FrameworkElementZoomFontSizeOnMouseWheelBehavior` | `Behavior<FrameworkElement>` | `FrameworkElement` | Ctrl+MouseWheel font zoom |
| `BrushModifier` | Attached helper | `Control` / `Border` / `Panel` | Lighten/darken background brush by HSL % |
| `TextBoxCopyBehavior` | `Behavior<Button>` | `Button` | Copy a target `TextBox` to clipboard on click |
| `TextBoxClearTextOnEscapeBehavior` | `Behavior<TextBoxBase>` | `TextBoxBase` | Clear text on Escape |
| `BlockCaretBehavior` | `Behavior<TextBox>` | `TextBox` | Terminal-style block caret |
| `ItemsControlAutoScrollBehavior` | `Behavior<ItemsControl>` | `ItemsControl` | Auto-scroll to newest item |
| `ItemsControlFilterBehavior` | `Behavior<TextBox>` | `TextBox` | Filter a target `ItemsControl` |
| `DataGridFilterBehavior` | `Behavior<TextBox>` | `TextBox` | Filter a target `DataGrid` |
| `DataGridLastColumnFillBehavior` | `Behavior<DataGrid>` | `DataGrid` | Stretch last column to fill |
| `GridViewLastColumnFillBehavior` | `Behavior<ListView>` | `ListView` | Auto-size last GridView column |
| `GridViewSortBehavior` | Attached helper | `ListView` / `GridViewColumn` | Click-header sort for GridView |
| `ListViewDeleteBehavior` | `Behavior<ListView>` | `ListView` | Delete selected items on Delete key |
| `OpenContextMenuBehavior` | `Behavior<ButtonBase>` | `ButtonBase` | Open the button's `ContextMenu` on click |
| `OpenWindowBehavior` | `Behavior<FrameworkElement>` | `ButtonBase` / `MenuItem` | Open a window of a given type on click |
| `CloseWindowOnEscapeBehavior` | `Behavior<Window>` | `Window` | Close the window on Escape |
| `WindowChromeBehavior` | Attached helper | `Window` | Custom borderless chrome + rounded corners |
| `AvalonTextEditorBindingBehavior` | `Behavior<TextEditor>` | AvalonEdit `TextEditor` | Bindable text/selection/caret — see [AvalonEditBehaviors.md](AvalonEditBehaviors.md) |
| `AvalonEditPropertiesBehavior` | `Behavior<TextEditor>` | AvalonEdit `TextEditor` | Caret brush + hyperlinks — see [AvalonEditBehaviors.md](AvalonEditBehaviors.md) |
| `AvalonEditVtTerminalBehavior` | `Behavior<TextEditor>` | AvalonEdit `TextEditor` | Retro VT/CRT skin — see [AvalonEditBehaviors.md](AvalonEditBehaviors.md) |
| `AvalonEditCopyBehavior` | `Behavior<Button>` | `Button` | Copy a target `TextEditor` to clipboard — see [AvalonEditBehaviors.md](AvalonEditBehaviors.md) |

The four `AvalonEdit*` behaviors are documented in [AvalonEditBehaviors.md](AvalonEditBehaviors.md).

---

## General / element behaviors

### FocusBehavior

Sets keyboard focus to the control once it loads. If applied to multiple controls, the last one to load wins.

No properties.

```xml
<TextBox>
    <i:Interaction.Behaviors>
        <behaviors:FocusBehavior />
    </i:Interaction.Behaviors>
</TextBox>
```

### BlinkingBehavior

Animates the element's `Opacity` (1 → 0, auto-reversing, forever) to make it blink.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsBlinking` | `bool` | `false` | Starts/stops the blink. |
| `BlinkDuration` | `double` | `0.5` | Half-cycle duration in seconds. A full on→off→on cycle is double this value because the animation auto-reverses. |

```xml
<TextBlock Text="Blinking Behavior">
    <i:Interaction.Behaviors>
        <behaviors:BlinkingBehavior IsBlinking="True" BlinkDuration="0.5" />
    </i:Interaction.Behaviors>
</TextBlock>
```

### FrameworkElementZoomFontSizeOnMouseWheelBehavior

Holds Ctrl and scrolls the mouse wheel to zoom the element's inherited `TextElement.FontSize`. The new size is clamped between the min/max.

| Property | Type | Default | Description |
|---|---|---|---|
| `MinFontSize` | `double` | `4.0` | Smallest allowed font size. |
| `MaxFontSize` | `double` | `96.0` | Largest allowed font size. |

> These are plain CLR properties, **not** dependency properties — settable in XAML but not bindable.

```xml
<TextBlock Text="Ctrl+Wheel to zoom">
    <i:Interaction.Behaviors>
        <behaviors:FrameworkElementZoomFontSizeOnMouseWheelBehavior MinFontSize="8" MaxFontSize="48" />
    </i:Interaction.Behaviors>
</TextBlock>
```

### BrushModifier (attached helper)

Lightens or darkens an element's background `SolidColorBrush` by an HSL-lightness percentage, re-applying automatically when the underlying background changes. Handles `Control`, `Border`, and `Panel` (adjusts their `Background`). Useful for deriving hover/zebra shades from a single theme brush without a converter.

| Attached property | Type | Default | Description |
|---|---|---|---|
| `AdjustBackgroundPercentage` | `double` | `0.0` | Lightness adjustment, clamped `-1.0..1.0`. Negative darkens, positive lightens. |
| `IsModified` | `bool` | `false` | Internal marker set on generated brushes to prevent re-processing loops (rarely set by hand). |

```xml
<Border behaviors:BrushModifier.AdjustBackgroundPercentage="-0.2"
        Background="{DynamicResource {x:Static themes:MosaicTheme.ControlBackgroundBrush}}" />
```

---

## TextBox behaviors

### TextBoxCopyBehavior

Attached to a **`Button`**. On click, copies the target `TextBox`'s selected text to the clipboard, or the full text if nothing is selected.

| Property | Type | Default | Description |
|---|---|---|---|
| `TargetTextBox` | `TextBox` | `null` | The TextBox to copy from. |

```xml
<Button Content="Copy selected text">
    <i:Interaction.Behaviors>
        <behaviors:TextBoxCopyBehavior TargetTextBox="{Binding ElementName=CopySourceTextBox}" />
    </i:Interaction.Behaviors>
</Button>
```

### TextBoxClearTextOnEscapeBehavior

Clears the control's text when Escape is pressed while it has focus. Handles `TextBox` and `RichTextBox`, and falls back to reflectively setting a writable `Text` property on any other `TextBoxBase`. Does not mark the event handled.

No properties. (Class name is `TextBoxClearTextOnEscapeBehavior`; file is `TextBoxClearOnEscapeBehavior.cs`.)

```xml
<TextBox Text="Press Escape to clear">
    <i:Interaction.Behaviors>
        <behaviors:TextBoxClearTextOnEscapeBehavior />
    </i:Interaction.Behaviors>
</TextBox>
```

### BlockCaretBehavior

Replaces the standard I-beam caret in a `TextBox` with a full-character-width block caret (terminal / classic-editor style). Hides the native caret, draws a rectangle adorner at the caret position, tracks focus/selection/scroll, and (optionally) re-draws the character on top so it stays legible. The block hides while a range of text is selected.

| Property | Type | Default | Description |
|---|---|---|---|
| `BlockBrush` | `Brush` | `null` → TextBox `Foreground` | Fill color of the block rectangle. |
| `CharacterBrush` | `Brush` | `null` → TextBox `Background` | Color of the character drawn over the block. |
| `ShowCharacterUnderCaret` | `bool` | `true` | When false, the block is a solid opaque rectangle that hides the character. |

```xml
<TextBox FontFamily="Consolas" FontSize="14"
         Background="#0D0D0D" Foreground="#33FF33" CaretBrush="Transparent">
    <i:Interaction.Behaviors>
        <behaviors:BlockCaretBehavior BlockBrush="#33FF33" CharacterBrush="#0D0D0D" />
    </i:Interaction.Behaviors>
</TextBox>
```

---

## ItemsControl / ListView / DataGrid behaviors

### ItemsControlAutoScrollBehavior

Auto-scrolls to the last item whenever the items collection changes — the "keep the newest chat message / log line in view" behavior. Special-cases `ListView` and `ListBox`; falls back to bring-into-view for a plain `ItemsControl`.

No properties.

```xml
<ListBox ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <behaviors:ItemsControlAutoScrollBehavior />
    </i:Interaction.Behaviors>
</ListBox>
```

### ItemsControlFilterBehavior

Attached to a **`TextBox`**. Filters the target `ItemsControl`'s default collection view by `item.ToString().Contains(text)` (case-insensitive), with a 300 ms debounce. `SideMenuHeader` items are always shown regardless of the search term.

| Property | Type | Default | Description |
|---|---|---|---|
| `TargetItemsControl` | `ItemsControl` | `null` | The control whose items are filtered. |

```xml
<TextBox x:Name="SearchBox">
    <i:Interaction.Behaviors>
        <behaviors:ItemsControlFilterBehavior TargetItemsControl="{Binding ElementName=MyListBox}" />
    </i:Interaction.Behaviors>
</TextBox>
```

### DataGridFilterBehavior

Attached to a **`TextBox`** (works nicely on `mosaic:SearchBox`). Debounced (300 ms) full-text filter over **all** `TypeDescriptor` properties of each DataGrid row item; a failing getter is skipped rather than aborting the row.

| Property | Type | Default | Description |
|---|---|---|---|
| `TargetDataGrid` | `DataGrid` | `null` | The grid whose rows are filtered. |

```xml
<mosaic:SearchBox Watermark="Filter rows...">
    <i:Interaction.Behaviors>
        <behaviors:DataGridFilterBehavior TargetDataGrid="{Binding ElementName=SampleGrid}" />
    </i:Interaction.Behaviors>
</mosaic:SearchBox>
```

### DataGridLastColumnFillBehavior

Forces the last `DataGrid` column to `*` (star) width so it fills the remaining horizontal space. Re-applies on Loaded and SizeChanged.

No properties.

```xml
<DataGrid AutoGenerateColumns="False" ItemsSource="{Binding Rows}">
    <i:Interaction.Behaviors>
        <behaviors:DataGridLastColumnFillBehavior />
    </i:Interaction.Behaviors>
</DataGrid>
```

### GridViewLastColumnFillBehavior

The `ListView`/`GridView` counterpart: sets the last `GridViewColumn` width to `double.NaN` (auto-size to content) on Loaded/SizeChanged.

No properties.

```xml
<ListView ItemsSource="{Binding Rows}">
    <i:Interaction.Behaviors>
        <behaviors:GridViewLastColumnFillBehavior />
    </i:Interaction.Behaviors>
    <ListView.View>
        <GridView>...</GridView>
    </ListView.View>
</ListView>
```

### GridViewSortBehavior (attached helper)

Click a `GridView` column header to sort the collection view by a named property; clicking the same header again toggles ascending/descending.

| Attached property | Set on | Type | Default | Description |
|---|---|---|---|---|
| `IsEnabled` | `ListView` | `bool` | `false` | Enables header-click sorting. |
| `SortPropertyName` | `GridViewColumn` | `string` | `null` | Property name each column sorts by. |

```xml
<ListView behaviors:GridViewSortBehavior.IsEnabled="True" ItemsSource="{Binding Rows}">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="Name"
                            behaviors:GridViewSortBehavior.SortPropertyName="Name"
                            DisplayMemberBinding="{Binding Name}" />
        </GridView>
    </ListView.View>
</ListView>
```

### ListViewDeleteBehavior

Deletes the selected items when Delete is pressed (the `ItemsSource` must be an `IList`), then reselects a sensible adjacent item. Optionally prompts for confirmation first.

| Property | Type | Default | Description |
|---|---|---|---|
| `ConfirmDelete` | `bool` | `false` | Show a Yes/No confirmation dialog before deleting. |

```xml
<ListView ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <behaviors:ListViewDeleteBehavior ConfirmDelete="True" />
    </i:Interaction.Behaviors>
</ListView>
```

---

## Button / MenuItem behaviors

### OpenContextMenuBehavior

Opens the button's own `ContextMenu` on left-click (turning a plain button into a menu trigger) at a configurable placement. Class name is `OpenContextMenuBehavior` (file `ButtonOpenContextMenu.cs`).

| Property | Type | Default | Description |
|---|---|---|---|
| `Placement` | `PlacementMode` | `Bottom` | Where the menu opens relative to the button. |

```xml
<Button Content="Open menu">
    <Button.ContextMenu>
        <ContextMenu>
            <MenuItem Header="First option" />
            <MenuItem Header="Second option" />
        </ContextMenu>
    </Button.ContextMenu>
    <i:Interaction.Behaviors>
        <behaviors:OpenContextMenuBehavior Placement="Right" />
    </i:Interaction.Behaviors>
</Button>
```

### OpenWindowBehavior

Instantiates and shows a `Window` of a given type when the attached `ButtonBase` or `MenuItem` is clicked. Attaching to anything else throws `InvalidOperationException`.

| Property | Type | Default | Description |
|---|---|---|---|
| `WindowType` | `Type` | `null` | A type deriving from `Window` to instantiate. |
| `IsDialog` | `bool` | `false` | When true, `ShowDialog()` (and set `Owner`); otherwise `Show()`. |
| `WindowStartupLocation` | `WindowStartupLocation` | `CenterOwner` | Startup location of the opened window. |

```xml
<MenuItem Header="_Open">
    <i:Interaction.Behaviors>
        <behaviors:OpenWindowBehavior WindowType="{x:Type local:MainWindow}" IsDialog="True" />
    </i:Interaction.Behaviors>
</MenuItem>
```

---

## Window behaviors

### CloseWindowOnEscapeBehavior

Closes the window when Escape is pressed. If the window has an `Owner` (i.e. it is a dialog), sets `DialogResult = false` first (guarded). Class name is `CloseWindowOnEscapeBehavior` (file `CloseWindowOnEscape.cs`).

No properties.

```xml
<Window ...>
    <i:Interaction.Behaviors>
        <behaviors:CloseWindowOnEscapeBehavior />
    </i:Interaction.Behaviors>
</Window>
```

### WindowChromeBehavior (attached helper)

Applies and maintains `System.Windows.Shell.WindowChrome` for a borderless custom-chrome window (pairs with `mosaic:WindowTitleBar`), with optional themed window brushes and native rounded corners/region. This is the behavior the `mosaic-setup-project` skill applies to `MainWindow`.

| Attached property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `false` | Master switch for the chrome behavior. |
| `CaptionHeight` | `double` | `0` | Non-client caption (drag) height. |
| `CornerRadius` | `CornerRadius` | `10` | Chrome corner radius. |
| `ResizeBorderThickness` | `Thickness` | `5` | Hit-test thickness of the resize border. |
| `GlassFrameThickness` | `Thickness` | `0` | DWM glass frame thickness. |
| `UseAeroCaptionButtons` | `bool` | `true` | Use native aero caption buttons. |
| `ApplyThemeWindowBrushes` | `bool` | `false` | Apply Mosaic `WindowBackgroundBrush`/`WindowForegroundBrush` to the window. |
| `RoundBorder` | `bool` | `false` | Give the window a rounded-border template plus a native rounded HWND region so a set border follows the corners. |

```xml
<Window ...
        behaviors:WindowChromeBehavior.IsEnabled="True"
        behaviors:WindowChromeBehavior.CaptionHeight="32"
        behaviors:WindowChromeBehavior.CornerRadius="10"
        behaviors:WindowChromeBehavior.ApplyThemeWindowBrushes="True"
        behaviors:WindowChromeBehavior.RoundBorder="True">
    ...
</Window>
```

---

## AvalonEdit behaviors

`AvalonTextEditorBindingBehavior`, `AvalonEditPropertiesBehavior`, `AvalonEditVtTerminalBehavior`, and `AvalonEditCopyBehavior` target the AvalonEdit `TextEditor` (or a `Button` copying from one). See [AvalonEditBehaviors.md](AvalonEditBehaviors.md) for full details.

## ImageMouseWheelZoomBehavior

**Base class:** `Behavior<Image>`\
**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Source:** `src/Mosaic.UI.Wpf/Behaviors/ImageMouseWheelZoomBehavior.cs`

Adds mouse-wheel zooming to an `Image` by applying a render scale transform.

**Demo:** `src/MosaicWpfDemo/Views/Examples/ImageMouseWheelZoomBehaviorExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `double MinZoom` | Gets or sets the minimum zoom scale. |
| `double MaxZoom` | Gets or sets the maximum zoom scale. |
| `double ZoomFactor` | Gets or sets the multiplier applied for each mouse-wheel step. |
| `bool ResetOnSourceChanged` | Gets or sets a value that indicates whether zoom resets when the image source changes. |
| `bool RequireCtrl` | Gets or sets a value that indicates whether the Ctrl key must be held to zoom. |
| `FrameworkElement? ZoomTarget` | Gets or sets the element the zoom transform is applied to. |


## OpenFolderBehavior

**Base class:** `Behavior<FrameworkElement>`\
**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Source:** `src/Mosaic.UI.Wpf/Behaviors/OpenFolderBehavior.cs`

Behavior to open a folder in Windows Explorer when attached to a ButtonBase or MenuItem.

### API starting points

| Member | Purpose |
|---|---|
| `string? Folder` | Gets or sets the path of the folder to open. |


## VisualCaptureContextMenuBehavior

**Base class:** `Behavior<FrameworkElement>`\
**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Source:** `src/Mosaic.UI.Wpf/Behaviors/VisualCaptureContextMenuBehavior.cs`

Adds "Copy as Image" and "Save as Image..." items to the context menu of the element the behavior is attached to. When the element has no context menu one is created; when it already has one the items are appended after a separator. By default the attached element itself is captured, but a different element can be chosen with `Target` or `TargetName`. Scrolling elements are captured at their full scroll extent.

### API starting points

| Member | Purpose |
|---|---|
| `UIElement? Target` | Gets or sets the element to capture. Defaults to the element the behavior is attached to and takes precedence over `TargetName`. |
| `string? TargetName` | Gets or sets the x:Name of the element to capture, resolved from the attached element's name scope. |
| `object CopyHeader` | Gets or sets the header of the copy menu item. |
| `object SaveHeader` | Gets or sets the header of the save menu item. |
| `bool ShowCopy` | Gets or sets whether the copy item is shown. |
| `bool ShowSave` | Gets or sets whether the save item is shown. |
| `bool InsertSeparator` | Gets or sets whether a separator is inserted before the capture items when they are appended to an existing menu that already has items. |
| `string? FilePath` | Gets or sets the file the image is saved to. When empty the user is prompted with a save dialog. |
| `string? DefaultFileName` | Gets or sets the file name pre-populated in the save dialog. |
| `Brush? CaptureBackground` | Gets or sets a brush painted behind the captured element. Leave `null` to keep unpainted areas transparent. |
| `bool CaptureScrollExtent` | Gets or sets whether a scrolling target is expanded so its full scroll extent is captured. |
| `event EventHandler<VisualCapturedEventArgs>? Captured;` | Occurs after the image has been copied or saved. |
| `event EventHandler<VisualCapturedEventArgs>? CaptureFailed;` | Occurs when the capture could not be completed. |
| `UIElement? ResolveTarget(…)` | Resolves the element that will be captured. |
| `bool Capture(…)` | Performs a capture of the resolved target. |


## WindowCloseConfirmationBehavior

**Base class:** `Behavior<Window>`\
**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Source:** `src/Mosaic.UI.Wpf/Behaviors/WindowCloseConfirmationBehavior.cs`

A behavior that prompts the user with a confirmation message when attempting to close a window.

**Demo:** `src/MosaicWpfDemo/Views/Examples/WindowCloseConfirmationBehaviorExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `string Message` | Gets or sets the confirmation message displayed to the user. |
| `bool IsEnabled` | Gets or sets if the confirmation is currently enabled. |
