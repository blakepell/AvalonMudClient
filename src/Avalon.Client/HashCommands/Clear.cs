/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Hash command that clears the specified terminal windows.
    /// </summary>
    public class Clear : HashCommand
    {
        public Clear(IInterpreter interp) : base(interp)
        {
        }


        public override string Name { get; } = "#clear";

        public override string Description { get; } = "Clears the specified terminal windows.";

        public override void Execute()
        {
            // No argument clears all terminals.
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Main);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.BackBuffer);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal1);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal2);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal3);

                App.MainWindow.CustomTab1Badge.Value = 0;
                App.MainWindow.CustomTab2Badge.Value = 0;
                App.MainWindow.CustomTab3Badge.Value = 0;

                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<ClearArguments>(CreateArgs(this.Parameters))
                               .WithParsed(o =>
                               {
                                   if (o.Main)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Main);                                       
                                   }

                                   if (o.BackBuffer)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.BackBuffer);
                                   }

                                   if (o.Term1)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal1);
                                       App.MainWindow.CustomTab1Badge.Value = 0;
                                   }

                                   if (o.Term2)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal2);
                                       App.MainWindow.CustomTab2Badge.Value = 0;
                                   }

                                   if (o.Term3)
                                   {
                                       App.MainWindow.CustomTab3Badge.Value = 0;
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal3);
                                   }

                                   if (o.AutoComplete)
                                   {
                                       ((Interpreter) this.Interpreter).InputAutoCompleteKeywords.Clear();
                                   }
                               });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for the #clear hash command.
        /// </summary>
        public class ClearArguments
        {
            [Option('m', "main", Required = false, HelpText = "Clear the main terminal.")]
            public bool Main { get; set; }

            [Option('b', "back", Required = false, HelpText = "Clear the main terminal back buffer.")]
            public bool BackBuffer { get; set; }

            [Option('1', "term1", Required = false, HelpText = "Clears terminal 1")]
            public bool Term1 { get; set; }

            [Option('2', "term2", Required = false, HelpText = "Clears terminal 2")]
            public bool Term2 { get; set; }

            [Option('3', "term3", Required = false, HelpText = "Clears terminal 3.")]
            public bool Term3 { get; set; }

            [Option('a', "auto-complete", Required = false, HelpText = "Clears the auto complete list.")]
            public bool AutoComplete { get; set; }

        }
    }
}
