/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Triggers;

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{
    /// <summary>
    /// A trigger for a permanent affect.  This will be linked to the main AffectTrigger so it can update
    /// that master list of current affects.
    /// </summary>
    public class AffectsPermanentTrigger : Trigger
    {
        public AffectsTrigger AffectsTrigger { get; set; }

        public AffectsPermanentTrigger(AffectsTrigger at)
        {                              
            this.AffectsTrigger = at;
            this.Pattern = @"^(Spell|Song ): ([\w\s]+): modifies ([\w\s]+) by ([-+]?\d*) permanently$";
            this.IsSilent = true;
            this.Identifier = "6f8bbbe5-067f-4bd2-82a8-b7dbd81e0f95";
        }

        public override string Command => "";

        public override void Execute()
        {
            if (Match == null)
            {
                return;
            }

            // Create the affect.
            var a = new Affect
            {
                Name = Match.Groups[2].Value.Trim(),
                Modifies = Match.Groups[3].Value.Trim(),
                Modifier = int.Parse(Match.Groups[4].Value),
                Duration = -1
            };

            // Add it to the list on the main affects trigger.
            if (this.AffectsTrigger != null)
            {
                this.AffectsTrigger.Affects.Add(a);
                this.AffectsTrigger.UpdateUI();
            }

        }

    }
}
