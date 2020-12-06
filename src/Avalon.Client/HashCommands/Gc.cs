using System;
using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Force a garbage collection to run on the memory.
    /// </summary>
    public class Gc : HashCommand
    {
        public Gc(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#gc";

        public override string Description { get; } = "Force garbage collection to run on the memory.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                               .WithParsed(o =>
                               {
                                   try
                                   {
                                       GC.Collect();
                                       GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                                       GC.WaitForPendingFinalizers();

                                       if (!o.Silent)
                                       {
                                           Interpreter.Conveyor.EchoSuccess("Garbage Collection Executed");
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       Interpreter.Conveyor.EchoError($"Error running garbage collection: {ex.Message}");
                                   }
                               });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        public class Arguments
        {
            [Option('s', "silent", Required = false, HelpText = "Whether to run silently.")]
            public bool Silent { get; set; }
        }

    }
}
