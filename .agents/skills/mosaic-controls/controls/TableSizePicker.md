# TableSizePicker

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/TableSizePicker/TableSizePicker.cs`

A grid of cells used to pick a table size in the same manner as the Microsoft Word Insert Table button. Hovering a cell previews a region anchored at the upper left cell, clicking commits it and raises `TableSizeSelected` followed by `RequestClose` so a hosting popup can close.

**Demo:** `src/MosaicWpfDemo/Views/Examples/TableSizePickerExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `int RowCount` | The number of rows of cells the picker displays. Values are coerced into the range 1 to 50. |
| `int ColumnCount` | The number of columns of cells the picker displays. Values are coerced into the range 1 to 50. |
| `int SelectedRowCount` | The number of rows in the committed selection, or zero when nothing has been committed. |
| `int SelectedColumnCount` | The number of columns in the committed selection, or zero when nothing has been committed. |
| `int PreviewRowCount` | The number of rows currently being previewed, or zero when there is no preview. The preview never overwrites the committed selection. |
| `int PreviewColumnCount` | The number of columns currently being previewed, or zero when there is no preview. The preview never overwrites the committed selection. |
| `ObservableCollection<TableSizePickerCell> Cells` | The cells rendered by the control template, in row major order. Rebuilt whenever `RowCount` or `ColumnCount` changes. |
| `string SelectionText` | The caption displayed with the grid, for example 4 × 6 Table. Reflects the preview dimensions while hovering, the committed dimensions otherwise, and `EmptySelectionText` when neither exists. |
| `string SelectionTextFormat` | The composite format string used to build `SelectionText` where {0} is the row count and {1} is the column count. Defaults to {0} × {1} Table. |
| `string EmptySelectionText` | The caption displayed when there is neither a preview nor a committed selection. |
| `bool ClearSelectionOnShow` | Whether `OnShow` clears the committed selection. Defaults to true since an insert table picker normally opens with nothing selected. |
| `double CellWidth` | The width of an individual cell. |
| `double CellHeight` | The height of an individual cell. |
| `Thickness CellMargin` | The spacing rendered around each cell. |
| `Brush? CellBackground` | The fill used for cells that are neither previewed nor selected. |
| `Brush? CellBorderBrush` | The outline used for cells that are neither previewed nor selected. |
| `Brush? HighlightBackground` | The fill used for cells inside the hover or keyboard preview region. |
| `Brush? HighlightBorderBrush` | The outline used for cells inside the hover or keyboard preview region. |
| `Brush? CommittedBackground` | The fill used for cells inside the committed selection. |
| `Brush? CommittedBorderBrush` | The outline used for cells inside the committed selection. |
| `event EventHandler<TableSizeSelectedEventArgs> TableSizeSelected` | Occurs when the user commits a table size. Bubbles so a hosting ribbon, popup or view can handle it. |
| `event EventHandler? RequestClose;` | Occurs after a selection is committed or when the user presses Escape. A hosting popup or dropdown should close itself in response, the picker has no knowledge of its host. |
| `ICommand ClearCommand` | A command that calls `Clear`. |
| `void Clear(…)` | Clears the preview and the committed selection returning the control to its initial state. |
| `void OnShow(…)` | Prepares the control for interaction. Clears any stale hover state, optionally clears the committed selection based on `ClearSelectionOnShow`, and moves focus to the control so the keyboard can be used immediately. Call this when the hosting popup or dropdown opens. |
| `void OnHide(…)` | Returns the control to a clean state, clearing the preview and releasing the mouse if it is captured. The committed selection is preserved. Called automatically after a selection is committed. |
| `void Select(…)` | Commits the supplied table size exactly as a click would, raising `TableSizeSelected` and then `RequestClose`. |


## Usage notes

Host the picker inside a popup or menu, call `OnShow()`/`OnHide()` with the host lifecycle, and handle `RequestClose` when the user commits or dismisses a selection.
