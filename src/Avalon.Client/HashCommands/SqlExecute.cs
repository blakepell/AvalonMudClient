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
    /// <remarks>
    /// This command requires developer mode to be turned on because it does not escape input.  When
    /// a user creates the SQL this isn't a problem, it's their database after all.  However, if they
    /// use this command in tandem with a trigger it becomes potentially problematic and that should
    /// be understood (a player name for instance with a quote in it would break a statement if used
    /// as a parameter.. those things should be escaped with the sqlite quote command for which I'll
    /// provide examples.  That approach can allow this to work while preventing injection exploits.
    /// But at some point if we're going to allow flexible developer features the dev has to understand
    /// some basics.
    /// </remarks>
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