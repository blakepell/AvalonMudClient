using System;
using System.Windows;
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
            this.Interpreter.Conveyor.ProgressBarRepeaterStatusVisible = !this.Interpreter.Conveyor.ProgressBarRepeaterStatusVisible;
            this.Interpreter.Conveyor.ProgressBarRepeaterStatusText = Guid.NewGuid().ToString();
        }

    }
}