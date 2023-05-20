/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Hash command that interacts with custom variables.
    /// </summary>
    public class Var : HashCommand
    {
        public Var(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#var";

        public override string Description { get; } = "Allows interaction with traditional variables.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    string currentValue = this.Interpreter.Conveyor.GetVariable(o.Name) ?? "";

                    // There is no value, but the caller wants to do a execute a math operation
                    // on the value, let's set the initial value to 0.
                    if (string.IsNullOrWhiteSpace(currentValue) &&
                        (o.Add > 0 || o.Subtract > 0 || o.Increment || o.Decrement))
                    {
                        currentValue = "0";
                    }

                    if (o.Add > 0 && currentValue.IsNumeric())
                    {
                        int newValue = int.Parse(currentValue) + o.Add;
                        this.Interpreter.Conveyor.SetVariable(o.Name, newValue.ToString());
                    }

                    if (o.Subtract > 0 && currentValue.IsNumeric())
                    {
                        int newValue = int.Parse(currentValue) - o.Subtract;
                        this.Interpreter.Conveyor.SetVariable(o.Name, newValue.ToString());
                    }

                    if (o.Increment && currentValue.IsNumeric())
                    {
                        int newValue = int.Parse(currentValue) + 1;
                        this.Interpreter.Conveyor.SetVariable(o.Name, newValue.ToString());
                    }

                    if (o.Decrement && currentValue.IsNumeric())
                    {
                        int newValue = int.Parse(currentValue) - 1;
                        this.Interpreter.Conveyor.SetVariable(o.Name, newValue.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(o.Set))
                    {
                        this.Interpreter.Conveyor.SetVariable(o.Name, o.Set);
                    }

                    if (o.Echo)
                    {
                        // Get the variable again in case it has changed.
                        currentValue = this.Interpreter.Conveyor.GetVariable(o.Name) ?? "";
                        this.Interpreter.Conveyor.EchoText(currentValue);
                    }

                    if (o.Delete)
                    {
                        this.Interpreter.Conveyor.RemoveVariable(o.Name);
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
            /// <summary>
            /// Adds the number to the variable if the variable is a number.
            /// </summary>
            [Option('a', "add", Required = false, HelpText = "Adds the number to the variable if the variable is a number.")]
            public int Add { get; set; } = 0;

            /// <summary>
            /// Subtracts the number to the variable if the variable is a number.
            /// </summary>
            [Option('s', "subtract", Required = false, HelpText = "Subtracts the number to the variable if the variable is a number.")]
            public int Subtract { get; set; } = 0;

            /// <summary>
            /// Subtracts the number to the variable if the variable is a number.
            /// </summary>
            [Option('i', "increment", Required = false, HelpText = "Increments the variable by 1 if the variable is a number.")]
            public bool Increment { get; set; }

            /// <summary>
            /// Subtracts the number to the variable if the variable is a number.
            /// </summary>
            [Option('d', "decrement", Required = false, HelpText = "Decrements the number to the variable if the variable is a number.")]
            public bool Decrement { get; set; }

            [Option("delete", Required = false, HelpText = "Deletes the specified variable.")]
            public bool Delete { get; set; }

            /// <summary>
            /// Whether or not to echo out the final value (or the current value if no other operations are specified).
            /// </summary>
            [Option('e', "echo", Required = false, HelpText = "Echos the final value after any operations are run against the variable.")]
            public bool Echo { get; set; }

            /// <summary>
            /// The name of the variable.
            /// </summary>
            [Option('n', "name", Required = true, HelpText = "The name of the variable.")]
            public string Name { get; set; }

            /// <summary>
            /// The name of the variable.
            /// </summary>
            [Option("set", Required = false, HelpText = "The specific value to set.")]
            public string Set { get; set; }

        }

    }
}