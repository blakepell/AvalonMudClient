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
    /// Appends text to a file.
    /// </summary>
    public class AppendToFile : HashCommand
    {
        public AppendToFile(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#append-to-file";

        public override string Description { get; } = "Appends text to a file.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<AppendFileArguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    File.AppendAllText(o.File, o.Text);

                    if (o.AppendNewLine)
                    {
                        File.AppendAllText(o.File, Environment.NewLine);
                    }

                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this application.
        /// </summary>
        public class AppendFileArguments
        {
            [Option('f', "file", Required = true, HelpText = "The path to the file that should be appended to.")]
            public string File { get; set; }

            [Option('t', "text", Required = true, HelpText = "The text that should be appended to the file.")]
            public string Text { get; set; }

            [Option('n', "newline", Required = true, HelpText = "Whether or not to append a new line at the end of the text.")]
            public bool AppendNewLine { get; set; }

        }

    }
}