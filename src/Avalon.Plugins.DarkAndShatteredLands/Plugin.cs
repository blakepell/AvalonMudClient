using Argus.Extensions;
using Avalon.Common.Models;
using Avalon.Common.Triggers;
using Avalon.Plugins.DarkAndShatteredLands.Affects;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Avalon.Plugins.DarkAndShatteredLands
{

    /// <summary>
    /// The main plugin for Dark and Shattered Lands (dsl-mud.org)
    /// </summary>
    public class Plugin : Avalon.Common.Plugins.Plugin
    {
        public override string IpAddress { get; set; } = "dsl-mud.org";

        private AffectsTrigger _affectsTrigger;

        public override void Initialize()
        {
            this.Conveyor.Title = "Dark and Shattered Lands";

            try
            {
                this.LoadTriggers();
                this.LoadMenu();
                this.LoadHashCommands();
                this.ResetVariables();
                this.LoadLuaCommands();
                this.CreateDbTables();
            }
            catch (Exception ex)
            {
                this.Conveyor.EchoLog("An error occurred in the DSL plugin Initialize method.", LogType.Error);
                this.Conveyor.EchoLog(ex.ToFormattedString(), LogType.Error);
            }

            this.Conveyor.SetCustomTabVisible(CustomTab.Tab1, true);
            this.Conveyor.SetCustomTabLabel(CustomTab.Tab1, "Communication");

            this.Conveyor.SetCustomTabVisible(CustomTab.Tab2, true);
            this.Conveyor.SetCustomTabLabel(CustomTab.Tab2, "OOC");

            this.Initialized = true;
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

            // Now, add our prompt in that is set via #set-prompt.
            // This will run more than any other trigger any user of DSL will have.  It is priority #1.  It will also stop processing when it's done
            // which is an opinionated take.  If the user doesn't want that and wants to write additional triggers off of the prompt they can simply
            // lock this trigger and then change the stop processing bit themselves.
            this.Triggers.Add(new Trigger(pattern: @"^(\[Quiet\] )?\<(?<Health>\d+)/(?<MaxHealth>\d+)hp (?<Mana>\d+)/(?<MaxMana>\d+)m (?<Move>\d+)/(?<MaxMove>\d+)mv \((?<Wimpy>\d+)\|(?<Stance>\w+)\) \((?<Room>.*?)\) \((?<ExitsShort>.*?)\) (?<ExpTnl>.*?) (?<Gold>.*?) (?<Silver>.*?) (?<QuestPoints>.*?) (?<Language>.*?) (?<Weight>.*?) (?<MaxWeight>.*?) (?<GameTime>.*?)\>"
                                            , command: "#update-info-bar", isSilent: true, identifier: "1b8a50c4-ab92-48a8-8527-21331791f33d", moveTo: TerminalTarget.None, gag: true, group: "dsl-mud.org"
                                            , priority: 1, stopProcessing: true));

            // Keep proc'ing after this one, the user might want to do something based on it.
            this.Triggers.Add(new Trigger(pattern: @"\[Exits: (?<Exits>.*?)  \]"
                                            , command: "", identifier: "dbaad2c7-45fd-4bf6-9512-8b0f6e19033a", group: "dsl-mud.org",
                                            priority: 2, stopProcessing: true));

            // Keep proc'ing after this one, the user might want to do something based on it.
            this.Triggers.Add(new Trigger(pattern: @"\[Exits:  \]"
                                            , command: "#set ExitsShort none;#set Exits none", isSilent: true, identifier: "f8efc60a-adcf-46fc-b230-bcf3a4fee8a2", group: "dsl-mud.org"
                                            , priority: 2, stopProcessing: true));

            // Communication Channels
            // Tells
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \btell(s?)\b.*'$"
                                            , command: "", isSilent: true, identifier: "1b06d6a5-98c3-4839-b795-aa0ef13e694e", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Tells"
                                            , priority: 9, stopProcessing: true));

            // Clan
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bclan(s?)\b.*'$"
                                            , command: "", isSilent: true, identifier: "b3c6d826-a665-4db7-8921-b9f1d447de41", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Clan"
                                            , priority: 10, stopProcessing: true));

            // OOC Clan
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bOOC [Cc]lan(s?)\b.*'$"
                                            , command: "", isSilent: true, identifier: "d9d63ee7-b15a-4ce8-92c0-f47d802919e5", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: OOC Clan"
                                            , priority: 11, stopProcessing: true));

            // OOC
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bOOC\b:.*'$"
                                , command: "", isSilent: true, identifier: "52ce58c0-63ca-4c28-932a-d51e2af903b1", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: OOC"
                                , priority: 12, stopProcessing: true));

            // Kingdom
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?^[\a]?(\[ .* \] )?([\w'-]+||A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+|)(\s)?Kingdom: .*$"
                                            , command: "", isSilent: true, identifier: "1dcf2580-da86-45b5-880f-36f9468891c1", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Kingdom"
                                            , priority: 13, stopProcessing: true));


            // OOC Kingdom
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bOOC Kingdom(s?)\b:.*'$"
                                            , command: "", isSilent: true, identifier: "c03e245b-db36-41f8-93fa-9d15d6afe858", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: OOC Kingdom"
                                            , priority: 14, stopProcessing: true));

            // Clan Gossip
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bclan gossip(s?)\b.*'$"
                                            , command: "", isSilent: true, identifier: "32875844-7ff1-423f-8a18-d4c0729dc3a1", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Clan Gossip"
                                            , priority: 15, stopProcessing: true));

            // Gossip
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bgossip(s?)\b.*'$"
                                , command: "", isSilent: true, identifier: "d4fd3a81-02d9-4e3c-945c-33fc8a752cf2", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Gossip"
                                , priority: 15, stopProcessing: true));

            // Shared Clan/Kingdom IC
            this.Triggers.Add(new Trigger(pattern: @"^\((Shalonesti|Clave|Thaxanos)\).*$"
                                , command: "", isSilent: true, identifier: "15a01c9a-736d-4fd0-a4dd-6d7c2506e3ab", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Shared Clan/Kingdom"
                                , priority: 16, stopProcessing: true));

            // Shared Clan/Kingdom OOC
            this.Triggers.Add(new Trigger(pattern: @"^\((OOC Shalonesti|OOC Clave|OOC Thaxanos)\).*$"
                                , command: "", isSilent: true, identifier: "74876d5e-8fc8-4735-a9e9-fb597568b852", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: Shared Clan/Kingdom OOC"
                                , priority: 16, stopProcessing: true));

            // Imm
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?(\(Wizi@\d\d\) )?(You|\(An Imm\)|\(Imm\) [\w'-]+|\(Imm\) [\w'-]+|[\w'-]+) \bimm(s?)\b:.*'$"
                                , command: "", isSilent: true, identifier: "4c1170e0-9dfe-4276-8afe-4773aa7be204", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Imm"
                                , priority: 17, stopProcessing: true));

            // Admin/Coder
            this.Triggers.Add(new Trigger(pattern: @"\((Admin|Coder)\) \(Imm\) [\w'-]+:"
                                            , command: "", isSilent: true, identifier: "95a9bb06-fdd0-4211-af28-a8e46671bbc8", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Admin/Coder"
                                            , priority: 17, stopProcessing: true));

            // Pray
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) \bpray(s?)\b.*'$"
                                , command: "", isSilent: true, identifier: "ae667eb0-d5f7-4daa-810d-0fdbb434f203", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Pray"
                                , priority: 16, stopProcessing: true));

            // Ask/Answer
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bask(s?)\b|\banswers(s?)\b).*'$"
                                            , command: "", isSilent: true, identifier: "91ecbf7e-4fee-437a-baeb-a6f5d53da0c2", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: Ask/Answer"
                                            , priority: 17, stopProcessing: true));

            // Auction
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bauction(s?)\b).*'$"
                                , command: "", isSilent: true, identifier: "f93d4b6e-3789-4b0b-8d86-a15bdf28a897", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Auction"
                                , priority: 18, stopProcessing: true));

            // Quest
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bquest(s?)\b).*'$"
                                , command: "", isSilent: true, identifier: "a2e71ba2-2e89-42ed-b951-f55461560fba", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Quest"
                                , priority: 19, stopProcessing: true));

            // Bloodbath
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bBloodbath(s?)\b).*'$"
                                , command: "", isSilent: true, identifier: "a6b89afc-9c07-4616-b4c9-616ab7dc96ad", moveTo: TerminalTarget.Terminal1, gag: true, group: "Communication: Bloodbath"
                                , priority: 20, stopProcessing: true));

            // Newbie
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\(.*\)?)?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (\[Newbie\]).*$"
                                , command: "", isSilent: true, identifier: "ee6fbe61-751c-4b38-be03-4616c65a67dd", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: Newbie"
                                , priority: 21, stopProcessing: true));

            // Grats
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bgrats\b).*'$"
                                , command: "", isSilent: true, identifier: "deb3ae91-523a-47d0-a1a0-20950b030e30", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: Grats"
                                , priority: 25, stopProcessing: true));

            // Radio
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?(\[ .* \] )?([\w'-]+|A masked swashbuckler|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bradio(s?)\b).*'$"
                    , command: "", isSilent: true, identifier: "20679c91-2aad-4ad4-9476-3ab60a9ff059", moveTo: TerminalTarget.Terminal2, gag: true, group: "Communication: Radio"
                    , priority: 26, stopProcessing: true));


            // Toasts, TODO: Dragon/Remort toasts ( Dragon ) ( Balanx )
            this.Triggers.Add(new Trigger(pattern: @"^[\a]?([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?"
                                            , command: "", isSilent: true, identifier: "6731ca72-5672-4dab-879b-896b2945805a", moveTo: TerminalTarget.Terminal1, gag: true, group: "dsl-mud.org"));

            // Game events: Keeps
            this.Triggers.Add(new Trigger(pattern: @"^([\w'-]+) has (conquered|unlocked) (.*)!"
                                            , command: "", isSilent: true, identifier: "638a6c30-0a5e-4859-b666-72413f2f1781", moveTo: TerminalTarget.Terminal1, group: "dsl-mud.org"));

            this.Triggers.Add(new Trigger(pattern: @"^(personal> )?Keep Lord clans '(.*) is invading (.*)!'"
                                            , command: "", isSilent: true, identifier: "16f9a412-a6ef-4b5d-a2b7-4dd36f117416", moveTo: TerminalTarget.Terminal1, group: "dsl-mud.org"));

            // Login, find out who the player is and put it into a variable.  Run whoami and then score to scrap initial info.
            this.Triggers.Add(new Trigger(pattern: @"^Welcome to DSL! DSL Loves You! Other muds think you are ugly, they said so"
                                            , command: "whoami;score;aff;#set-prompt", identifier: "bb8d252b-155d-42b6-bc72-1b5e86b551f4", group: "dsl-mud.org"));

            // Re-enable the whois trigger which will disable itself when run again.
            this.Triggers.Add(new Trigger(pattern: @"Reconnecting. Type replay to see missed tells."
                                            , command: "#trigger -i 0000f3e4-9ab9-4f52-9e29-0ba6d88348a6 -e;whoami;score", identifier: "115e3e81-fc66-4f1d-a997-dbeb642ba5c3", group: "dsl-mud.org"));

            this.Triggers.Add(new Trigger(pattern: @"^You are logged in as\: (?<Character>.*?)$"
                                            , command: "whois @Character", identifier: "abe0ea16-1abe-4207-835e-bb10e154dba2", group: "dsl-mud.org"));

            this.Triggers.Add(new Trigger(pattern: @"^Your current war\(s\): (?<Wars>.*?)$"
                                            , command: "", identifier: "80421928-bf25-4ad5-953f-ef196d4d17a5", group: "dsl-mud.org"));

            // One time trigger that will disable itself, it sets the players info from the whois entry (but then
            // disables it so you don't reset those variables when you look at other players entries.  Some Lua will
            // be run here so it only runs a cinfo if the person is in a clan.
            var t = new Trigger(pattern: @"^(\[\s?(?<Level>\d+)\s+([\w-]+) (?<Class>\w+)\]|\[ .*? \])\s(\[Quiet\] )?(\(Wizi@\d\d\)\s)?(\[ (?<Clan>.*?) \] )?(\((?<Kingdom>.*?)\))?"
                                            , command: "", isSilent: false, identifier: "0000f3e4-9ab9-4f52-9e29-0ba6d88348a6", isLua: true, disableAfterTriggered: true, group: "dsl-mud.org");

            t.Command = @"
