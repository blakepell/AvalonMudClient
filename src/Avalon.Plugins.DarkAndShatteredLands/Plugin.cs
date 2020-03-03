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

            // TODO: (The ghost of) The ghost of Jimminy clan gossips 'please, you were hunting down a trainee. stop your pathetic rationalizing'
            
            // Clan
            this.Triggers.Add(new Trigger(@"^You clan '(.*?)'", "", "", true, "8b0bc970-08de-498e-9866-8e1aec458c08", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You clan \((\w+)\) '(.*?)'", "", "", true, "1cb43145-5020-4300-97db-856e5d515530", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) clans '(.*?)'", "", "", true, "4bad07c4-3d50-45db-bfc0-7a9ba4d48908", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) clans \((\w+)\) '.*?'", "", "", true, "5de12b95-ae6f-41db-999a-c02f0b094aa2", TerminalTarget.Communication, true));

            // OOC Clan
            this.Triggers.Add(new Trigger(@"^([\w'-]+) OOC (Clan|clan): '(.*?)'", "", "", true, "4c7efde0-6f9a-4429-8b5c-edf23e60e61f", TerminalTarget.Communication, true));

            // Clan Gossip
            this.Triggers.Add(new Trigger(@"^You clan gossip '(.*?)'", "", "", true, "37a3adbc-3628-4336-b537-b12b4f218ed1", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You clan gossip \((\w+)\) '(.*?)'", "", "", true, "89bd548b-5f6c-4664-83d5-b72ef192e7bb", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) clan gossips '(.*?)'", "", "", true, "64cffbfa-3431-4b44-8c74-475e5c4ae3fb", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) clan gossips \((\w+)\) '(.*?)'", "", "", true, "6e36dc83-104a-4d0c-b379-0f6fd5ddc289", TerminalTarget.Communication, true));

            // Kingdom (TODO - Work on this, it's getting both OOC and IC).  Need two so they can go to the correct window.
            this.Triggers.Add(new Trigger(@"^(The ghost of)?\s?([\w'-]+)?\s?Kingdom: (.*?)\s?'(.*'?)'", "", "", true, "a9f2539d-f89f-4c7d-8a22-29ca63177d97", TerminalTarget.Communication, true));

            // Gossip
            this.Triggers.Add(new Trigger(@"^You gossip '(.*?)'", "", "", true, "62507924-4462-46a6-b3a2-91241c6699cb", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You gossip \((\w+)\) '(.*?)'", "", "", true, "51f60ec4-21da-4ff0-8c62-abf35c3ba30e", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) gossips '(.*?)'", "", "", true, "de0bdc2b-301b-4b4f-80f6-61174e5dcf18", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) gossips \((\w+)\) '(.*?)'", "", "", true, "adb00ed9-8fe5-4c9f-9b3e-228380972cdb", TerminalTarget.Communication, true));

            // Group Tells & Tells
            this.Triggers.Add(new Trigger(@"^([\w'-]+) tells the group '(.*?)'", "", "", true, "e726579f-af97-497e-bab8-3b5285038133", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You tell the group '(.*?)'", "", "", true, "20822698-009e-43e0-82f3-5f325d68f054", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You tell ([\w'-]+) '(.*?)'", "", "", true, "010f15e4-5d49-42b4-b825-b0c6d827666c", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^You tell ([\w'-]+) \((.*?)\) '(.*?)'", "", "", true, "c12928c4-ef8c-4608-b09e-898f10c22f29", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) tells you '(.*?)'", "", "", true, "6c03fc96-4894-474c-8800-2c7d1f3edd79", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) tells you \((.*?)\) '(.*?)'", "", "", true, "296ec620-8797-4431-b00c-3dee09e1f4ae", TerminalTarget.Communication, true));

            // OOC
            this.Triggers.Add(new Trigger(@"^(([\w'-]+)|\(An Imm\)|\(Imm\) ([\w'-]+)) OOC: '(.*?)", "", "", true, "27aa49ac-aa12-4228-9366-f0ef241da014", TerminalTarget.OutOfCharacterCommunication, true));

            // Auction
            this.Triggers.Add(new Trigger(@"^([\w'-]+) auctions: '(.*?)'", "", "", true, "ae84003b-8252-4946-8db9-01b71dd6d44e", TerminalTarget.Communication, true));

            // Prays for immortals & grats.
            this.Triggers.Add(new Trigger(@"^([\w'-]+) (pray|prays|grats) '(.*?)'", "", "", true, "8f324cf1-0f99-4c80-a4f4-a32f1b3e6cfb", TerminalTarget.Communication, true));

            // Shalonesti shared channel.
            this.Triggers.Add(new Trigger(@"^\(Shalonesti\) ", "", "", true, "bd8f5ed0-3122-4091-bb23-db2f589e7cf0", TerminalTarget.Communication, true));

            // Ask and Answer
            this.Triggers.Add(new Trigger(@"^([\w'-]+) ask '(.*?)'", "", "", true, "a7805e35-a3d7-4183-9eab-f5b54796a3b5", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^([\w'-]+) (answer|answers) '(.*?)'", "", "", true, "cd36715c-6a1d-44d6-87fa-5b3544ec0b66", TerminalTarget.Communication, true));

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
