﻿using Avalon.Common.Models;
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
            
            // IC Channels: Clan gossip, clan, gossip, ask, answer, kingdom, group tells, tells
            this.Triggers.Add(new Trigger(@"^([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (clan gossip|clan|gossip|ask|answer|Kingdom|tell).*$", "", "", true, "8b0bc970-08de-498e-9866-8e1aec458c08", TerminalTarget.Communication, true));

            // OOC Channels: OOC, OOC Clan, OOC Kingdom
            this.Triggers.Add(new Trigger(@"^([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (OOC).*$", "", "", true, "4c7efde0-6f9a-4429-8b5c-edf23e60e61f", TerminalTarget.OutOfCharacterCommunication, true));

            // Auction
            this.Triggers.Add(new Trigger(@"^([\w'-]+) auctions: '(.*?)'", "", "", true, "ae84003b-8252-4946-8db9-01b71dd6d44e", TerminalTarget.Communication, true));

            // Prays for immortals & grats.
            this.Triggers.Add(new Trigger(@"^([\w'-]+) (pray|prays|grats) '(.*?)'", "", "", true, "8f324cf1-0f99-4c80-a4f4-a32f1b3e6cfb", TerminalTarget.Communication, true));

            // Shalonesti shared channel.
            this.Triggers.Add(new Trigger(@"^\(Shalonesti\) ", "", "", true, "bd8f5ed0-3122-4091-bb23-db2f589e7cf0", TerminalTarget.Communication, true));

            // Geirhart [Newbie]: 'So you need a description, five sentences that describe your character '
            this.Triggers.Add(new Trigger(@"^(([\w'-]+)|\(An Imm\)|\(Imm\) ([\w'-]+)) \[Newbie\]: '(.*?)", "", "", true, "35acad5d-970e-4835-adb2-0f855fbde318", TerminalTarget.OutOfCharacterCommunication, true));

            // Bloodbath
            this.Triggers.Add(new Trigger(@"^([\w'-]+) Bloodbath: '(.*?)'", "", "", true, "6ee1fe9f-2a27-4afb-8c6f-451213f1d2bb", TerminalTarget.Communication, true));

            // Toasts, TODO: Dragon/Remort toasts ( Dragon ) ( Balanx )
            this.Triggers.Add(new Trigger(@"([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?", "", "", true, "6731ca72-5672-4dab-879b-896b2945805a", TerminalTarget.Communication, true));

            // Game events
            this.Triggers.Add(new Trigger(@"^([\w'-]+) has unlocked the Underworld Keeps!", "", "", true, "638a6c30-0a5e-4859-b666-72413f2f1781", TerminalTarget.Communication, false));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) has conquered (.*)!", "", "", true, "03e68858-53dd-4412-80e3-6c869544ae15", TerminalTarget.Communication, false));
            this.Triggers.Add(new Trigger(@"^personal> Keep Lord clans '(.*) is invading (.*)!'", "", "", true, "16f9a412-a6ef-4b5d-a2b7-4dd36f117416", TerminalTarget.Communication, false));
        }
    }
}
