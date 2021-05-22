/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Threading.Tasks;
using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Executes an alias.  The main purpose of this is to be able to short hand lookup aliases
    /// from the autocomplete box.  This will basically just send the alias text to the interpreter.
    /// </summary>
    public class Alias : HashCommand
    {
        public Alias(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#a";

        public override string Description { get; } = "Executes an alias.";

        public override void Execute()
        {
        }

        public override async Task ExecuteAsync()
        {
            var args = new AliasArguments();

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<AliasArguments>(CreateArgs(this.Parameters))
                               .WithParsed(o =>
                               {
                                   // Save the reference, we'll delay outside the lambda.
                                   args = o;
                               });

            if (!string.IsNullOrWhiteSpace(args.Alias))
            {
                if (args.DelayMilliseconds > 0)
                {
                    await Task.Delay(args.DelayMilliseconds);
                }

                Interpreter.Send(args.Alias);
            }

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// Arguments for the alias hash command.
        /// </summary>
        public class AliasArguments
        {
            [Option('d', "delay", Required = false, HelpText = "The delay in milliseconds this command should wait before executing.")]
            public int DelayMilliseconds { get; set; }

            [Value(0, Required = true)]
            public string Alias { get; set; }
        }

    }
}