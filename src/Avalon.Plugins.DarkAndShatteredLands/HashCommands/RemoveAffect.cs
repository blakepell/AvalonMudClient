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

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Removes an affect from the current affects list.
    /// </summary>
    public class RemoveAffect : HashCommand
    {
        /// <summary>
        /// A reference to the affects trigger.
        /// </summary>
        private AffectsTrigger _trigger;
        
        public RemoveAffect(IInterpreter interp) : base(interp)
        {
        }

        public RemoveAffect(AffectsTrigger at)
        {
            _trigger = at;
        }

        public RemoveAffect()
        {
        }

        public override string Name { get; } = "#remove-affect";

        public override string Description { get; } = "Removes an affect from the affects window.";

        public override void Execute()
        {
            // Remove the affect if it's found in the affects list.
            _trigger.RemoveAffect(this.Parameters);
        }

        public override async Task ExecuteAsync()
        {

        }

    }
}
