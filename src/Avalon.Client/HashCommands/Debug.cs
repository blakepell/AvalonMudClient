﻿using System;
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
            this.Interpreter.Conveyor.EchoText("Hello, this is some text from the game probably.", "testWin");
        }

    }
}