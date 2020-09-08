using System;
using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Adds a scheduled task (command or Lua) to be executed after a designated time.
    /// </summary>
    public class TaskAdd : HashCommand
    {
        public TaskAdd(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#task-add";

        public override string Description { get; } = "Adds a scheduled task (command or Lua) to be executed after a designated time.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    App.MainWindow.ScheduledTasks.AddTask(o.Command, o.IsLua, DateTime.Now.AddSeconds(o.Seconds));
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported arguments for the #add-task hash command.
        /// </summary>
        private class Arguments
        {
            [Option('c', "command", Required = true, HelpText = "The command or Lua that should be sent or executed.")]
            public string Command { get; set; }

            [Option('l', "lua", Required = false, HelpText = "Whether the command should be executed as Lua.")]
            public bool IsLua { get; set; } = false;

            [Option('s', "seconds", Required = true, HelpText = "The number of seconds the task should wait before executing.")]
            public int Seconds  { get; set; } = 1;
        }

    }
}