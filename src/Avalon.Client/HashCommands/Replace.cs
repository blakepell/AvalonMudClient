using Avalon.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Replaces all occurances of a string in a terminal window.
    /// </summary>
    public class Replace : HashCommand
    {
        public Replace(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#replace";

        public override string Description { get; } = "Replaces all occurances of one string with another.";

        public override void Execute()
        {
            // If no parameters echo the help.
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                this.Interpreter.Send("#replace --help");
                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var term = App.MainWindow.GameTerminal;

                    switch (o.Terminal)
                    {
                        case TerminalTarget.Terminal1:
                            term = App.MainWindow.Terminal1;
                            break;
                        case TerminalTarget.Terminal2:
                            term = App.MainWindow.Terminal2;
                            break;
                        case TerminalTarget.Terminal3:
                            term = App.MainWindow.Terminal3;
                            break;
                        case TerminalTarget.BackBuffer:
                            term = App.MainWindow.GameBackBufferTerminal;
                            break;
                    }

                    if (o.Colorize)
                    {
                        var sb = Argus.Memory.StringBuilderPool.Take(o.ReplaceWith);
                        Colorizer.MudToAnsiColorCodes(sb);
                        o.ReplaceWith = sb.ToString();
                        Argus.Memory.StringBuilderPool.Return(sb);
                    }

                    term.ReplaceAll(o.SearchText, o.ReplaceWith, false);
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class Arguments
        {

            [Value(0, Required = true, HelpText = "The text that should be replaced.")]
            public string SearchText { get; set; }

            [Value(1, Required = true, HelpText = "The text that should be put in place of the replaced text.")]
            public string ReplaceWith { get; set; }

            [Option('c', "colorize", Required = false, HelpText = "Whether or not this should colorize the text.  Turning off colorization speeds performance.")]
            public bool Colorize { get; set; } = true;

            /// <summary>
            /// What terminal should the echo be sent to.
            /// </summary>
            [Option('t', "term", Required = false, HelpText = "The terminal that shuld be echoed to.  The main terminal is the default if not specified.")]
            public TerminalTarget Terminal { get; set; } = TerminalTarget.Main;

        }

    }
}
