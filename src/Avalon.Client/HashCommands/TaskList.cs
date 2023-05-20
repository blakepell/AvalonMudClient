/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Lists the tasks that are currently scheduled.
    /// </summary>
    public class TaskList : HashCommand
    {
        public TaskList(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#task-list";

        public override string Description { get; } = "Lists the tasks that are currently scheduled.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    if (o.Count)
                    {
                        this.Interpreter.Conveyor.EchoLog($"There are {App.MainWindow.ScheduledTasks.Tasks.Count} items currently scheduled.", LogType.Information);
                        return;
                    }

                    if (App.MainWindow.ScheduledTasks.Tasks.Count == 0)
                    {
                        this.Interpreter.Conveyor.EchoLog("There are no scheduled tasks that are in the queue.", LogType.Information);
                        return;
                    }

                    var tb = new TableBuilder("Time", "Type", "Command");

                    foreach (var task in App.MainWindow.ScheduledTasks.Tasks)
                    {
                        tb.AddRow(task.RunAfter.ToString("hh:mm:ss tt"), task.IsLua ? "Lua" : "Command", task.Command.TrimEnd('\n').TrimEnd('r').TrimLengthWithEllipses(46));
                    }

                    this.Interpreter.Conveyor.EchoText(tb.ToString());
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported arguments for the #add-task hash command.
        /// </summary>
        private class Arguments
        {
            [Option('c', "count", Required = false, HelpText = "The number of items in the scheduled task queue.")]
            public bool Count { get; set; }
        }

    }
}