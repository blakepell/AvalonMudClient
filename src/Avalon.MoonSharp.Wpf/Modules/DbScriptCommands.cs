/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Argus.Memory;
using Dapper;
using Microsoft.Data.Sqlite;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Database Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "db")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DbScriptCommands
    {
        /// <summary>
        /// The dispatch timer used for checking the queue.
        /// </summary>
        private readonly DispatcherTimer _timer = new(DispatcherPriority.Background);

        /// <summary>
        /// All pending scheduled.
        /// </summary>
        private List<SqlTask> SqlQueue { get; set; } = new();

        /// <summary>
        /// The SQLite Connection.
        /// </summary>
        private SqliteConnection Connection { get; set; }

        /// <summary>
        /// A SQLite command cache.
        /// </summary>
        private Dictionary<int, SqliteCommand> SqliteCommandCache { get; set; }

        /// <summary>
        /// The number of database commands that have been run since the creation of this object.
        /// </summary>
        public int CommandsRun { get; private set; } = 0;

        /// <summary>
        /// Whether a write operation is currently in process.
        /// </summary>
        public bool IsWriting { get; private set; } = false;

        [MoonSharpModuleMethod(Description = "Opens a SQLite database connection.",
            ParameterCount = 1)]
        public void Open(string connectionString)
        {
            this.Connection ??= new SqliteConnection(connectionString);

            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection = new SqliteConnection(connectionString);
                this.SqliteCommandCache = new Dictionary<int, SqliteCommand>();

                this.Connection.Open();

                _timer.Interval = TimeSpan.FromSeconds(10);
                _timer.Tick += this.SqlTasks_Tick!;
                _timer.IsEnabled = true;
            }
        }

        [MoonSharpModuleMethod(Description = "Closes a SQLite database connection and flushes and pending SQL statements.",
            ParameterCount = 1)]
        public void Close()
        {
            this.Flush();

            this.Connection.Close();
            this.SqliteCommandCache.Clear();

            _timer.IsEnabled = false;
            _timer.Tick -= this.SqlTasks_Tick!;
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
                await this.Connection.ExecuteAsync("BEGIN");

                // Run everything that isn't in a batch.
                foreach (var task in this.SqlQueue)
                {
                    var cmd = this.GetCachedSqliteCommand(task.Sql);

                    for (int i = 0; i < task.Parameters.Length; i++)
                    {
                        cmd.Parameters[i].Value = task.Parameters[i];
                    }

                    await cmd.ExecuteNonQueryAsync();
                    this.CommandsRun++;
                }

                this.SqlQueue.Clear();
                await this.Connection.ExecuteAsync("COMMIT");
            }
            catch (Exception ex)
            {
                await this.Connection.ExecuteAsync("ROLLBACK");
            }
            finally
            {
                this.SqlQueue.Clear();
                this.IsWriting = false;
            }

            sw.Stop();
        }

        /// <summary>
        /// Adds a SQL task to the buffered Queue.
        /// </summary>
        /// <param name="sql"></param>
        [MoonSharpModuleMethod(Description = "Adds a SQL statement to the buffered queue.",
            ParameterCount = 2)]
        public void ExecuteBuffered(string sql)
        {
            var sqlTask = new SqlTask { Sql = sql, Parameters = Array.Empty<string>() };
            this.SqlQueue.Add(sqlTask);

            // There is something in the timer, make sure it's enabled.
            _timer.IsEnabled = true;
        }

        /// <summary>
        /// Adds a SQL task to the buffered Queue.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [MoonSharpModuleMethod(Description = "Adds a parameterized SQL statement to the buffered queue.",
            ParameterCount = 2)]
        public void ExecuteBuffered(string sql, string[] parameters)
        {
            var sqlTask = new SqlTask { Sql = sql, Parameters = parameters };
            this.SqlQueue.Add(sqlTask);

            // There is something in the timer, make sure it's enabled.
            _timer.IsEnabled = true;
        }

        /// <summary>
        /// Executes a SQL statement immediately.
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
        {
            this.Execute(sql, Array.Empty<string>());
        }

        /// <summary>
        /// Adds a SQL task.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [MoonSharpModuleMethod(Description = "Executes parameterized SQL statement immediately",
            ParameterCount = 2)]
        public void Execute(string sql, string[] parameters)
        {
            var cmd = this.GetCachedSqliteCommand(sql);

            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters[i].Value = parameters[i];
            }

            cmd.ExecuteNonQuery();
            this.CommandsRun++;
        }

        /// <summary>
        /// Returns a new or cached database command for a SQL statement.  The SQL statement
        /// provided is put into the command as part of this function.
        /// </summary>
        /// <param name="sql"></param>
        private SqliteCommand GetCachedSqliteCommand(string sql)
        {
            int count = sql.Count(c => c == '@');
            var cmd = this.GetCachedSqliteCommand(count);

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
        [MoonSharpModuleMethod(Description = "Clears the cached SQLite commands.",
            ParameterCount = 0)]
        public void ClearCache()
        {
            if (this.SqliteCommandCache.Count == 0)
            {
                return;
            }

            // Dispose of all of the cached SQL Commands
            foreach (var item in this.SqliteCommandCache)
            {
                item.Value?.Dispose();
            }

            // Clear the dictionary.
            this.SqliteCommandCache.Clear();
        }

        /// <summary>
        /// Clears any pending database operations that are in the queue.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Clears any pending SQL statements from the buffered queue.",
            ParameterCount = 0)]
        public void ClearQueue()
        {
            this.SqlQueue.Clear();
            _timer.IsEnabled = false;
        }

        /// <summary>
        /// Flushes any pending database operations.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Executes all pending SQL statements in the buffered queue.",
            ParameterCount = 0)]
        public void Flush()
        {
            this.SqlTasks_Tick(null, null);
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
        [MoonSharpModuleMethod(Description = "Select a single (scalar) value from the database and returns it as an object.",
            ParameterCount = 2)]
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
        [MoonSharpModuleMethod(Description = "Executes a parameterized SQL statement and returns a dictionary result set.",
            ReturnTypeHint = "Dictionary",
            ParameterCount = 2)]
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
        [MoonSharpModuleMethod(Description = "Sets the interval in seconds that the buffered queue should run at.",
            ParameterCount = 1)]
        public void SetInterval(int seconds)
        {
            _timer.Interval = TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Returns the number of seconds between intervals in the batch SQLite tasks.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Returns the number of seconds between intervals in the buffered SQLite queue.",
            ParameterCount = 0)]
        public int GetInterval()
        {
            return (int)_timer.Interval.TotalSeconds;
        }

        /// <summary>
        /// Disposes of any resources used by the class.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Closes and disposes of all database resources.",
            ParameterCount = 0)]
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