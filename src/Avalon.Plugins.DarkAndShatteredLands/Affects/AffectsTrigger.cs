/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common;
using Avalon.Common.Interfaces;
using Avalon.Common.Triggers;

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{
    /// <summary>
    /// A trigger to track affects, this will store the current state of them as well as provide
    /// methods to deal with sorting, etc.
    /// </summary>
    /// <remarks>
    /// TODO - Handle lower level that doesn't have modifies or duration.
    /// You are affected by the following spells:
    /// Spell: armor
    /// Spell: bless
    /// </remarks>
    public class AffectsTrigger : Trigger
    {
        /// <summary>
        /// A list of the current affects.
        /// </summary>
        [JsonIgnore]
        public List<Affect> Affects { get; set; }

        /// <summary>
        /// Known affects that have commands different than casting.
        /// </summary>
        public Dictionary<string, AffectCommand> AffectToCommands { get; set; } = new Dictionary<string, AffectCommand>();

        public AffectsTrigger()
        {
            this.Pattern = @"^(Spell|Song ): ([\w\s]+): modifies ([\w\s]+) by ([-+]?\d*) for (\d+) ([\w\s]+) *(.+)$";
            this.IsSilent = true;
            this.Identifier = "c40f9237-7753-4357-84a5-8e7d789853ed";
            this.Affects = new List<Affect>();
            this.PopulateKnownAffects();
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
                Duration = int.Parse(Match.Groups[5].Value)
            };

            // Add the affect into our saved list.
            this.Affects.Add(a);

            // Update the UI.
            this.UpdateUI();
        }

        /// <summary>
        /// Populates affects to command dictionary.
        /// </summary>
        private void PopulateKnownAffects()
        {
            this.AffectToCommands.Add("sneak", new AffectCommand("sneak", "sneak", false));
            this.AffectToCommands.Add("veil of misery", new AffectCommand("veil of misery", "", true));
            this.AffectToCommands.Add("thorn aura", new AffectCommand("sneak", "", true));
            this.AffectToCommands.Add("purgatory", new AffectCommand("purgatory", "", true));
            this.AffectToCommands.Add("citadel", new AffectCommand("citadel", "", true));
            this.AffectToCommands.Add("curse", new AffectCommand("curse", "", true));
            this.AffectToCommands.Add("blind", new AffectCommand("blind", "", true));
            this.AffectToCommands.Add("slow", new AffectCommand("slow", "", true));
            this.AffectToCommands.Add("plague", new AffectCommand("plague", "", true));
            this.AffectToCommands.Add("poison", new AffectCommand("poison", "", true));
            this.AffectToCommands.Add("weaken", new AffectCommand("weaken", "", true));
            this.AffectToCommands.Add("charm", new AffectCommand("charm", "", true));
        }

        /// <summary>
        /// Updates the mud client's UI.
        /// </summary>
        public void UpdateUI()
        {
            var conveyor = AppServices.GetService<IConveyor>();

            if (conveyor == null)
            {
                return;
            }

            this.SortAffects();

            conveyor.ProgressBarRepeaterClear();

            foreach (var affect in this.Affects)
            {
                // We have a command that's not the default trying to cast (or is nothing and should be ignored).
                if (this.AffectToCommands.ContainsKey(affect.Name) && !this.AffectToCommands[affect.Name].IgnoreCommand)
                {
                   conveyor.ProgressBarRepeaterAdd(affect.Name, affect.Duration < 0 ? 50 : affect.Duration + 1, 50, affect.Display(), this.AffectToCommands[affect.Name].Command);
                }
                else
                {
                    // Default, try to cast.
                    conveyor.ProgressBarRepeaterAdd(affect.Name, affect.Duration < 0 ? 50 : affect.Duration + 1, 50, affect.Display(), $"c '{affect.Name}'");
                }
            }

            // Critical spells found
            bool found = false;
            var sb = Argus.Memory.StringBuilderPool.Take();

            if (!this.Affects.Any(x => x.Name.Equals("sanctuary", System.StringComparison.Ordinal)))
            {
                found = true;
                sb.Append("Sanctuary");
            }

            if (!this.Affects.Any(x => x.Name.Equals("haste", System.StringComparison.Ordinal)))
            {
                found = true;
                sb.Append(" Haste");
            }

            if (found)
            {
                conveyor.ProgressBarRepeaterStatusVisible = true;
                conveyor.ProgressBarRepeaterStatusText = $"Spells Missing: {sb}";
            }
            else
            {
                conveyor.ProgressBarRepeaterStatusVisible = false;
            }

            Argus.Memory.StringBuilderPool.Return(sb);
        }

        /// <summary>
        /// Sorts the affects list by duration.
        /// </summary>
        public void SortAffects()
        {
            if (this.Affects.Count > 1)
            {
                // Sort in descending order.
                this.Affects.Sort((x, y) => y.Duration.CompareTo(x.Duration));

                var temp = new List<Affect>();

                // Now put any permanently ones at the top of the list.
                for (int i = this.Affects.Count - 1; i >= 0; i--)
                {
                    if (this.Affects[i].Duration == -1)
                    {
                        temp.Add(this.Affects[i]);
                        this.Affects.RemoveAt(i);
                    }
                }

                foreach (var a in temp)
                {
                    this.Affects.Insert(0, a);
                }
            }
        }

        /// <summary>
        /// Remove a specific affect from the current affects list.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAffect(string key)
        {
            bool found = false;
            var conveyor = AppServices.GetService<IConveyor>();

            for (int i = this.Affects.Count - 1; i >= 0; i--)
            {
                // It's run out, remove it, then continue.
                if (this.Affects[i].Name.Equals(key, System.StringComparison.OrdinalIgnoreCase))
                {
                    conveyor.ProgressBarRemove(this.Affects[i].Name);
                    this.Affects.RemoveAt(i);
                    found = true;
                }
            }

            if (found)
            {
                this.UpdateUI();
            }
        }

        /// <summary>
        /// Adds an affect into the list whether it exists or not.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modifies"></param>
        /// <param name="modifier"></param>
        /// <param name="duration"></param>
        public void AddAffect(string name, string modifies, int modifier, int duration)
        {
            // Create the affect.
            var a = new Affect
            {
                Name = name,
                Modifies = modifies,
                Modifier = modifier,
                Duration = duration
            };

            // Add the affect into our saved list.
            this.Affects.Add(a);

            this.UpdateUI();
        }

        /// <summary>
        /// Adds an affect into the list if it does not exist or updates an existing one.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modifies"></param>
        /// <param name="modifier"></param>
        /// <param name="duration"></param>
        public void AddOrUpdateAffect(string name, string modifies, int modifier, int duration)
        {
            // Get the affect if it exists.
            var a = this.Affects.Find(x => x.Name.Equals(name, System.StringComparison.Ordinal));
            bool add = false;

            // If it doesn't exist, create a new one.
            if (a == null)
            {
                add = true;
                a = new Affect();
            }

            // Set or update the values.
            a.Name = name;
            a.Modifies = modifies;
            a.Modifier = modifier;
            a.Duration = duration;

            // Add the affect into our saved list if it wasn't already there.  Otherwise, the
            // reference has been updated.
            if (add)
            {
                this.Affects.Add(a);
            }

            this.UpdateUI();
        }

        /// <summary>
        /// Reduces all affects tick count by 1 and removes those that have expired.  This will update
        /// the UI on it's own.
        /// </summary>
        public void DecrementAffectDurations()
        {
            var conveyor = AppServices.GetService<IConveyor>();

            for (int i = this.Affects.Count - 1; i >= 0; i--)
            {
                // It's perm (-1) or something custom we set (-2 being we know the spell was cast but don't know how
                // long it lasts yet).  Skip these.
                if (this.Affects[i].Duration < 0)
                {
                    continue;
                }

                // It's run out, remove it, then continue.
                if (this.Affects[i].Duration == 0)
                {
                    conveyor.ProgressBarRemove(this.Affects[i].Name);
                    this.Affects.RemoveAt(i);
                    continue;
                }

                // Reduce it's tick count by 1
                this.Affects[i].Duration -= 1;
                conveyor.ProgressBarRepeaterAdd(this.Affects[i].Name, this.Affects[i].Duration + 1, 50, this.Affects[i].Display());
            }
        }
    }
}