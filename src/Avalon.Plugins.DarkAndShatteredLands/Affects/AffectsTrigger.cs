using Avalon.Common.Colors;
using Avalon.Common.Models;
using Avalon.Common.Settings;
using Avalon.Common.Triggers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{
    /// <summary>
    /// A trigger to track affects, this will store the current state of them as well as provide
    /// methods to deal with sorting, etc.
    /// </summary>
    public class AffectsTrigger : Trigger
    {
        /// <summary>
        /// A list of the current affects.
        /// </summary>
        [JsonIgnore]
        public List<Affect> Affects { get; set; }

        public AffectsTrigger()
        {
            this.Pattern = @"^Spell: ([\w\s]+): modifies ([\w\s]+) by ([-+]?\d*) for (\d+) ([\w\s]+) *(.+)$";
            this.IsSilent = true;
            this.Identifier = "c40f9237-7753-4357-84a5-8e7d789853ed";
            this.Affects = new List<Affect>();
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
                Name = Match.Groups[1].Value.Trim(),
                Modifies = Match.Groups[2].Value.Trim(),
                Modifier = int.Parse(Match.Groups[3].Value),
                Duration = int.Parse(Match.Groups[4].Value)
            };

            // Add the affect into our saved list.
            this.Affects.Add(a);

            // Update the UI.
            this.UpdateUI();
        }

        /// <summary>
        /// Updates the mud client's UI.
        /// </summary>
        public void UpdateUI()
        {
            if (this.Conveyor == null)
            {
                return;
            }

            this.SortAffects();

            this.Conveyor.ProgressBarRepeaterClear();

            foreach (var affect in this.Affects)
            {
                this.Conveyor.ProgressBarRepeaterAdd(affect.Name, affect.Duration < 0 ? 50 : affect.Duration + 1, 50, affect.Display());
            }
        }

        /// <summary>
        /// Sorts the affects list by duration.
        /// </summary>
        public void SortAffects()
        {
            if (this.Affects.Count > 1)
            {
                // Sort in decending order.
                this.Affects.Sort((x, y) => y.Duration.CompareTo(x.Duration));

                var temp = new List<Affect>();

                // Now put any permaentely ones at the top of the list.
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

            for (int i = this.Affects.Count - 1; i >= 0; i--)
            {
                // It's run out, remove it, then continue.
                if (this.Affects[i].Name.Equals(key, System.StringComparison.OrdinalIgnoreCase))
                {
                    this.Conveyor.ProgressBarRemove(this.Affects[i].Name);
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
            var a = this.Affects.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.Ordinal));
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
                    this.Conveyor.ProgressBarRemove(this.Affects[i].Name);
                    this.Affects.RemoveAt(i);
                    continue;
                }

                // Reduce it's tick count by 1
                this.Affects[i].Duration -= 1;
                this.Conveyor.ProgressBarRepeaterAdd(this.Affects[i].Name, this.Affects[i].Duration + 1, 50, this.Affects[i].Display());
            }
        }

    }
}
