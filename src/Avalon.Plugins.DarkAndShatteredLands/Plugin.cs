using Avalon.Common.Models;
using Avalon.Common.Triggers;

namespace Avalon.Plugins.DarkAndShatteredLands
{

    /// <summary>
    /// The main plugin for Dark and Shattered Lands (dsl-mud.org)
    /// </summary>
    public class Plugin : Avalon.Common.Plugins.Plugin
    {
        public override string IpAddress { get; set; } = "dsl-mud.org";

        public override void Initialize()
        {
            this.Conveyor.Title = "Dark and Shattered Lands";
            
            // IC Channels: Clan gossip, clan, gossip, ask, answer, kingdom, group tells, tells, auction, pray, grats, quest
            this.Triggers.Add(new Trigger(@"^([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (clan gossip|clan|gossip|ask|answer|tell|auction|Bloodbath|pray|grats|quest|radio|imm).*$", "", "", true, "8b0bc970-08de-498e-9866-8e1aec458c08", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^Kingdom: .*$", "", "", true, "1dcf2580-da86-45b5-880f-36f9468891c1", TerminalTarget.Communication, true));

            // OOC Channels: OOC, OOC Clan, OOC Kingdom, Newbie
            this.Triggers.Add(new Trigger(@"^([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (OOC|\[Newbie\]).*$", "", "", true, "4c7efde0-6f9a-4429-8b5c-edf23e60e61f", TerminalTarget.OutOfCharacterCommunication, true));

            // Shared channels (Shalonesti, Conclave)
            this.Triggers.Add(new Trigger(@"^\(Shalonesti|OOC Shalonesti|Clave|OOC Clave\) ", "", "", true, "bd8f5ed0-3122-4091-bb23-db2f589e7cf0", TerminalTarget.Communication, true));

            // Toasts, TODO: Dragon/Remort toasts ( Dragon ) ( Balanx )
            this.Triggers.Add(new Trigger(@"([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?", "", "", true, "6731ca72-5672-4dab-879b-896b2945805a", TerminalTarget.Communication, true));

            // Game events: Keeps
            this.Triggers.Add(new Trigger(@"^([\w'-]+) has (conquered|unlocked) (.*)!", "", "", true, "638a6c30-0a5e-4859-b666-72413f2f1781", TerminalTarget.Communication, false));
            this.Triggers.Add(new Trigger(@"^(personal> )?Keep Lord clans '(.*) is invading (.*)!'", "", "", true, "16f9a412-a6ef-4b5d-a2b7-4dd36f117416", TerminalTarget.Communication, false));
        }
    }
}
