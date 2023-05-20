/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Echos back the parameter the user sends in an easy to see colored event line.
    /// </summary>
    public class EchoEvent : HashCommand
    {
        public EchoEvent(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#echo-event";

        public override string Description { get; } = "Echos back the full parameter string the caller passes in an easy to see event line.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var foregroundColor = Colorizer.ColorMapByName(o.Color);

                    if (foregroundColor != null)
                    {
                        Interpreter.Conveyor.EchoLog($"{foregroundColor.AnsiColor}{o.Text}{AnsiColors.Default}", o.EventType);
                    }
                    else
                    {
                        Interpreter.Conveyor.EchoLog($"{o.Text}", o.EventType);
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments the #beep hash command.
        /// </summary>
        private class Arguments
        {
            [Option('t', "type", Required = false, HelpText = "The type of event: [Information|Warning|Success|Error|Debug]")]
            public LogType EventType { get; set; } = LogType.Information;

            [Option('c', "color", Required = false, HelpText = "The name of the supported color the echo should rendered as.")]
            public string Color { get; set; }

            /// <summary>
            /// Returns the rest of the values that weren't parsed by switches into a list of strings.
            /// </summary>
            [Value(0, Min = 1, HelpText = "The text to echo to the terminal window.")]
            public IEnumerable<string> TextList { get; set; }

            /// <summary>
            /// Returns the left over values as a single string that doesn't have to be escaped by quotes.
            /// </summary>
            public string Text => string.Join(" ", TextList);
        }
    }
}