using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dapper;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Data.Sqlite;

namespace Avalon.Sqlite
{
    /// <summary>
    /// Interaction logic for SqliteQueryControl.xaml
    /// </summary>
    public partial class SqliteQueryControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The database schema current as of the last refresh.
        /// </summary>
        public Schema Schema { get; set; }

        /// <summary>
        /// The results of the last query.
        /// </summary>
        public DataTable DataTable { get; set; }

        /// <summary>
        /// The connection string used to connect to the SQLite database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Internal variable for IsQueryExecuting
        /// </summary>
        private bool _isQueryExecuting = false;

        /// <summary>
        /// Whether or not a query is currently executing.
        /// </summary>
        public bool IsQueryExecuting
        {
            get => _isQueryExecuting;
            set
            {
                _isQueryExecuting = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The current text in the query editor.
        /// </summary>
        public string QueryText
        {
            get => SqlEditor.Text;
            set => SqlEditor.Text = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqliteQueryControl()
        {
            InitializeComponent();

            var compact = new ResourceDictionary { Source = new Uri("/ModernWpf;component/DensityStyles/Compact.xaml", UriKind.Relative) };            
            SqlResults.Resources.MergedDictionaries.Add(compact);
            TreeViewSchema.Resources.MergedDictionaries.Add(compact);
            SqlCommandBar.Resources.MergedDictionaries.Add(compact);
            DbExplorerCommandBar.Resources.MergedDictionaries.Add(compact);

            var asm = Assembly.GetExecutingAssembly();
            string resourceName = $"{asm.GetName().Name}.Assets.SQLite.xshd";

            using (var s = asm.GetManifestResourceStream(resourceName))
            {
                using (var reader = new XmlTextReader(s))
                {
                    SqlEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // Intellisense
            SqlEditor.TextArea.TextEntering += SqlEditor_TextEntering;
            SqlEditor.TextArea.TextEntered += SqlEditor_TextEntered;
        }

        /// <summary>
        /// Loaded event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryControl_Loaded(object sender, RoutedEventArgs e)
        {
            SqlEditor.Focus();
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonExecuteSql_ClickAsync(object sender, RoutedEventArgs e)
        {
            await ExecuteQuery();
        }

        /// <summary>
        /// Refreshes the database schema so that the schema explorer shows the latest
        /// schema available in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonRefreshSchema_ClickAsync(object sender, RoutedEventArgs e)
        {
            await this.RefreshSchema();
        }


        /// <summary>
        /// When a key is pressed anywhere on the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void QueryControl_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                // ExecuteQuery() will handle closing autocompletion since it's called from multiple paths.
                await ExecuteQuery();
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
        private void SqlResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            e.Column.Header = header.Replace("_", "__");
        }

        /// <summary>
        /// Executes the query that's currently in the SqlEditor.
        /// </summary>
        public async Task ExecuteQuery()
        {
            this.IsQueryExecuting = true;

            try
            {
                // Close the auto complete window box if its open.
                _completionWindow?.Close();

                // Get rid of anything in the current DataTable.
                if (this.DataTable != null)
                {
                    this.DataTable.Clear();
                    this.DataTable.Dispose();
                    SqlResults.ItemsSource = null;
                }

                await using (var conn = new SqliteConnection(this.ConnectionString))
                {
                    await conn.OpenAsync();
                    
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = SqlEditor.Text;

                        await using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            /// The DataSet is required to ignore contraints on the DataTable which is important
                            /// because queries don't always have the contraints of the source tables (e.g. an inner joined
                            /// query can bring back records with keys listed many times because of the join).
                            this.DataTable = new DataTable();

                            using (var ds = new DataSet() { EnforceConstraints = false })
                            {
                                ds.Tables.Add(this.DataTable);
                                this.DataTable.BeginLoadData();
                                this.DataTable.Load(dr);
                                this.DataTable.EndLoadData();
                                ds.Tables.Remove(this.DataTable);
                            }

                            SqlResults.ItemsSource = this.DataTable.DefaultView;
                        }
                    }

                    await conn.CloseAsync();
                }

                TextBlockStatus.Text = $"{this?.DataTable?.Rows?.Count ?? 0} records returned.";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                // TODO - Show this exception.
                TextBlockStatus.Text = "0 records returned.";
            }

            this.IsQueryExecuting = false;
        }

        public async Task RefreshSchema()
        {
            var schema = new Schema();

            await using (var conn = new SqliteConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                schema.DatabaseName = Argus.IO.FileSystemUtilities.ExtractFileName(conn.DataSource);

                SqlMapper.SetTypeMap(typeof(Table), new ColumnAttributeTypeMapper<Table>());
                var ieTables = await conn.QueryAsync<Table>("select * from sqlite_master where type = 'table' order by name");
                schema.Tables = ieTables.ToList();

                foreach (var table in schema.Tables)
                {
                    // From the official SQLite documentation: The quote() function converts its argument into a form that
                    // is appropriate for inclusion in an SQL statement.

                    // So, the PRAGMA command won't work with parameters AND selecting from pragma_table_info isn't working
                    // with this version of SQLite so we're going to use the SQLite quote function to escape our parameter
                    // before doing a string concat.  Before people RAWR in anger this was a recommendation from a senior
                    // development on the Microsoft Entity Framework team.
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@TableName", table.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string tableName = await conn.QueryFirstAsync<string>("SELECT quote(@TableName)", parameters);

                    var ieFields = await conn.QueryAsync<Field>("PRAGMA table_info(" + tableName + ");", parameters);
                    table.Fields = ieFields.ToList();
                }

                SqlMapper.SetTypeMap(typeof(View), new ColumnAttributeTypeMapper<View>());
                var ieViews = await conn.QueryAsync<View>("select * from sqlite_master where type = 'view' order by name");
                schema.Views = ieViews.ToList();

                foreach (var view in schema.Views)
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@ViewName", view.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string viewName = await conn.QueryFirstAsync<string>("SELECT quote(@ViewName)", parameters);

                    // See comment above about escaping via the quote SQLite function.
                    var ieFields = await conn.QueryAsync<Field>("PRAGMA table_info(" + viewName + ");", parameters);
                    view.Fields = ieFields.ToList();
                }

                await conn.CloseAsync();
            }

            this.Schema = schema;
            this.TreeViewSchema.DataContext = this.Schema;
        }

        CompletionWindow _completionWindow;

        void SqlEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text was a space.. see if the previous word was a command that has sub commands.
            if (e.Text == " ")
            {
                string word = GetWordBeforeSpace(SqlEditor);

                if (word.Equals("from", StringComparison.OrdinalIgnoreCase))
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(SqlEditor.TextArea);
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
                _completionWindow = new CompletionWindow(SqlEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;

                foreach (var table in Schema.Tables)
                {
                    foreach (var field in table.Fields)
                    {
                        data.Add(new CompletionData(field.Name, $"Type: {field.Type}\r\nNot Null: {field.NotNull}\r\nDefault Value: {field.DefaultValue}"));
                    }
                }

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        public static string GetWordBeforeSpace(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true)
            {
                if (text == null && text.CompareTo(" ") > 0)
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

        void SqlEditor_TextEntering(object sender, TextCompositionEventArgs e)
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}