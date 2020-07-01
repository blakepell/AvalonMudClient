using Avalon.Common.Interfaces;
using CommandLine;
using System;
using System.Linq;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Options to update a compass window.
    /// </summary>
    public class Compass : HashCommand
    {
        public Compass(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#compass";

        public override string Description { get; } = "Options to update a compass window.";

        public override void Execute()
        {
            // If no parameters echo the help.
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == Common.Models.WindowType.CompassWindow && x.Name.Equals("Compass", StringComparison.Ordinal)) as CompassWindow;

                if (win == null)
                {
                    win = new CompassWindow();
                    win.Name = "Compass";
                    win.Owner = App.MainWindow;
                    win.Left = (App.MainWindow.Left + App.MainWindow.Width) - win.Width;
                    win.Top = (App.MainWindow.Top + App.MainWindow.Height) - win.Height;
                    win.Show();

                    this.Interpreter.Conveyor.WindowList.Add(win);

                    App.MainWindow.TextInput.Focus();
                }

                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == Common.Models.WindowType.CompassWindow && x.Name.Equals("Compass", StringComparison.Ordinal)) as CompassWindow;

                    if (win == null)
                    {
                        win = new CompassWindow();
                        win.Name = "Compass";
                        win.Owner = App.MainWindow;
                        win.Left = (App.MainWindow.Left + App.MainWindow.Width) - win.Width;
                        win.Top = (App.MainWindow.Top + App.MainWindow.Height) - win.Height;
                        win.Show();

                        this.Interpreter.Conveyor.WindowList.Add(win);

                        App.MainWindow.TextInput.Focus();
                    }

                    // Get rid of the window.
                    if (o.Close)
                    {
                        win.Close();
                        return;
                    }

                    if (o.Angle > 0 && o.Angle <= 360)
                    {
                        win.SetAngle(o.Angle);
                    }

                    if (!string.IsNullOrWhiteSpace(o.Direction))
                    {
                        win.SetDirection(o.Direction);
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

            [Option('a', "angle", Required = false, HelpText = "The specific angle to position the needle to.")]
            public int Angle { get; set; } = -1;


            [Option('d', "direction", Required = false, HelpText = "The direction to position the needle to.  Valid values are n, ne, e, se, s, sw, w, nw.")]
            public string Direction { get; set; }

            [Option('c', "close", Required = false, HelpText = "Closes the compass window.")]
            public bool Close { get; set; } = false;

        }

    }
}
