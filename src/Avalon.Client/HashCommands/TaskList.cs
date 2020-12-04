using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;
using Argus.Extensions;

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
                        this.Interpreter.Conveyor.EchoLog($"There are no scheduled tasks that are in the queue.", LogType.Information);
                        return;
                    }

                    var sb = Argus.Memory.StringBuilderPool.Take();
                    sb.AppendLine("");
                    sb.AppendLine("+--------------------------------------------------------------------------+");
                    sb.AppendLine("+ Time         | Type     | Command                                        +");
                    sb.AppendLine("+--------------------------------------------------------------------------+");

                    foreach (var task in App.MainWindow.ScheduledTasks.Tasks)
                    {
                        sb.AppendFormat("+ {0, -12} |", task.RunAfter.ToString("hh:mm:ss tt"));
                        sb.AppendFormat(" {0, -8} | ", task.IsLua ? "Lua" : "Command");
                        sb.AppendFormat("{0, -46} |\r\n", task.Command.TrimEnd('\n').TrimEnd('r').TrimLengthWithEllipses(46));
                    }

                    sb.AppendLine("+--------------------------------------------------------------------------+\r\n");

                    this.Interpreter.Conveyor.EchoText(sb.ToString());
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