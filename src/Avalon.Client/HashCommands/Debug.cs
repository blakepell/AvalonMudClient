using System;
using System.Linq;
using System.Text;
using System.Windows;
using Accessibility;
using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.Controls;
using Avalon.Windows;

namespace Avalon.HashCommands
{

    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Runs some debugging code.";

        Random _rand = new Random();

        public override void Execute()
        {
            //var win = new Avalon.Shell(new VariableList(), App.MainWindow)
            //{
            //    HeaderTitle = "Variables",
            //    HeaderIcon = ModernWpf.Controls.Symbol.Account,
            //    Width = App.MainWindow.Width * .85,
            //    Height = App.MainWindow.Height * .85,
            //    Left = App.MainWindow.Left + (App.MainWindow.Width - (App.MainWindow.Width * .85)) / 2,
            //    Top = App.MainWindow.Top + (App.MainWindow.Height - (App.MainWindow.Height * .85)) / 2,
            //    WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //    SecondaryButtonVisibility = Visibility.Collapsed
            //};

            var win = new Avalon.Shell(new VariableList(), null)
            {
                HeaderTitle = "Variables",
                HeaderIcon = ModernWpf.Controls.Symbol.Account,
                SecondaryButtonVisibility = Visibility.Collapsed
            };

            win.SetSizeAndPosition(.85);
            win.Show();

            //App.MainWindow.VariableRepeater.Bind();
            //var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == Common.Models.WindowType.CompassWindow) as CompassWindow;

            //if (win == null)
            //{
            //    win = new CompassWindow();
            //    win.Name = "Compass";
            //    win.Owner = App.MainWindow;
            //    win.Show();
            //    this.Interpreter.Conveyor.WindowList.Add(win);
            //}

            //win.SetAngle(_rand.Next(0, 360));

            //this.Interpreter.IsRecordingCommands = !this.Interpreter.IsRecordingCommands;
            //this.Interpreter.Conveyor.EchoLog($"Recording: {this.Interpreter.IsRecordingCommands}", Common.Models.LogType.Debug);

            //var sb = new StringBuilder();

            //foreach (string item in this.Interpreter.RecordedCommands)
            //{
            //    sb.AppendFormat("'{0}',", item);
            //}

            //if (!this.Interpreter.IsRecordingCommands)
            //{
            //    this.Interpreter.Send("#window -n cmds");
            //    this.Interpreter.Send("#echo -w \"cmds\" " + sb.ToString());
            //}

        }

    }
}