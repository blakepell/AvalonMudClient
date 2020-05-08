using Avalon.Common.Triggers;

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{
    /// <summary>
    /// Clears the list of affects and prepares to repopulate them.
    /// </summary>
    public class AffectsClearTrigger : Trigger
    {
        public AffectsTrigger AffectsTrigger { get; set; }

        public AffectsClearTrigger(AffectsTrigger at)
        {
            this.AffectsTrigger = at;
            this.Pattern = @"^(You are affected by the following spells:)|(You are not affected by any spells.)";
            this.IsSilent = true;
            this.Identifier = "33d1fc1b-7383-4d65-a93c-e225a81f40b4";
        }

        public override string Command => "";

        public override void Execute()
        {
            if (this.AffectsTrigger != null)
            {
               this.AffectsTrigger.Affects.Clear();
               this.AffectsTrigger.UpdateUI();
            }
        }
    }
}
