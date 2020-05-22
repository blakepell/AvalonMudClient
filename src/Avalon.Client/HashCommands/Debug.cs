using System;
using System.Windows;
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
            this.Interpreter.IsRecordingCommands = true;
            this.Interpreter.Conveyor.EchoText(this.Interpreter.RecordedCommands.ToDelimitedString(";"));
        }

    }
}