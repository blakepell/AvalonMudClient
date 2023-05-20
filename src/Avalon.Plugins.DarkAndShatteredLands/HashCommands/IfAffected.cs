/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.HashCommands;
using Avalon.Plugins.DarkAndShatteredLands.Affects;
using CommandLine;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Will execute a command if a given affect is present.
    /// </summary>
    public class IfAffected : HashCommand
    {
        /// <summary>
        /// A reference to the affects trigger.
        /// </summary>
        private AffectsTrigger _trigger;

        public IfAffected(IInterpreter interp) : base(interp)
        {
        }

        public IfAffected(AffectsTrigger at)
        {
            _trigger = at;
        }

        public IfAffected()
        {
        }

        public override string Name { get; } = "#if-affected";

        public override string Description { get; } = "Executes a command if affected by the specified affect.";

        public override void Execute()
        {
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(async o =>
                {
                    if (_trigger.Affects.Any(x => x.Name.Equals(o.AffectName, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        this.Interpreter.Send(o.Command, false, false);
                    }
                });
        }

        public override async Task ExecuteAsync()
        {

        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        private class Arguments
        {
            [Value(0, Required = true, HelpText = "The name of the affect.")]
            public string AffectName { get; set; } = "";

            [Value(1, Required = true, HelpText = "The command to execute if affected by the affect.")]
            public string Command { get; set; } = "";
        }

    }
}
