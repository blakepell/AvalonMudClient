/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Data;
using Avalon.Common.Interfaces;
using CommandLine;
using System.Threading.Tasks;

namespace Avalon.HashCommands
{
    public class SqlDebug : HashCommand
    {
        public SqlDebug(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#sql-debug";

        public override string Description { get; } = "Info the state of the SQLite environment.";

        public override async Task ExecuteAsync()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var sb = Argus.Memory.StringBuilderPool.Take();

                    sb.AppendLine();
                    sb.Append("{CSQL{cite{x Environment Info:{x\r\n");
                    sb.Append("---------------------------------------------------------------------\r\n");
                    sb.AppendFormat(" {{G * {{WData Source:{{x          {{C{0}{{x\r\n", App.Settings.ProfileSettings.SqliteDatabase);
                    sb.AppendFormat(" {{G * {{WConnection Open:{{x      {{C{0}{{x\r\n", App.MainWindow.SqlTasks.Connection.State == ConnectionState.Open ? "Yes" : "No");
                    sb.AppendFormat(" {{G * {{WSQL Scripts Pending:{{x  {{C{0}{{x\r\n", App.MainWindow.SqlTasks.SqlQueue.Count);
                    sb.AppendFormat(" {{G * {{WSQL Scripts Run:{{x      {{C{0}{{x\r\n", App.MainWindow.SqlTasks.CommandsRun);
                    sb.AppendFormat(" {{G * {{WCommand Cache Count:{{x  {{C{0}{{x\r\n", App.MainWindow.SqlTasks.SqliteCommandCache.Count);
                    sb.AppendFormat(" {{G * {{WBatch Interval:{{x       {{C{0}s{{c{{x\r\n", App.MainWindow.SqlTasks.GetInterval());

                    this.Interpreter.Conveyor.EchoText(sb.ToString());

                    Argus.Memory.StringBuilderPool.Return(sb);
                });

            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        public class Arguments
        {

        }

    }
}