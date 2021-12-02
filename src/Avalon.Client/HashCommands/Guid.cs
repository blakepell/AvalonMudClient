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
    /// Echos a unique Guid.
    /// </summary>
    /// <remarks>
    /// Named as to not conflict with System.Guid.
    /// </remarks>
    public class GuidHashCmd : HashCommand
    {
        public GuidHashCmd(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#guid";

        public override string Description { get; } = "Echos a unique guid as well as settings a variable named guid so that it can be used via scripts.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    if (o.Count > 100)
                    {
                        Interpreter.Conveyor.EchoLog("The count is limited to 100 Guids at a time.", Common.Models.LogType.Warning);
                    }

                    var sb = Argus.Memory.StringBuilderPool.Take();

                    // If more than one guid is generated we're only saving the last one into a variable.
                    for (int i = 0; i < o.Count; i++)
                    {
                        string guid = Guid.NewGuid().ToString();

                        if (i == o.Count - 1)
                        {
                            this.Interpreter.Conveyor.SetVariable(o.VariableName, guid);
                        }

                        sb.AppendFormat("\r\n{0}", guid);
                    }

                    sb.Append("\r\n");

                    Interpreter.Conveyor.EchoText(sb.ToString());
                    Argus.Memory.StringBuilderPool.Return(sb);

                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class Arguments
        {
            /// <summary>
            /// Adds the number to the variable if the variable is a number.
            /// </summary>
            [Option('c', "count", Required = false, HelpText = "The number of GUIDs to generate.  If not set the default is 1.")]
            public int Count { get; set; } = 1;


            [Option('n', "name", Required = false, HelpText = "The name of the variable to set, the default is 'Guid'")]
            public string VariableName { get; set; } = "Guid";
        }

    }
}
