using System;
using System.Text;
using System.Windows;
using Accessibility;
using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Runs some debugging code.";

        Random _rand = new Random();

        public override void Execute()
        {
            this.Interpreter.IsRecordingCommands = !this.Interpreter.IsRecordingCommands;
            this.Interpreter.Conveyor.EchoLog($"Recording: {this.Interpreter.IsRecordingCommands}", Common.Models.LogType.Debug);

            var sb = new StringBuilder();

            foreach (string item in this.Interpreter.RecordedCommands)
            {
                sb.AppendFormat("'{0}',", item);
            }

            if (!this.Interpreter.IsRecordingCommands)
            {
                this.Interpreter.Send("#window -n cmds");
                this.Interpreter.Send("#echo -w \"cmds\" " + sb.ToString());
            }

        }

    }
}