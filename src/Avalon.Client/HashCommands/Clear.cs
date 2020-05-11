﻿using Argus.Extensions;
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
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Communication);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.OutOfCharacterCommunication);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.BackBuffer);
                Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal3);

                App.MainWindow.Panel2Badge.Value = 0;
                App.MainWindow.Panel3Badge.Value = 0;
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

                                   if (o.Comm)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Communication);
                                       App.MainWindow.Panel2Badge.Value = 0;
                                   }

                                   if (o.Ooc)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.OutOfCharacterCommunication);
                                       App.MainWindow.Panel3Badge.Value = 0;
                                   }

                                   if (o.BackBuffer)
                                   {
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.BackBuffer);
                                   }

                                   if (o.Term3)
                                   {
                                       App.MainWindow.CustomTab3Badge.Value = 0;
                                       Interpreter.Conveyor.ClearTerminal(TerminalTarget.Terminal3);
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

            [Option('o', "ooc", Required = false, HelpText = "Clear out of character (OOC) terminal.")]
            public bool Ooc { get; set; }

            [Option('c', "comm", Required = false, HelpText = "Clear the communication (IC) terminal.")]
            public bool Comm { get; set; }

            [Option('b', "back", Required = false, HelpText = "Clear the main terminal back buffer.")]
            public bool BackBuffer { get; set; }


            [Option('3', "term3", Required = false, HelpText = "Clears terminal 3.")]
            public bool Term3 { get; set; }


        }
    }
}
