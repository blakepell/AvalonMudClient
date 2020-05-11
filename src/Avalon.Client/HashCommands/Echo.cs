using System;
using System.Collections.Generic;
using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends.
    /// </summary>
    public class Echo : HashCommand
    {
        public Echo(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#echo";

        public override string Description { get; } = "Echos the text to the terminal in the specified color.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<EchoArguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var foregroundColor = Colorizer.ColorMapByName(o.Color);
                    string timeStamp = "";

                    if (o.Timestamp)
                    {
                        switch (App.Settings.AvalonSettings.TimestampFormat)
                        {
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutes:
                                timeStamp = $"[{DateTime.Now.ToString("hh:mm tt")}]: ";
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutesSeconds:
                                timeStamp = $"[{DateTime.Now.ToString("hh:mm:ss tt")}]: ";
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.OSDefault:
                                timeStamp = $"[{DateTime.Now.ToString("g")}]: ";
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.TwentyFourHour:
                                timeStamp = $"[{DateTime.Now.ToString("HH:mm:ss")}]: ";
                                break;
                        }
                    }

                    string text = $"{timeStamp}{o.Text}";

                    if (foregroundColor != null)
                    {
                        Interpreter.EchoText(text, foregroundColor.AnsiColor, o.Reverse, o.Terminal);
                    }
                    else
                    {
                        Interpreter.EchoText($"{text}", AnsiColors.Default, false, o.Terminal);
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this application.
        /// </summary>
        public class EchoArguments
        {
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

            [Option('r', "reverse", Required = false, HelpText = "Whether or not to reverse the colors.  Only works when a valid color is specified.")]
            public bool Reverse { get; set; } = false;

            /// <summary>
            /// What terminal should be echoed to.
            /// </summary>
            [Option('t', "term", Required = false, HelpText = "The terminal that shuld be echoed to.  The main terminal is the default if not specified.")]
            public TerminalTarget Terminal { get; set; } = TerminalTarget.Main;

            [Option('d', "datetime", Required = false, HelpText = "Whether a timestamp should preceed the echo.")]
            public bool Timestamp { get; set; } = false;
        }

    }
}
