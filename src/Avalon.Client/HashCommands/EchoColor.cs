using System;
using System.IO;
using Avalon.Colors;
using Avalon.Common.Interfaces;
using CommandLine;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends.
    /// </summary>
    public class EchoColor : HashCommand
    {
        public EchoColor(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#echo-color";

        public override string Description { get; } = "Echos the text to the terminal in the specified color.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<EchoColor.EchoColorArguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var foregroundColor = Colorizer.ColorMapByName(o.Color);

                    if (foregroundColor != null)
                    {
                        Interpreter.EchoText(o.Text, foregroundColor.AnsiColor, false);
                    }
                    else
                    {
                        Interpreter.Conveyor.EchoLog($"Color '{o.Color}' is not a valid color.", LogType.Warning);
                        Interpreter.EchoText($"{o.Text}");
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this application.
        /// </summary>
        public class EchoColorArguments
        {
            [Option('c', "color", Required = true, HelpText = "The name of the supported color the echo should rendered as.")]
            public string Color { get; set; }

            [Value(0, HelpText = "The text to echo to the terminal window.")]
            public string Text { get; set; }

        }

    }
}
