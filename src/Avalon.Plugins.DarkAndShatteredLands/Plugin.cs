using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System;

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

            this.LoadTriggers();
            this.LoadMenu();
            this.ResetVariables();
        }

        /// <summary>
        /// Loads any triggers if they haven't already been loaded.
        /// </summary>
        public void LoadTriggers()
        {
            if (this.Triggers.Count > 0)
            {
                return;
            }

            // IC Channels: Clan gossip, clan, gossip, ask, answer, kingdom, group tells, tells, auction, pray, grats, quest (quote at the end)
            this.Triggers.Add(new Trigger(@"^[\a]?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (clan gossip|clan|gossip|ask|answer|tell|auction|Bloodbath|pray|grats|quest|radio|imm).*'$", "", "", true, "8b0bc970-08de-498e-9866-8e1aec458c08", TerminalTarget.Communication, true));
            this.Triggers.Add(new Trigger(@"^[\w'-]+ Kingdom: .*$", "", "", true, "1dcf2580-da86-45b5-880f-36f9468891c1", TerminalTarget.Communication, true));

            // OOC Channels: OOC, OOC Clan, OOC Kingdom, Newbie
            this.Triggers.Add(new Trigger(@"^([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (OOC|\[Newbie\]).*$", "", "", true, "4c7efde0-6f9a-4429-8b5c-edf23e60e61f", TerminalTarget.OutOfCharacterCommunication, true));

            // Shared channels (Shalonesti, Conclave)
            this.Triggers.Add(new Trigger(@"^\((Shalonesti|OOC Shalonesti|Clave|OOC Clave)\).*$", "", "", true, "bd8f5ed0-3122-4091-bb23-db2f589e7cf0", TerminalTarget.Communication, true));

            // Toasts, TODO: Dragon/Remort toasts ( Dragon ) ( Balanx )
            this.Triggers.Add(new Trigger(@"([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?", "", "", true, "6731ca72-5672-4dab-879b-896b2945805a", TerminalTarget.Communication, true));

            // Game events: Keeps
            this.Triggers.Add(new Trigger(@"^([\w'-]+) has (conquered|unlocked) (.*)!", "", "", true, "638a6c30-0a5e-4859-b666-72413f2f1781", TerminalTarget.Communication, false));
            this.Triggers.Add(new Trigger(@"^(personal> )?Keep Lord clans '(.*) is invading (.*)!'", "", "", true, "16f9a412-a6ef-4b5d-a2b7-4dd36f117416", TerminalTarget.Communication, false));

            // Login, find out who the player is and put it into a variable.  Run whoami and then score to scrap initial info.
            this.Triggers.Add(new Trigger(@"^Welcome to DSL! DSL Loves You! Other muds think you are ugly, they said so", "whoami;score"));
            this.Triggers.Add(new Trigger(@"Reconnecting. Type replay to see missed tells.", "#trigger -i 0000f3e4-9ab9-4f52-9e29-0ba6d88348a6 -e;whoami;score"));
            this.Triggers.Add(new Trigger(@"^You are logged in as\: (?<Character>.*?)$", "whois @Character"));
            this.Triggers.Add(new Trigger(@"\[Exits: (?<Exits>.*?)  \]", ""));
            this.Triggers.Add(new Trigger(@"\[Exits:  \]", "#set ExitsShort none;#set Exits none", "", true, "f8efc60a-adcf-46fc-b230-bcf3a4fee8a2"));
            this.Triggers.Add(new Trigger(@"^Your current war\(s\): (?<Wars>.*?)$", ""));

            // One time trigger that will disable itself, it sets the players info from the whois entry (but then
            // disables it so you don't reset those variables when you look at other players entries.  Some Lua will
            // be run here so it only runs a cinfo if the person is in a clan.
            var t = new Trigger(@"^\[\s?(?<Level>\d+)\s+([\w-]+) (?<Class>\w+)\]\s+(\[Quiet\] )?(\[ (?<Clan>.*?) \] )?(\((?<Kingdom>.*?)\))?", "", "", false, "0000f3e4-9ab9-4f52-9e29-0ba6d88348a6")
            {
                IsLua = true,
                DisableAfterTriggered = true
            };

            t.Command = @"
local clan = lua:GetVariable(""Clan"")

if clan ~= nil and clan ~= """" and clan ~= ""Loner"" and clan ~= ""Renegade"" and clan ~= ""Dragon"" and clan ~= ""Angel"" and clan ~= ""Balanx"" and clan ~= ""Demon"" then
    lua:Send(""cinfo "" ..clan)
end";

            this.Triggers.Add(t);


        }

        /// <summary>
        /// Loads any menu items if they haven't already been loaded.
        /// </summary>
        public void LoadMenu()
        {
            if (this.MenuItems.Count > 0)
            {
                return;
            }

            // Load any menu items that exist.  The implementing UI will now how to cast them.
            var rd = new System.Windows.ResourceDictionary();
            rd.Source = new Uri("/Avalon.Plugins.DarkAndShatteredLands;component/Menu/DslMenuItem.xaml", UriKind.Relative);
            this.MenuItems.Add(rd);
        }

        /// <summary>
        /// Resets any variables used by this plugin.
        /// </summary>
        public void ResetVariables()
        {
            // Reset any CLR variables we want to be empty by default.
            this.Conveyor.SetVariable("Health", "0");
            this.Conveyor.SetVariable("MaxHealth", "0");
            this.Conveyor.SetVariable("Mana", "0");
            this.Conveyor.SetVariable("MaxMana", "0");
            this.Conveyor.SetVariable("Move", "0");
            this.Conveyor.SetVariable("MaxMove", "0");
            this.Conveyor.SetVariable("Target", "Nobody");
            this.Conveyor.SetVariable("Room", "Limbo");
            this.Conveyor.SetVariable("Area", "Unknown");
            this.Conveyor.SetVariable("Exits", "none");
            this.Conveyor.SetVariable("ExitsShort", "none");
            this.Conveyor.SetVariable("Character", "");
            this.Conveyor.SetVariable("Clan", "");
            this.Conveyor.SetVariable("Kingdom", "");
            this.Conveyor.SetVariable("Wimpy", "0");
            this.Conveyor.SetVariable("PKP", "0");
            this.Conveyor.SetVariable("PKPLevel", "0");
            this.Conveyor.SetVariable("Level", "1");
            this.Conveyor.SetVariable("Class", "");
        }

    }
}
