# SqliteQueryControl

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/SqliteQueryControl/SqliteQueryControl.cs`

A SQLite query workbench: a database explorer tree, a SQL editor with schema aware auto completion, and a results grid, wired to a transport style run/cancel tool bar.

**Demo:** `src/MosaicWpfDemo/Views/Examples/SqliteQueryControlExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `string? ConnectionString` | Gets or sets the connection string used to reach the database. Setting it reloads the schema. |
| `string? DatabaseFilePath` | Gets or sets the path of the database file. This is a convenience over `ConnectionString`; setting it builds a Data Source= connection string. |
| `SqliteSchema? Schema` | Gets the database schema as of the last refresh. This drives the explorer tree and the auto completion candidates. |
| `string QueryText` | Gets or sets the text currently in the SQL editor. |
| `string StatusText` | Gets or sets the status message shown beside the run tool bar. |
| `bool IsQueryExecuting` | Gets a value indicating whether a query is currently running. |
| `bool RefreshSchemaAfterQuery` | Gets or sets a value indicating whether the schema is reloaded after every query, so DDL executed in the editor shows up in the explorer. Defaults to `true`. |
| `bool AutoCompleteEnabled` | Gets or sets a value indicating whether schema aware auto completion is offered in the editor. Defaults to `true`. |
| `int RowLimit` | Gets or sets the row count used by the explorer's "Select Top N Rows" command. Defaults to 1,000. |
| `bool ShowExportOptions` | Gets or sets a value indicating whether the save and export menu is visible. Defaults to `true`. |
| `event EventHandler<SqliteSchemaContextMenuEventArgs> SchemaContextMenuRequested` | Raised after the control has populated the standard explorer context menu items for the clicked node but before the menu is shown, allowing consumers to customize it. |
| `event RoutedEventHandler QueryCompleted` | Raised after a query finishes, whether it succeeded, failed, or was cancelled. Inspect `StatusText` for the outcome. |
| `IAsyncRelayCommand ExecuteQueryCommand` | Runs the current selection, or the whole document when nothing is selected. Reports that it cannot execute while a query is already running or no database is open. |
| `IAsyncRelayCommand ExecuteCurrentStatementCommand` | Runs the current selection, or the single statement on the caret's line when nothing is selected. Reports that it cannot execute while a query is already running or no database is open. |
| `IRelayCommand CancelQueryCommand` | Cancels the running query. Reports that it cannot execute when nothing is running. |
| `IAsyncRelayCommand RefreshSchemaCommand` | Reloads the database schema into the explorer. |
| `IAsyncRelayCommand ExportToExcelCommand` | Exports the data set to Excel. |
| `IAsyncRelayCommand SaveAsJsonCommand` | Saves the data set as JSON. |
| `IAsyncRelayCommand SaveAsMarkdownCommand` | Saves the data set as a Markdown table. |
| `IAsyncRelayCommand SaveAsCsvCommand` | Saves the data set as comma-separated values. |
| `Task OpenDatabaseAsync(…)` | Opens a database file and loads its schema. |
| `async Task ExportToExcelAsync(…)` | Prompts for a destination and creates an Excel workbook from the current query results. |
| `async Task SaveAsJsonAsync(…)` | Prompts for a destination and saves the current query results as JSON. |
| `async Task SaveAsMarkdownAsync(…)` | Prompts for a destination and saves the current query results as a Markdown table. |
| `async Task SaveAsCsvAsync(…)` | Prompts for a destination and saves the current query results as comma-separated values. |
| `async Task RefreshSchemaAsync(…)` | Reloads the database schema into the explorer. |
| `async Task ExecuteQueryAsync(…)` | Executes SQL against the open database and shows the first result set in the grid. |
| `void Dispose(…)` | Cancels any running query and releases the completion window and cached result set. |


## Usage notes

Configure `DatabaseFilePath` or `ConnectionString`, or call `OpenDatabaseAsync`. The control executes SQL against that database; use the cancellation command for a running query and dispose the workbench when it is no longer needed.
