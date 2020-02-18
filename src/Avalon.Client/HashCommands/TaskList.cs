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
            var result = Parser.Default.ParseArguments<TaskList.TaskListArguments>(CreateArgs(this.Parameters))
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

                    foreach (var task in App.MainWindow.ScheduledTasks.Tasks)
                    {
                        string commandType = "";

                        if (task.IsLua)
                        {
                            commandType = "Lua";
                        }
                        else
                        {
                            commandType = "Command";
                        }

                        this.Interpreter.Conveyor.EchoLog($"Scheduled to run after: {task.RunAfter.ToString()}", LogType.Information);
                        this.Interpreter.Conveyor.EchoLog($"Command type: {commandType}", LogType.Information);
                        this.Interpreter.Conveyor.EchoLog(task.Command, LogType.Information);
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported arguments for the #add-task hash command.
        /// </summary>
        public class TaskListArguments
        {
            [Option('c', "count", Required = false, HelpText = "The number of items in the scheduled task queue.")]
            public bool Count { get; set; }
        }

    }
}