using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Whether the client has a TCP/IP connection open.
    /// </summary>
    /// <remarks>
    /// This can determine if a connection is open or closed (if a connection has broken but the disconnect event hasn't fired).
    /// </remarks>
    public class IsConnected : HashCommand
    {
        public IsConnected(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#is-connected";

        public override string Description { get; } = "Whether the client currently has an open TCP/IP connection.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    string value = this.Interpreter?.Telnet?.IsConnected().ToString() ?? "False";
                    this.Interpreter.Conveyor.SetVariable(o.VariableName, value);

                    if (!o.Silent)
                    {
                        this.Interpreter.Conveyor.EchoText($"{value}\r\n");
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class Arguments
        {
            [Option('s', "silent", Required = false, HelpText = "Whether the value should be echoed to the main terminal or not.")]
            public bool Silent { get; set; } = false;

            [Option('n', "name", Required = false, HelpText = "Puts the result into a variable.  The default variable name is 'IsConnected'.")]
            public string VariableName { get; set; } = "IsConnected";
        }

    }
}