using System.IO;
using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Appends text to a file.
    /// </summary>
    public class AppendToFile : HashCommand
    {
        public AppendToFile(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#append-to-file";

        public override string Description { get; } = "Syntax: #append-to-file -f <filename> -t <text>";

        public override void Execute()
        {
            Parser.Default.ParseArguments<AppendFileArguments>(CreateArgs(this.Parameters))
                  .WithParsed(o =>
                  {
                      File.AppendAllText(o.File, o.Text);
                  })
                  .WithNotParsed(o =>
                  {
                          this.Interpreter.EchoText("[ Failure ] #append-to-file parameters are not correct.");
                          this.Interpreter.EchoText("[         ] Syntax: #append-to-file -f <filename> -t <text>");
                  });
        }

        /// <summary>
        /// The supported command line arguments for this application.
        /// </summary>
        public class AppendFileArguments
        {
            [Option('f', "file", Required = true, HelpText = "The path to the file that should be appended to.")]
            public string File { get; set; }

            [Option('t', "text", Required = true, HelpText = "The text that should be appended to the file.")]
            public string Text { get; set; }
        }

    }
}