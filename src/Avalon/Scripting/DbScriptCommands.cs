using System.IO;
using System.Windows.Threading;
using Avalon.Common;
using Microsoft.Data.Sqlite;
using Mosaic.UI.Wpf.Controls.Scripting;

namespace Avalon.Scripting;

/// <summary>
/// The <c>db</c> object: a SQLite database scripts can use to store game data, ported from Continuum.
/// </summary>
/// <remarks>
/// Statements passed to <see cref="Execute"/> are queued and written together in one transaction every couple of
/// seconds (the role Continuum's SqlTasks plays), so a trigger that fires on every line stays cheap.  Parameters are
/// positional: @1, @2 and so on.
/// </remarks>
[ScriptModule(Name = "db", Description = "A SQLite database for script data. Parameters are @1, @2 ...")]
public sealed class DbScriptCommands
{
    private static readonly Lazy<DbScriptCommands> Shared = new(() => new DbScriptCommands());
    private readonly object _lock = new();
    private readonly List<(string Sql, string[] Parameters)> _pending = [];
    private readonly DispatcherTimer _timer;

    private DbScriptCommands()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += async (_, _) => await FlushAsync();
        _timer.Start();
    }

    /// <summary>
    /// Gets the database used by every terminal.
    /// </summary>
    public static DbScriptCommands Instance => Shared.Value;

    /// <summary>
    /// Gets the path of the database file.
    /// </summary>
    public static string DatabasePath => Path.Combine(AppPaths.AppDataFolder, "Avalon.db");

    /// <summary>
    /// Gets the connection string for the database file.
    /// </summary>
    public static string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();

    /// <summary>
    /// Gets the number of statements waiting to be written.
    /// </summary>
    public int PendingCount
    {
        get
        {
            lock (_lock)
                return _pending.Count;
        }
    }

    /// <summary>
    /// Queues a statement to run with the next batched write.
    /// </summary>
    public void Execute(string sql, params string[] parameters)
    {
        lock (_lock)
            _pending.Add((sql, parameters));
    }

    /// <summary>
    /// Runs a statement now.
    /// </summary>
    public void ExecuteImmediate(string sql, params string[] parameters)
    {
        using var connection = Open();
        using var command = CreateCommand(connection, sql, parameters);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Returns the first column of the first row for a statement without parameters.
    /// </summary>
    public object? SelectValue(string sql) => SelectValue(sql, []);

    /// <summary>
    /// Returns the first column of the first row.
    /// </summary>
    public object? SelectValue(string sql, params string[] parameters)
    {
        using var connection = Open();
        using var command = CreateCommand(connection, sql, parameters);
        var value = command.ExecuteScalar();
        return value is DBNull ? null : value;
    }

    /// <summary>
    /// Returns the first column of the first row asynchronously for a statement without parameters.
    /// </summary>
    public Task<object?> SelectValueAsync(string sql) => SelectValueAsync(sql, []);

    /// <summary>
    /// Returns the first column of the first row without blocking.
    /// </summary>
    public async Task<object?> SelectValueAsync(string sql, params string[] parameters)
    {
        await using var connection = Open();
        await using var command = CreateCommand(connection, sql, parameters);
        var value = await command.ExecuteScalarAsync();
        return value is DBNull ? null : value;
    }

    /// <summary>
    /// Returns rows for a statement without parameters.
    /// </summary>
    public List<Dictionary<string, object?>> Select(string sql) => Select(sql, []);

    /// <summary>
    /// Returns every row as a dictionary of column name to value.
    /// </summary>
    public List<Dictionary<string, object?>> Select(string sql, params string[] parameters)
    {
        using var connection = Open();
        using var command = CreateCommand(connection, sql, parameters);
        using var reader = command.ExecuteReader();
        var rows = new List<Dictionary<string, object?>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    /// <summary>
    /// Writes any queued statements now.
    /// </summary>
    public Task Flush() => FlushAsync();

    internal void FlushSynchronously() => Task.Run(FlushAsync).GetAwaiter().GetResult();

    /// <summary>
    /// Writes any queued statements in one transaction.
    /// </summary>
    public async Task FlushAsync()
    {
        (string Sql, string[] Parameters)[] batch;
        lock (_lock)
        {
            if (_pending.Count == 0)
            {
                return;
            }

            batch = _pending.ToArray();
            _pending.Clear();
        }

        await using var connection = Open();
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();
        foreach (var (sql, parameters) in batch)
        {
            try
            {
                await using var command = CreateCommand(connection, sql, parameters);
                command.Transaction = transaction;
                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex)
            {
                Mosaic.UI.Wpf.Logging.Logger.LogError($"db: {ex.Message} ({sql})");
            }
        }
        await transaction.CommitAsync();
    }

    private static SqliteConnection Open()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static SqliteCommand CreateCommand(SqliteConnection connection, string sql, string[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        for (int i = 0; i < parameters.Length; i++)
            command.Parameters.AddWithValue($"@{i + 1}", parameters[i] ?? (object)DBNull.Value);
        return command;
    }
}
