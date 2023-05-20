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
    /// Used when a spell is cast but we don't know how long the affect lasts for.  This will
    /// add or update the duration to -2 which will render as a ? on the UI until such time
    /// the affects command has been run in the game.
    /// </summary>
    public class PartialAffect : HashCommand
    {
        /// <summary>
        /// A reference to the affects trigger.
        /// </summary>
        private AffectsTrigger _trigger;

        public PartialAffect(IInterpreter interp) : base(interp)
        {
        }

        public PartialAffect(AffectsTrigger at)
        {
            _trigger = at;
        }

        public PartialAffect()
        {
        }

        public override string Name { get; } = "#partial-affect";

        public override string Description { get; } = "Adds or updates an affect with an unknown time limit.";

        public override void Execute()
        {
            // Remove the affect if it's found in the affects list.
            _trigger.AddOrUpdateAffect(this.Parameters, "", 0, -2);
        }

        public override async Task ExecuteAsync()
        {

        }

    }
}
