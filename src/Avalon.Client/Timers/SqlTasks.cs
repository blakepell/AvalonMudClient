using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Avalon.Timers
{
    /// <summary>
    /// SQLite interactions.
    /// </summary>
    /// <remarks>
    /// This will be the main entry point for scripts to utilize SQLite.  SQLite integration poses some unique
    /// challenges based off of the way command might be run from triggers.  In a traditional setting we would
    /// know that we're creating a batch and use a transaction which could re-use a <see cref="SQLiteConnection"/>,
    /// and a <see cref="SQLiteCommand"/> while committing at the end for an efficient and low memory set of tasks.
    /// When database operations need to happen from triggers in the game we don't know when that transaction will
    /// necessarily be over and from a traditional sense all of those objects will have to be created every time
    /// which can quickly cause performance and memory problems.  As a result, this class will act as the gate
    /// keeper.  It will run operations perhaps unrelated as transactions for efficiency but also allow for traditional
    /// cohesive transactions to be grouped together.
    ///
    ///       Pause selects if writing
    /// </remarks>
    public class SqlTasks
    {

        /// <summary>
        /// The dispatch timer used for checking the queue.
        /// </summary>
        private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Background);

        /// <summary>
        /// All pending scheduled.
        /// </summary>
        public List<SqlTask> SqlQueue { get; set; } = new List<SqlTask>();

        /// <summary>
        /// The SQLite Connection.
        /// </summary>
        public SqliteConnection Connection { get; set; }

        /// <summary>
        /// A SQLite command cache.
        /// </summary>
        public Dictionary<int, SqliteCommand> SqliteCommandCache { get; set; }

        /// <summary>
        /// The number of database commands that have been run since the creation of this object.
        /// </summary>
        public int CommandsRun { get; set; } = 0;

        /// <summary>
        /// Whether a write operation is currently in process.
        /// </summary>
        public bool IsWriting { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlTasks(string connectionString)
        {
            this.Connection = new SqliteConnection(connectionString);
            this.SqliteCommandCache = new Dictionary<int, SqliteCommand>();

            _timer.Interval = TimeSpan.FromSeconds(App.Settings.AvalonSettings.DatabaseWriteInterval);
            _timer.Tick += SqlTasks_Tick;
        }

        /// <summary>
        /// Main handler and gate keeper for batch write operations on the SQLite database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SqlTasks_Tick(object sender, object e)
        {
            // Disable the timer if there's nothing in the Queue.  It will be restarted
            // when something exists.
            if (this.SqlQueue.Count == 0 || this.Connection.State != ConnectionState.Open)
            {
                this.IsWriting = false;
                _timer.IsEnabled = false;
            }

            this.IsWriting = true;

            try
            {
                await Connection.ExecuteAsync("BEGIN");

                // Run everything that isn't in a batch.
                foreach (var task in SqlQueue)
                {
                    var cmd = this.GetCachedSqliteCommand(task.Sql);

                    for (int i = 0; i < task.Parameters.Length; i++)
                    {
                        cmd.Parameters[i].Value = task.Parameters[i];
                    }

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        this.CommandsRun++;
                    }
                    catch (Exception ex)
                    {
                        App.Conveyor.EchoError(ex.Message);
                    }
                }

                this.SqlQueue.Clear();
                await Connection.ExecuteAsync("COMMIT");
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError(ex.Message);
            }
            finally
            {
                App.MainWindow.ViewModel.PendingSqlStatements = 0;
                this.IsWriting = false;
            }
        }

        /// <summary>
        /// Adds a SQL task.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void Add(string sql, string[] parameters)
        {
            var sqlTask = new SqlTask { Sql = sql, Parameters = parameters };
            this.SqlQueue.Add(sqlTask);

            App.MainWindow.ViewModel.PendingSqlStatements = this.SqlQueue.Count;

            // There is something in the timer, make sure it's enabled.
            _timer.IsEnabled = true;
        }

        /// <summary>
        /// Returns a new or cached database command for a SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        private SqliteCommand GetCachedSqliteCommand(string sql)
        {
            int count = sql.Count(c => c == '@');
            var cmd = GetCachedSqliteCommand(count);
            
            cmd.CommandText = sql;

            return cmd;
        }

        /// <summary>
        /// Returns a new or cached database command for the specified number of parameters.
        /// </summary>
        /// <param name="numberOfParameters"></param>
        private SqliteCommand GetCachedSqliteCommand(int numberOfParameters)
        {
            // Check to see if this already exists.
            if (this.SqliteCommandCache.ContainsKey(numberOfParameters))
            {
                return this.SqliteCommandCache[numberOfParameters];
            }

            var cmd = this.Connection.CreateCommand();

            // Create the parameter list.
            for (int i = 1; i <= numberOfParameters; i++)
            {
                var param = new SqliteParameter
                {
                    ParameterName = $"@{i}"
                };

                cmd.Parameters.Add(param);
            }

            // Add the new SqliteCommand to the cache
            this.SqliteCommandCache.Add(numberOfParameters, cmd);

            return cmd;
        }

        /// <summary>
        /// Clears any database command objects that are cached.
        /// </summary>
        private void ClearCache()
        {
            if (this.SqliteCommandCache.Count == 0)
            {
                return;
            }

            for (int i = this.SqliteCommandCache.Count - 1; i > 0; i--)
            {
                this.SqliteCommandCache[i]?.Dispose();
                this.SqliteCommandCache[i] = null;
                this.SqliteCommandCache.Remove(i);
            }
        }

        /// <summary>
        /// Clears any pending database operations that are in the queue.
        /// </summary>
        public void ClearQueue()
        {
            this.SqlQueue.Clear();
            App.MainWindow.ViewModel.PendingSqlStatements = 0;
            _timer.IsEnabled = false;
        }

        /// <summary>
        /// Flushes any pending database operations.
        /// </summary>
        public void Flush()
        {
            SqlTasks_Tick(null, null);
        }

        /// <summary>
        /// Opens a connection to the database and clears any pending cached database
        /// command objects.
        /// </summary>
        public async Task OpenAsync()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                await this.Connection.OpenAsync();
                this.ClearCache();
            }
        }

        /// <summary>
        /// Returns the first column of the first row.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public object SelectValue(string sql, params string[] parameters)
        {
            this.CommandsRun++;

            // Get a new command for a select so it doesn't overlap with any writes using the cached pool.
            using (var cmd = this.Connection.CreateCommand())
            {
                cmd.CommandText = sql;

                for (int i = 0; i < parameters.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@{i + 1}", parameters[i]);
                }

                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes a parameterized statement and results a result set that Lua can use.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public IEnumerable<Dictionary<string, object>> Select(string sql, params string[] parameters)
        {
            this.CommandsRun++;

            using (var cmd = this.Connection.CreateCommand())
            {
                cmd.CommandText = sql;

                for (int i = 0; i < parameters.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@{i + 1}", parameters[i]);
                }

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var row = new Dictionary<string, object>();

                        for (int x = 0; x <= dr.FieldCount - 1; x++)
                        {
                            row.Add(dr.GetName(x), dr.GetValue(x));
                        }

                        yield return row;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the interval of the batch timer in seconds.
        /// </summary>
        /// <param name="seconds"></param>
        public void SetInterval(int seconds)
        {
            _timer.Interval = TimeSpan.FromSeconds(seconds);
        }

    }

    /// <summary>
    /// A single SQL operation with parameters.
    /// </summary>
    /// <remarks>
    /// SQL statements must be parameterized starting with @1, @2 for any
    /// parameters that are provided.
    /// </remarks>
    public class SqlTask
    {
        public string Sql { get; set; }

        public string[] Parameters { get; set; }
    }
}