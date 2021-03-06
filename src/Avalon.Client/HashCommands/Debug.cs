﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using MoonSharp.Interpreter;
using System.Threading.Tasks;

namespace Avalon.HashCommands
{
    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
        }

        private DynValue Code { get; set; }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Runs some debugging code.";

        public override async Task ExecuteAsync()
        {
            ((Interpreter)this.Interpreter).ScriptHost.MoonSharp.Reset();
        }

        public override void Execute()
        {
        }
    }
}