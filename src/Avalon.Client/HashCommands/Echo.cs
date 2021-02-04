using System;
using System.Collections.Generic;
using System.Linq;
using Argus.Extensions;
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
        public Echo(IInterpreter interp) : base(interp)
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

                    if (!string.IsNullOrWhiteSpace(o.WindowName))
                    {
                        // This case is if they specified a window that might exist, we'll find it, edit that.
                        var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == WindowType.TerminalWindow && x.Name.Equals(o.WindowName, StringComparison.Ordinal)) as TerminalWindow;

                        if (win == null)
                        {
                            return;
                        }

                        var line = App.LineMemoryPool.Get();
                        line.FormattedText.AppendFormatIf(o.Timestamp, "[{0}]: ", Utilities.Utilities.Timestamp());
                        line.FormattedText.Append(o.Text);
                        line.FormattedText.AppendFormatIf(!o.NoLineBreak, "\r\n");

                        Colorizer.MudToAnsiColorCodes(line.FormattedText);

                        if (foregroundColor != null)
                        {
                            line.FormattedText.Insert(0, foregroundColor.AnsiColor.AnsiCode);

                            if (o.Reverse)
                            {
                                line.FormattedText.Insert(0, AnsiColors.Reverse.AnsiCode);
                            }
                        }
                        else
                        {
                            line.ForegroundColor = AnsiColors.Default;
                        }

                        win.AppendText(line);
                        App.LineMemoryPool.Return(line);

                        return;
                    }

                    string text = o.Timestamp ? $"[{Utilities.Utilities.Timestamp()}]: {o.Text}" : o.Text;

                    // This is the default echo implementation.
                    if (foregroundColor != null)
                    {
                        Interpreter.EchoText(text, foregroundColor.AnsiColor, o.Reverse, o.Terminal, false);
                    }
                    else
                    {
                        Interpreter.EchoText(text, AnsiColors.Default, false, o.Terminal, true);
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
            /// <summary>
            /// The color of the echo.
            /// </summary>
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

            /// <summary>
            /// Whether the color should be reversed.
            /// </summary>
            [Option('r', "reverse", Required = false, HelpText = "Whether or not to reverse the colors.  Only works when a valid color is specified.")]
            public bool Reverse { get; set; } = false;

            /// <summary>
            /// What terminal should the echo be sent to.
            /// </summary>
            [Option('t', "term", Required = false, HelpText = "The terminal that should be echoed to.  The main terminal is the default if not specified.")]
            public TerminalTarget Terminal { get; set; } = TerminalTarget.Main;

            /// <summary>
            /// What terminal window should the echo be sent to.
            /// </summary>
            [Option('w', "window", Required = false, HelpText = "The name of the user created terminal window that the echo should be sent to.")]
            public string WindowName { get; set; } = "";

            /// <summary>
            /// Includes a timestamp at the front of the line.
            /// </summary>
            [Option('d', "datetime", Required = false, HelpText = "Whether a timestamp should precede the echo.")]
            public bool Timestamp { get; set; } = false;

            /// <summary>
            /// Forces a line break if one doesn't already exist.
            /// </summary>
            [Option('b', "nolinebreak", Required = false, HelpText = "Skips adding a line break if that is possible in the scenario which this is called.")]
            public bool NoLineBreak { get; set; } = false;
        }

    }
}
