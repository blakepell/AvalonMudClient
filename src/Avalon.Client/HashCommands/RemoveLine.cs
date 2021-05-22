/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Removes a specific line in the game terminal.
    /// </summary>
    public class RemoveLine : HashCommand
    {
        public RemoveLine(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#remove-line";

        public override string Description { get; } = "Removes a line from the game terminal.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    if (o.EndLine == 0)
                    {
                        // Only a start line was specified, so remove it.
                        App.MainWindow.GameTerminal.RemoveLine(o.StartLine);
                    }
                    else
                    {
                        // A range was specified, remove them all.
                        App.MainWindow.GameTerminal.RemoveLine(o.StartLine, o.EndLine);
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        public class Arguments
        {
            [Value(0, Required = true, HelpText = "The starting line to remove (or the line to remove if no end line is specified).")]
            public int StartLine { get; set; } = 0;

            [Value(1, Required = false, HelpText = "The ending line in the range.")]
            public int EndLine { get; set; } = 0;
        }
    }
}