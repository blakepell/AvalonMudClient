/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Avalon.Timers
{
    /// <summary>
    /// SQLite gatekeeper.
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
    /// cohesive transactions to be grouped together.  The downside is, unrelated commands could stop fail and stop
    /// commits that otherwise would have succeeded.  But, once someone's scripts are solid that won't really
    /// be an issue.
    /// 
    /// Because the commands are parameterized by index (@1, @2, @3) we are going to cache the SqlCommand's and
    /// re-use them (a cached SQLCommand with parameters will exist by the number of params passed in).  As a result
    /// the cache will never grow that large and will have a high re-use rate.
    /// </remarks>
    public class SqlTasks : IDisposable
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
                return;
            }

            this.IsWriting = true;

            var sw = new Stopwatch();
            sw.Start();

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
                await Connection.ExecuteAsync("ROLLBACK");
                App.Conveyor.EchoError(ex.Message);
            }
            finally
            {
                this.SqlQueue.Clear();
                App.MainWindow.ViewModel.PendingSqlStatements = 0;
                this.IsWriting = false;
            }

            sw.Stop();

            // I don't necessarily want to spam the user but if the batches are taking this long I think
            // a warning message is appropriate.
            if (sw.ElapsedMilliseconds > 5000)
            {
                App.Conveyor.EchoWarning($"Performance: Database batch took {sw.ElapsedMilliseconds.ToString()}ms.");
            }

            if (App.Settings.AvalonSettings.Debug)
            {
                if (sw.ElapsedMilliseconds < 500)
                {
                    App.Conveyor.EchoSuccess($"SQL batch committed in {sw.ElapsedMilliseconds.ToString()}ms");
                }
                else
                {
                    App.Conveyor.EchoWarning($"SQL batch committed in {sw.ElapsedMilliseconds.ToString()}ms");
                }
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
        /// Returns a new or cached database command for a SQL statement.  The SQL statement
        /// provided is put into the command as part of this function.
        /// </summary>
        /// <param name="sql"></param>
        private SqliteCommand GetCachedSqliteCommand(string sql)
        {
            int count = sql.Count(c => c == '@');
            var cmd = GetCachedSqliteCommand(count);

            // Save an allocation and only change the string if it needs to be changed.
            if (cmd.CommandText != sql)
            {
                cmd.CommandText = sql;
            }

            return cmd;
        }

        /// <summary>
        /// Returns a new or cached database command for the specified number of parameters.  Note:  The
        /// caller is responsible for adding the SQL statement into the returned object.
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
                    ParameterName = $"@{i.ToString()}"
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

            // Dispose of all of the cached SqliteCommand objects.
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
        /// Opens a connection to a database provided via a new connection string.  Any resources
        /// from previous connections if they exist will be released.
        /// </summary>
        /// <param name="connectionString"></param>
        public async Task OpenAsync(string connectionString)
        {
            this.ClearCache();

            if (this.Connection != null)
            {
                await this.Connection.CloseAsync();
                await this.Connection.DisposeAsync();
            }

            this.Connection = new SqliteConnection(connectionString);
            await this.Connection.OpenAsync();
        }

        /// <summary>
        /// Executes a single SQL non query command outside of a transaction.  Required for things
        /// like creating tables.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public async Task ExecuteNonQueryAsync(string sql, params string[] parameters)
        {
            // Get a new command for a select so it doesn't overlap with any writes using the cached pool.
            using (var cmd = this.Connection.CreateCommand())
            {
                cmd.CommandText = sql;

                for (int i = 0; i < parameters.Length; i++)
                {
                    _ = cmd.Parameters.AddWithValue($"@{(i + 1).ToString()}", parameters[i]);
                }

                await cmd.ExecuteNonQueryAsync();
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
                    cmd.Parameters.AddWithValue($"@{(i + 1).ToString()}", parameters[i]);
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
                    cmd.Parameters.AddWithValue($"@{(i + 1).ToString()}", parameters[i]);
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

        /// <summary>
        /// Returns the number of seconds between intervals in the batch SQLite tasks.
        /// </summary>
        /// <returns></returns>
        public int GetInterval()
        {
            return (int)_timer.Interval.TotalSeconds;
        }

        /// <summary>
        /// Disposes of any resources used by the class.
        /// </summary>
        public void Dispose()
        {
            this.ClearCache();
            this.ClearQueue();
            this.Connection?.Close();
            this.Connection?.Dispose();
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
        /// <summary>
        /// The parameterized SQL command that should be run.  All parameters will be numeric
        /// by index (e.g. @1, @2, @3, etc.).
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// The parameters specified in the order by index start at 1.
        /// </summary>
        public string[] Parameters { get; set; }
    }
}