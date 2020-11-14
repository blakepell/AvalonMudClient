using System;
using System.Threading.Tasks;
using Argus.Extensions;
using Avalon.Common.Interfaces;
using CommandLine;
using Microsoft.Data.Sqlite;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Executes a SQL statement with scaler result.
    /// </summary>
    public class SqlQueryOne : HashCommand
    {
        public SqlQueryOne(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#sql-query-one";

        public override string Description { get; } = "Executes a scaler SQL statement which returns one value.";

        public override void Execute()
        {

        }

        public override async Task ExecuteAsync()
        {
            if (!App.Settings.AvalonSettings.DeveloperMode)
            {
                Interpreter.Conveyor.EchoLog("#sql-query-one can only be run in developer mode.", Common.Models.LogType.Error);
                return;
            }

            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.EchoLog("Syntax: #sql-queryone <SQL statement> <parameters>", Common.Models.LogType.Information);
                return;
            }

            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(async o =>
                {
                    try
                    {
                        // This allow the user to run arbitrary SQL from a trigger or alias.  This can execute raw SQL
                        // or can be parameterized by ordinal, @1, @2, @3, @4 corresponding with the position passed in after the SQL.
                        await using var conn = new SqliteConnection($"Data Source={App.Settings.ProfileSettings.SqliteDatabase}");
                        await conn.OpenAsync();
                        await using var cmd = conn.CreateCommand();

                        cmd.CommandText = o.Sql;

                        if (!string.IsNullOrWhiteSpace(o.Parameter1))
                        {
                            cmd.AddWithValue("@1", o.Parameter1);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter2))
                        {
                            cmd.AddWithValue("@2", o.Parameter2);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter3))
                        {
                            cmd.AddWithValue("@3", o.Parameter3);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter4))
                        {
                            cmd.AddWithValue("@4", o.Parameter4);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter5))
                        {
                            cmd.AddWithValue("@5", o.Parameter5);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter6))
                        {
                            cmd.AddWithValue("@6", o.Parameter6);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter7))
                        {
                            cmd.AddWithValue("@7", o.Parameter7);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter8))
                        {
                            cmd.AddWithValue("@8", o.Parameter8);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter9))
                        {
                            cmd.AddWithValue("@9", o.Parameter9);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter10))
                        {
                            cmd.AddWithValue("@10", o.Parameter10);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter11))
                        {
                            cmd.AddWithValue("@11", o.Parameter11);
                        }

                        if (!string.IsNullOrWhiteSpace(o.Parameter12))
                        {
                            cmd.AddWithValue("@12", o.Parameter12);
                        }

                        string result = (string)(await cmd.ExecuteScalarAsync());

                        // If the user wants to save it into a Variable for use elsewhere do that.
                        if (!string.IsNullOrWhiteSpace(o.VariableName))
                        {
                            this.Interpreter?.Conveyor.SetVariable(o.VariableName, result);
                        }

                        if (!o.Silent)
                        {
                            this.Interpreter.EchoText((string)result);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Interpreter.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        private class Arguments
        {
            [Value(0, Required = true, HelpText = "The SQL to execute.")]
            public string Sql { get; set; } = "";
            
            [Value(1, Required = false, HelpText = "Parameter 1")]
            public string Parameter1 { get; set; } = "";

            [Value(2, Required = false, HelpText = "Parameter 2")]
            public string Parameter2 { get; set; } = "";

            [Value(3, Required = false, HelpText = "Parameter 3")]
            public string Parameter3 { get; set; } = "";

            [Value(4, Required = false, HelpText = "Parameter 4")]
            public string Parameter4 { get; set; } = "";

            [Value(5, Required = false, HelpText = "Parameter 5")]
            public string Parameter5 { get; set; } = "";

            [Value(6, Required = false, HelpText = "Parameter 6")]
            public string Parameter6 { get; set; } = "";

            [Value(7, Required = false, HelpText = "Parameter 7")]
            public string Parameter7 { get; set; } = "";

            [Value(8, Required = false, HelpText = "Parameter 8")]
            public string Parameter8 { get; set; } = "";

            [Value(9, Required = false, HelpText = "Parameter 9")]
            public string Parameter9 { get; set; } = "";

            [Value(10, Required = false, HelpText = "Parameter 10")]
            public string Parameter10 { get; set; } = "";

            [Value(11, Required = false, HelpText = "Parameter 11")]
            public string Parameter11 { get; set; } = "";

            [Value(12, Required = false, HelpText = "Parameter 12")]
            public string Parameter12 { get; set; } = "";


            [Option('n', "name", Required = false, HelpText = "If set, the name of the Variable the result should be put in.")]
            public string VariableName { get; set; } = "";


            [Option('s', "silent", Required = false, HelpText = "If set the value will not be echod to the terminal.")]
            public bool Silent { get; set; } = false;

        }

    }
}