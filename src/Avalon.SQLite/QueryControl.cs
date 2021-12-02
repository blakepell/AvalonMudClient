/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Sqlite.Common;
using Avalon.Sqlite.Editor;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ModernWpf.Controls;

/*
 * TODO: Faster selection of all records
 * TODO: Vacuum
 * TODO: Export of data
 */

namespace Avalon.Sqlite
{
    /// <summary>
    /// SQLite Query Control.
    /// </summary>
    public class QueryControl : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SchemaProperty =
            DependencyProperty.Register(nameof(Schema), typeof(Schema), typeof(QueryControl), new PropertyMetadata(new Schema()));

        /// <summary>
        /// The database schema current as of the last refresh.
        /// </summary>
        public Schema Schema
        {
            get => (Schema)GetValue(SchemaProperty);
            set => SetValue(SchemaProperty, value);
        }

        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            nameof(StatusText), typeof(string), typeof(QueryControl), new PropertyMetadata("Status: Idle"));

        /// <summary>
        /// The status text on the upper right hand corner of the control.
        /// </summary>
        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public static readonly DependencyProperty RefreshSchemaAfterQueryProperty = DependencyProperty.Register(
            nameof(RefreshSchemaAfterQuery), typeof(bool), typeof(QueryControl), new PropertyMetadata(true));

        /// <summary>
        /// Whether the schema should refresh after each query batch is run.
        /// </summary>
        public bool RefreshSchemaAfterQuery
        {
            get => (bool)GetValue(RefreshSchemaAfterQueryProperty);
            set => SetValue(RefreshSchemaAfterQueryProperty, value);
        }

        public static readonly DependencyProperty IsQueryExecutingProperty = DependencyProperty.Register(
            nameof(IsQueryExecuting), typeof(bool), typeof(QueryControl), new PropertyMetadata(false));

        /// <summary>
        /// Whether or not a query is currently executing.
        /// </summary>
        public bool IsQueryExecuting
        {
            get => (bool)GetValue(IsQueryExecutingProperty);
            set => SetValue(IsQueryExecutingProperty, value);
        }

        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register(
            nameof(Theme), typeof(ControlTheme), typeof(QueryControl), new PropertyMetadata(ControlTheme.Light));

        /// <summary>
        /// The current theme of the control.
        /// </summary>
        public ControlTheme Theme
        {
            get => (ControlTheme) GetValue(ThemeProperty);
            set
            {
                SetValue(ThemeProperty, value);
                ApplyTheme(value);
            }
        }

        private DataTable _dataTable;

        /// <summary>
        /// The results of the last query.
        /// </summary>
        public DataTable DataTable
        {
            get => _dataTable;
            set
            {
                _dataTable = value;
                NotifyPropertyChanged();
            }
        }

        private string _connectionString;

        /// <summary>
        /// The connection string used to connect to the SQLite database.
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The current text in the query editor.
        /// </summary>
        public string QueryText
        {
            get => _sqlEditor.Text;
            set
            {

                _sqlEditor.Text = value;
                NotifyPropertyChanged();
            }
        }

        private CompletionWindow _completionWindow;

        // References to the UI elements we need to access programatically. 
        private AppBarButton _buttonRefreshSchema;
        private AppBarButton _btnExecuteSql;
        private DataGrid _sqlResults;
        private TextEditor _sqlEditor;
        private TreeView _treeViewSchema;
        private CommandBar _sqlCommandBar;
        private CommandBar _dbExplorerCommandBar;

        static QueryControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QueryControl),
                new FrameworkPropertyMetadata(typeof(QueryControl)));
        }

        /// <summary>
        /// Constructor: Used to find controls on the template and wire up their default behaviors.
        /// </summary>
        public QueryControl()
        {
            this.UpdateDefaultStyle();
            this.ApplyTemplate();
            this.DataContext = this;

            if (this.Template.FindName("ButtonRefreshSchema", this) is AppBarButton btnRefresh)
            {
                _buttonRefreshSchema = btnRefresh;
                _buttonRefreshSchema.Click += this.ButtonRefreshSchema_ClickAsync;
            }

            if (this.Template.FindName("ButtonExecuteSql", this) is AppBarButton btnExecuteSql)
            {
                _btnExecuteSql = btnExecuteSql;
                _btnExecuteSql.Click += this.ButtonExecuteSql_ClickAsync;
            }

            if (this.Template.FindName("TreeViewSchema", this) is TreeView treeViewSchema)
            {
                _treeViewSchema = treeViewSchema;
                _treeViewSchema.ContextMenuOpening += TreeViewSchemaOnContextMenuOpening;
            }

            if (this.Template.FindName("SqlResults", this) is DataGrid sqlResults)
            {
                _sqlResults = sqlResults;
                _sqlResults.AutoGeneratingColumn += SqlResultsOnAutoGeneratingColumn;
            }

            object o = this.Template.FindName("SqlEditor", this);

            if (this.Template.FindName("SqlEditor", this) is TextEditor sqlEditor)
            {
                // Intellisense
                _sqlEditor = sqlEditor;
                _sqlEditor.TextArea.TextEntering += SqlEditor_TextEntering;
                _sqlEditor.TextArea.TextEntered += SqlEditor_TextEntered;
            }

            if (this.Template.FindName("SqlCommandBar", this) is CommandBar sqlCommandBar)
            {
                _sqlCommandBar = sqlCommandBar;
            }

            if (this.Template.FindName("DbExplorerCommandBar", this) is CommandBar dbExplorerCommandBar)
            {
                _dbExplorerCommandBar = dbExplorerCommandBar;
            }

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.KeyDown += OnKeyDown;
        }

        private void TreeViewSchemaOnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_treeViewSchema.FindResource("TableSchemaContextMenu") is ContextMenu tablesContextMenu)
            {
                foreach (var obj in tablesContextMenu.Items)
                {
                    if (obj is MenuItem menuItem)
                    {
                        menuItem.Click -= this.TreeViewMenuItem_ClickAsync;
                        menuItem.Click += this.TreeViewMenuItem_ClickAsync;
                    }
                }
            }

            if (_treeViewSchema.FindResource("ViewSchemaContextMenu") is ContextMenu viewsContextMenu)
            {
                foreach (var obj in viewsContextMenu.Items)
                {
                    if (obj is MenuItem menuItem)
                    {
                        menuItem.Click -= this.TreeViewMenuItem_ClickAsync;
                        menuItem.Click += this.TreeViewMenuItem_ClickAsync;
                    }
                }
            }

        }

        /// <summary>
        /// OnLoaded Event: Used to set initial UI interactions like the setting the focus
        /// into the editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Apply the AvalonEdit coloring now that the DP has been set.
            this.ApplyTheme(this.Theme);

            _sqlEditor.Focus();
        }

        /// <summary>
        /// OnUnloaded Event: Used to cleanup any resources this instance of the control needs
        /// to release or handle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataTable != null)
            {
                this.DataTable.Clear();
                this.DataTable.Dispose();
            }

            // TODO: Unwire the rest of the events.

            if (_treeViewSchema.FindResource("TableSchemaContextMenu") is ContextMenu tablesContextMenu)
            {
                foreach (var obj in tablesContextMenu.Items)
                {
                    if (obj is MenuItem menuItem)
                    {
                        menuItem.Click -= this.TreeViewMenuItem_ClickAsync;
                    }
                }
            }

            if (_treeViewSchema.FindResource("ViewSchemaContextMenu") is ContextMenu viewsContextMenu)
            {
                foreach (var obj in viewsContextMenu.Items)
                {
                    if (obj is MenuItem menuItem)
                    {
                        menuItem.Click -= this.TreeViewMenuItem_ClickAsync;
                    }
                }
            }
        }

        /// <summary>
        /// When a key is pressed anywhere on the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                // TODO: Have this call and the button click call an async
                // function that does the actual search.
                this.ButtonExecuteSql_ClickAsync(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _completionWindow?.Close();
            }
        }

        /// <summary>
        /// Event for when a column auto generates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// The underscore in column headings gets escape because of AccessKey handling.  In order for it
        /// to display correctly we need double it to prevent AccessKey handling.
        /// </remarks>
        private void SqlResultsOnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            if (!string.IsNullOrWhiteSpace(header))
            {
                e.Column.Header = header.Replace("_", "__");
            }
        }

        /// <summary>
        /// Refreshes the database schema so that the schema explorer shows the latest
        /// schema available in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonRefreshSchema_ClickAsync(object sender, RoutedEventArgs e)
        {
            await this.RefreshSchemaAsync();
        }

        /// <summary>
        /// Executes the contents of the SQL editor against the currently loaded SQLite database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonExecuteSql_ClickAsync(object sender, RoutedEventArgs e)
        {
            string sql;

            // Cross thread exception if we pass SqlText.Text into the Task, put it in string first.
            // If text is selected in the editor we'll assume the caller only wants to run that
            // portion of selected text (otherwise run the whole thing).
            if (_sqlEditor.SelectionLength > 1)
            {
                sql = _sqlEditor.SelectedText;
            }
            else
            {
                sql = _sqlEditor.Text;
            }

            await ExecuteQueryAsync(sql);
        }

        /// <summary>
        /// Executes a query and displays it's results on the control.
        /// </summary>
        /// <param name="sql"></param>
        public async Task ExecuteQueryAsync(string sql)
        {
            // Close the auto complete window box if its open.
            _completionWindow?.Close();

            if (this.IsQueryExecuting)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                this.StatusText = "0 records returned.";
                return;
            }

            this.IsQueryExecuting = true;
            this.StatusText = "Status: Executing SQL";

            // Get rid of anything in the current DataTable.
            if (this.DataTable != null)
            {
                this.DataTable.Clear();
                this.DataTable.Dispose();
                _sqlResults.ItemsSource = null;
            }

            try
            {
                this.DataTable = await Task.Run(async () => await this.ExecuteDataTableAsync(sql));

                _sqlResults.BeginInit();
                _sqlResults.ItemsSource = this.DataTable.DefaultView;
                _sqlResults.EndInit();

                if (this.DataTable != null)
                {
                    this.StatusText = $"{DataTable?.Rows.Count.ToString().FormatIfNumber()} {"record".IfCountPluralize(DataTable?.Rows.Count ?? 0, "records")} returned.";
                }
            }
            catch (Exception ex)
            {
                this.StatusText = ex.Message;
            }

            if (this.RefreshSchemaAfterQuery)
            {
                await this.RefreshSchemaAsync();
            }

            this.IsQueryExecuting = false;
        }

        /// <summary>
        /// Executes SQL and returns a <see cref="DataTable"/>.
        /// </summary>
        public async Task<DataTable> ExecuteDataTableAsync(string sql)
        {
            var dt = new DataTable();

            await using (var conn = new SqliteConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    await using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        // The DataSet is required to ignore constraints on the DataTable which is important
                        // because queries don't always have the constraints of the source tables (e.g. an inner joined
                        // query can bring back records with keys listed many times because of the join).
                        using (var ds = new DataSet() { EnforceConstraints = false })
                        {
                            ds.Tables.Add(dt);
                            dt.BeginLoadData();
                            dt.Load(dr);
                            dt.EndLoadData();
                            ds.Tables.Remove(dt);
                        }
                    }
                }

                await conn.CloseAsync();
            }

            return dt;
        }

        /// <summary>
        /// Handles all of the click events from the database explorer's context menus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TreeViewMenuItem_ClickAsync(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as MenuItem;
            string cmd;
            string tag;
            int counter = 0;

            if (item == null)
            {
                return;
            }

            StringBuilder sb;
            Table table;
            cmd = item.CommandParameter.ToString();
            tag = item.Tag.ToString();

            switch (cmd)
            {
                case "SelectAll":
                    await this.ExecuteQueryAsync($"select * from {tag}");
                    break;
                case "Select1000":
                    await this.ExecuteQueryAsync($"select * from {tag} limit 1000");
                    break;
                case "GenerateSelect":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (_sqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }

                    sb.Append("SELECT ");

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , [{0}] -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      [{0}] -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.TrimEnd(',');
                    sb.AppendFormat("\r\nFROM [{0}];", table.Name);

                    _sqlEditor.AppendText(sb.ToString());

                    break;
                case "GenerateInsert":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (_sqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }

                    sb.AppendFormat("INSERT INTO [{0}] (", table.Name);

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , [{0}]", field.Name);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      [{0}]", field.Name);
                        }
                    }

                    counter = 0;
                    sb.Append("\r\n) VALUES (");

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , @{0} -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      @{0} -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.Append("\r\n);");

                    _sqlEditor.AppendText(sb.ToString());

                    break;
                case "GenerateUpdate":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (_sqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }

                    sb.AppendLine("-- Note: Remember to put a WHERE statement on this if you need it.");
                    sb.AppendFormat("UPDATE [{0}] SET ", table.Name);

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n     , [{0}] = @{0} -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n       [{0}] = @{0} -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.Append("\r\n;");

                    _sqlEditor.AppendText(sb.ToString());

                    break;
                case "CreateTable":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    if (_sqlEditor.Text.Length > 0)
                    {
                        _sqlEditor.AppendText("\r\n\r\n");
                    }

                    _sqlEditor.AppendText(table.Sql);

                    break;
                case "CreateView":
                    var view = this.Schema.Views.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (view == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    if (_sqlEditor.Text.Length > 0)
                    {
                        _sqlEditor.AppendText("\r\n\r\n");
                    }

                    _sqlEditor.AppendText(view.Sql);

                    break;

            }
        }

        /// <summary>
        /// Initiates auto completion actions when applicable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text was a space.. see if the previous word was a command that has sub commands.
            if (e.Text == " ")
            {
                string word = GetWordBeforeSpace(_sqlEditor);

                if (word.Equals("from", StringComparison.OrdinalIgnoreCase))
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(_sqlEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    // Add tables
                    foreach (var item in Schema.Tables)
                    {
                        data.Add(new CompletionData(item.TableName));
                    }

                    // Add views
                    foreach (var view in Schema.Views)
                    {
                        data.Add(new CompletionData(view.TableName));
                    }

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate
                    {
                        _completionWindow = null;
                    };
                }
                else
                {
                    return;
                }
            }

            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(_sqlEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;

                foreach (var table in Schema.Tables)
                {
                    foreach (var field in table.Fields)
                    {
                        data.Add(new CompletionData(field.Name, $"Type: {field.Type}\r\nNot Null: {field.NotNull.ToString()}\r\nDefault Value: {field.DefaultValue}"));
                    }
                }

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        /// <summary>
        /// Gets the word before the space in the current SQL editor.
        /// </summary>
        /// <param name="textEditor"></param>
        private static string GetWordBeforeSpace(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true)
            {
                if (text == null && string.Compare(text, " ", StringComparison.Ordinal) > 0)
                {
                    break;
                }

                if (Regex.IsMatch(text, @".*[^A-Za-z\. ]"))
                {
                    break;
                }

                if (text != " ")
                {
                    wordBeforeDot = text + wordBeforeDot;
                }

                if (caretPosition == 0)
                {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }

        /// <summary>
        /// SqlEditor TextEntering Event: Typing behaviors handled with auto completion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlEditor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }


        /// <summary>
        /// Opens or creates a SQLite database as the requested path.  In order to create the
        /// database the directory must already exist.
        /// </summary>
        /// <param name="filePath"></param>
        public async Task OpenDb(string filePath)
        {
            this.ConnectionString = $"Data Source={filePath}";
            await this.RefreshSchemaAsync();
        }

        /// <summary>
        /// Refreshes the database schema.
        /// </summary>
        public async Task RefreshSchemaAsync()
        {
            var schema = new Schema();

            await using (var conn = new SqliteConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                schema.ConnectionString = conn.DataSource;
                schema.DatabaseName = Argus.IO.FileSystemUtilities.ExtractFileName(conn.DataSource);

                SqlMapper.SetTypeMap(typeof(Table), new ColumnAttributeTypeMapper<Table>());
                var ieTables = await conn.QueryAsync<Table>("select * from sqlite_master where type = 'table' order by name");
                schema.Tables = new ObservableCollection<Table>(ieTables);

                foreach (var table in schema.Tables)
                {
                    // From the official SQLite documentation: The quote() function converts its argument into a form that
                    // is appropriate for inclusion in an SQL statement.

                    // So, the PRAGMA command won't work with parameters AND selecting from pragma_table_info isn't working
                    // with this version of SQLite so we're going to use the SQLite quote function to escape our parameter
                    // before doing a string concat.  Before people claim outrage this was a recommendation from a senior
                    // development on the Microsoft Entity Framework team.
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@TableName", table.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string tableName = await conn.QueryFirstAsync<string>("SELECT quote(@TableName)", parameters);

                    SqlMapper.SetTypeMap(typeof(Field), new ColumnAttributeTypeMapper<Field>());
                    var ieFields = await conn.QueryAsync<Field>($"PRAGMA table_info({tableName});", parameters);
                    table.Fields = new ObservableCollection<Field>(ieFields);
                }

                SqlMapper.SetTypeMap(typeof(View), new ColumnAttributeTypeMapper<View>());
                var ieViews = await conn.QueryAsync<View>("select * from sqlite_master where type = 'view' order by name");
                schema.Views = new ObservableCollection<View>(ieViews);

                foreach (var view in schema.Views)
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@ViewName", view.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string viewName = await conn.QueryFirstAsync<string>("SELECT quote(@ViewName)", parameters);

                    // See comment above about escaping via the quote SQLite function.
                    SqlMapper.SetTypeMap(typeof(Field), new ColumnAttributeTypeMapper<Field>());
                    var ieFields = await conn.QueryAsync<Field>($"PRAGMA table_info({viewName});", parameters);
                    view.Fields = new ObservableCollection<Field>(ieFields);
                }

                await conn.CloseAsync();
            }

            this.Schema = schema;
        }


        /// <summary>
        /// Expands the tables <see cref="TreeViewItem"/> node.
        /// </summary>
        public void ExpandTableNode()
        {
            if (this.Template.FindName("TablesNode", this) is TreeViewItem tablesNode)
            {
                tablesNode.IsExpanded = true;
            }
        }

        /// <summary>
        /// Collapses the tables <see cref="TreeViewItem"/> node.
        /// </summary>
        public void CollapseTableNode()
        {
            if (this.Template.FindName("TablesNode", this) is TreeViewItem tablesNode)
            {
                tablesNode.IsExpanded = false;
            }
        }

        /// <summary>
        /// Expands the views <see cref="TreeViewItem"/> node.
        /// </summary>
        public void ExpandViewsNode()
        {
            if (this.Template.FindName("ViewsNode", this) is TreeViewItem viewsNode)
            {
                viewsNode.IsExpanded = true;
            }
        }

        /// <summary>
        /// Collapses the views <see cref="TreeViewItem"/> node.
        /// </summary>
        public void CollapseViewsNode()
        {
            if (this.Template.FindName("ViewsNode", this) is TreeViewItem viewsNode)
            {
                viewsNode.IsExpanded = false;
            }
        }

        /// <summary>
        /// Applies a new theme and accent color.
        /// </summary>
        /// <param name="theme"></param>
        public void ApplyTheme(ControlTheme theme)
        {
            var asm = Assembly.GetExecutingAssembly();
            string resourceName = "";

            switch (theme)
            {
                case ControlTheme.Light:
                    resourceName = $"{asm.GetName().Name}.Editor.SqliteLight.xshd";
                    break;
                case ControlTheme.Dark:
                case ControlTheme.Gray:
                    resourceName = $"{asm.GetName().Name}.Editor.SqliteDark.xshd";
                    break;
            }

            // Update the colors for the syntax editor.
            using (var s = asm.GetManifestResourceStream(resourceName))
            {
                if (s != null)
                {
                    using var reader = new XmlTextReader(s);
                    _sqlEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}