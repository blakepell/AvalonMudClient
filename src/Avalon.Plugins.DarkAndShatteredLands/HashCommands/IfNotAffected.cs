/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.HashCommands;
using Avalon.Plugins.DarkAndShatteredLands.Affects;
using CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Will execute a command if a given affect is not present
    /// </summary>
    public class IfNotAffected : HashCommand
    {
        /// <summary>
        /// A reference to the affects trigger.
        /// </summary>
        private AffectsTrigger _trigger;

        public IfNotAffected(IInterpreter interp) : base(interp)
        {
        }

        public IfNotAffected(AffectsTrigger at)
        {
            _trigger = at;
        }

        public IfNotAffected()
        {
        }

        public override string Name { get; } = "#if-not-affected";

        public override string Description { get; } = "Executes a command if not affected by the specified affect.";

        public override void Execute()
        {
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(async o =>
                {
                    if (!_trigger.Affects.Any(x => x.Name.Equals(o.AffectName, System.StringComparison.OrdinalIgnoreCase)))
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

            [Value(1, Required = true, HelpText = "The command to execute if not affected by the affect.")]
            public string Command { get; set; } = "";
        }

    }
}
