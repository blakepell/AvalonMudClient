using Argus.Extensions;
using Avalon.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using CommandLine;
using System.Threading.Tasks;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Hash command that will pulse supported controls with a quick fade in and fade
    /// out color animation.
    /// </summary>
    public class Pulse : HashCommand
    {
        public Pulse(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }


        public override string Name { get; } = "#pulse";

        public override string Description { get; } = "Pulses a supported control with a fade in and fade out color animation.";

        public override async Task ExecuteAsync()
        {
            // No argument clears all terminals.
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                App.MainWindow.TextInput.Pulse(System.Windows.Media.Colors.Red);
                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<PulseArguments>(CreateArgs(this.Parameters))
                               .WithParsed(o =>
                               {
                                   int duration = 1000;

                                   if (o.DurationMilliseconds > 0)
                                   {
                                       duration = Argus.Math.MathUtilities.Clamp(o.DurationMilliseconds, 0, 5000);
                                   }

                                   if (!string.IsNullOrWhiteSpace(o.Color))
                                   {
                                       var color = Colorizer.ColorMapByName(o.Color);

                                       if (color != null)
                                       {
                                           App.MainWindow.TextInput.Pulse(color.Brush.Color, duration);
                                       }
                                       else
                                       {
                                           Interpreter.Conveyor.EchoLog("#pulse received an invalid color.", LogType.Error);
                                       }
                                   }
                               });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        public override void Execute()
        {
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class PulseArguments
        {
            [Option('c', "color", Required = false, HelpText = "A known color.")]
            public string Color { get; set; }

            [Option('d', "duration", Required = false, HelpText = "The duration in milliseconds.  Default is 1000 or 1 second.  Maximum duration is 5 seconds.")]
            public int DurationMilliseconds { get; set; }
        }
    }
}