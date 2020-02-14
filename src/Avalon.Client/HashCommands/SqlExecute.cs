using System;
using System.Threading.Tasks;
using Argus.Extensions;
using Avalon.Common.Interfaces;
using Microsoft.Data.Sqlite;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Executes a SQL statement with no result set.
    /// </summary>
    public class SqlExecute : HashCommand
    {
        public SqlExecute(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#sql-execute";

        public override string Description { get; } = "Executes a SQL statement with no result set.";

        public override void Execute()
        {

        }

        public override async Task ExecuteAsync()
        {
            if (!App.Settings.AvalonSettings.DeveloperMode)
            {
                Interpreter.Conveyor.EchoLog("#sql-execute can only be run in developer mode.", Common.Models.LogType.Error);
                return;
            }

            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.EchoLog("Syntax: #sql-execute <SQL statement>", Common.Models.LogType.Information);
                return;
            }

            try
            {
                // This allow the user to run arbitrary SQL from a trigger or alias.  It is not parameterized due to
                // the nature of how the user will input it.  Developer mode must be turned on to use this hash command.
                await using (var conn = new SqliteConnection($"Data Source={App.Settings.ProfileSettings.SqliteDatabase}"))
                {
                    await conn.OpenAsync();

                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = this.Parameters;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                this.Interpreter.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);
            }

        }
    }
}