local clan = lua:GetVariable(""Clan"")

if clan ~= nil and clan ~= """" and clan ~= ""Loner"" and clan ~= ""Renegade"" and clan ~= ""Dragon"" and clan ~= ""Angel"" and clan ~= ""Balanx"" and clan ~= ""Demon"" then
    lua:Send(""cinfo "" .. clan)
end";

            this.Triggers.Add(t);

            // Affects processing.
            _affectsTrigger = new AffectsTrigger();
            _affectsTrigger.SystemTrigger = true;
            _affectsTrigger.Group = "dsl-mud.org";

            var affectsPerm = new AffectsPermanentTrigger(_affectsTrigger);
            affectsPerm.SystemTrigger = true;
            affectsPerm.Group = "dsl-mud.org";

            var affectsClear = new AffectsClearTrigger(_affectsTrigger);
            affectsClear.SystemTrigger = true;
            affectsClear.Group = "dsl-mud.org";

            this.Triggers.Add(affectsClear);
            this.Triggers.Add(_affectsTrigger);
            this.Triggers.Add(affectsPerm);

            // These are of the utmost importance so we'll track these.
            this.Triggers.Add(new Trigger(pattern: @"^You are surrounded by a white aura.$", command: "#partial-affect sanctuary", isSilent: true, identifier: "273cfc6d-340b-4323-a36c-a3147356ba9a", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^The white aura around your body fades.$", command: "#remove-affect sanctuary", isSilent: true, identifier: "cc4c3bfc-06cb-476a-a03d-2cc24e995d17", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^You feel yourself moving more quickly.$", command: "#partial-affect haste", isSilent: true, identifier: "d0d81b22-9248-48b3-ad63-14e935802eae", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^You feel yourself slow down.$", command: "#remove-affect haste", isSilent: true, identifier: "0400ba9f-167d-4e3c-855a-66bbaf74b5c1", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^You feel yourself slowing down.", command: "#remove-affect haste", isSilent: true, identifier: "dae503cb-7089-4a2a-b691-5ad6bbc2ee02", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^You feel less protected.$", command: "#remove-affect protection good;#remove-affect protection neutral;#remove-affect protection evil", isSilent: true, identifier: "15984179-28ca-4b73-b1db-d825892d1927", systemTrigger: true, group: "dsl-mud.org"));
            this.Triggers.Add(new Trigger(pattern: @"^Your protection disappears!$", command: "#remove-affect protection good;#remove-affect protection neutral;#remove-affect protection evil", isSilent: true, identifier: "5fb3c6e1-d516-45e8-a72b-e25dc60886d8", systemTrigger: true, group: "dsl-mud.org"));

            //this.Triggers.Add(new Trigger(@"^Your skin feels soft again.$", "#remove-affect stone skin", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^You feel solid again..$", "#remove-affect pass door", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^You feel less sick.$", "#remove-affect poison", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^You feel less righteous.$", "#remove-affect bless", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^Your rage ebbs.$", "#remove-affect frenzy", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^You feel weaker.$", "#remove-affect giant strength", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^You start gasping for air as you can breath normally again!$", "#remove-affect water breathing", "", true, ""));
            //this.Triggers.Add(new Trigger(@"^ Your magic enhancement fades.$", "#remove-affect imbue", "", true, ""));
        }

        /// <summary>
        /// Code that should fire every tick.  Leave empty if nothing fires.
        /// </summary>
        public override void Tick()
        {
            // If this hasn't initialized a lot of this stuff is going to be null, ditch out here.
            if (!this.Initialized || _affectsTrigger == null)
            {
                return;
            }

            _affectsTrigger?.DecrementAffectDurations();
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
        /// Loads any custom CLR functions we'll pass to Lua.
        /// </summary>
        public void LoadLuaCommands()
        {
            // Only using this to get the type.       
            if (this.LuaCommands.Count > 0)
            {
                return;
            }

            Type t = Type.GetType("Avalon.Plugins.DarkAndShatteredLands.Lua.DslLuaCommands, Avalon.Plugins.DarkAndShatteredLands");
            this.LuaCommands.Add("dsl", t);
        }

        /// <summary>
        /// Loads any custom HashCommand objcts.
        /// </summary>
        public void LoadHashCommands()
        {
            this.HashCommands.Add(new HashCommands.ScanAll());
            this.HashCommands.Add(new HashCommands.RemoveAffect(_affectsTrigger));
            this.HashCommands.Add(new HashCommands.PartialAffect(_affectsTrigger));
            this.HashCommands.Add(new HashCommands.IfNotAffected(_affectsTrigger));
            this.HashCommands.Add(new HashCommands.IfAffected(_affectsTrigger));
            this.HashCommands.Add(new HashCommands.SetPrompt());
            this.HashCommands.Add(new HashCommands.OpenAll());
            this.HashCommands.Add(new HashCommands.DslVersion());
            this.HashCommands.Add(new HashCommands.Edit());
            this.HashCommands.Add(new HashCommands.ConCard());
            this.HashCommands.Add(new HashCommands.Online());
            this.HashCommands.Add(new HashCommands.Wiki());
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
            this.Conveyor.SetVariable("Target", "n/a");
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

        /// <summary>
        /// Creates any database tables that we add custom triggers for.
        /// </summary>
        public async void CreateDbTables()
        {
            await using var conn = new SqliteConnection($"Data Source={this.ProfileSettings.SqliteDatabase}");
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();

            var list = new List<string>();
            list.Add(@"
                                CREATE TABLE IF NOT EXISTS skills (
                                    player_name TEXT NOT NULL,
                                    skill_name TEXT NOT NULL,
                                    value INT,
                                    PRIMARY KEY (player_name, skill_name)
                                );");

            foreach (string sql in list)
            {
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}
