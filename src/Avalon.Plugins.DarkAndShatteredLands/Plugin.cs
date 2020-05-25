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
            this.Conveyor.SetCustomTabLabel(CustomTab.Tab1, "In Character");

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

            // IC Channels: Clan gossip, clan, gossip, ask, answer, kingdom, group tells, tells, auction, pray, grats, quest (quote at the end)
            this.Triggers.Add(new Trigger(@"^[\a]?(\[ .* \] )?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+|\(Wizi@\d\d\) \(Imm\) [\w'-]+) (\bclan gossip(s?)\b|\bclan(s?)\b|\bgossip(s?)\b|\bask(s?)\b|\banswers(s?)\b|\btell(s?)\b|\bBloodbath(s?)\b|\bpray(s?)\b|\bgrats\b|\bauction(s?)\b|\bquest(s?)\b|\bradio(s?)\b|\bimm(s?)\b).*'$", "", "", true, "8b0bc970-08de-498e-9866-8e1aec458c08", TerminalTarget.Terminal1, true));
            this.Triggers.Add(new Trigger(@"^[\a]?(\[ .* \] )?(?!.*OOC).*Kingdom: .*$", "", "", true, "1dcf2580-da86-45b5-880f-36f9468891c1", TerminalTarget.Terminal1, true));
            this.Triggers.Add(new Trigger(@"\((Admin|Coder)\) \(Imm\) [\w'-]+:", "", "", true, "b5c8f16b-31d1-48e9-a895-fda1be732051", TerminalTarget.Terminal1, true));

            // OOC Channels: OOC, OOC Clan, OOC Kingdom, Newbie
            this.Triggers.Add(new Trigger(@"^[\a]?(\(.*\)?)?([\w'-]+|The ghost of [\w'-]+|\(An Imm\)|\(Imm\) [\w'-]+) (OOC|\[Newbie\]).*$", "", "", true, "4c7efde0-6f9a-4429-8b5c-edf23e60e61f", TerminalTarget.Terminal2, true));

            // Shared channels (Shalonesti, Conclave)
            this.Triggers.Add(new Trigger(@"^\((Shalonesti|OOC Shalonesti|Clave|OOC Clave)\).*$", "", "", true, "bd8f5ed0-3122-4091-bb23-db2f589e7cf0", TerminalTarget.Terminal1, true));

            // Toasts, TODO: Dragon/Remort toasts ( Dragon ) ( Balanx )
            this.Triggers.Add(new Trigger(@"^[\a]?([\[\(](.*?)[\]\)])?[ ]{0,}([\w'-]+) got (.*?) by (.*?) ([\[\(] (.*?) [\]\)])?[ ]{0,}([\(]Arena[\)])?", "", "", true, "6731ca72-5672-4dab-879b-896b2945805a", TerminalTarget.Terminal1, true));

            // Game events: Keeps
            this.Triggers.Add(new Trigger(@"^([\w'-]+) has (conquered|unlocked) (.*)!", "", "", true, "638a6c30-0a5e-4859-b666-72413f2f1781", TerminalTarget.Terminal1, false));
            this.Triggers.Add(new Trigger(@"^(personal> )?Keep Lord clans '(.*) is invading (.*)!'", "", "", true, "16f9a412-a6ef-4b5d-a2b7-4dd36f117416", TerminalTarget.Terminal1, false));

            // Login, find out who the player is and put it into a variable.  Run whoami and then score to scrap initial info.
            this.Triggers.Add(new Trigger(@"^Welcome to DSL! DSL Loves You! Other muds think you are ugly, they said so", "whoami;score;aff;#set-prompt"));

            // Re-enable the whois trigger which will disable itself when run again.
            this.Triggers.Add(new Trigger(@"Reconnecting. Type replay to see missed tells.", "#trigger -i 0000f3e4-9ab9-4f52-9e29-0ba6d88348a6 -e;whoami;score"));

            this.Triggers.Add(new Trigger(@"^You are logged in as\: (?<Character>.*?)$", "whois @Character"));
            this.Triggers.Add(new Trigger(@"\[Exits: (?<Exits>.*?)  \]", ""));
            this.Triggers.Add(new Trigger(@"\[Exits:  \]", "#set ExitsShort none;#set Exits none", "", true, "f8efc60a-adcf-46fc-b230-bcf3a4fee8a2"));
            this.Triggers.Add(new Trigger(@"^Your current war\(s\): (?<Wars>.*?)$", ""));

            // One time trigger that will disable itself, it sets the players info from the whois entry (but then
            // disables it so you don't reset those variables when you look at other players entries.  Some Lua will
            // be run here so it only runs a cinfo if the person is in a clan.
            var t = new Trigger(@"^(\[\s?(?<Level>\d+)\s+([\w-]+) (?<Class>\w+)\]|\[ .*? \])\s(\[Quiet\] )?(\(Wizi@\d\d\)\s)?(\[ (?<Clan>.*?) \] )?(\((?<Kingdom>.*?)\))?", "", "", false, "0000f3e4-9ab9-4f52-9e29-0ba6d88348a6")
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

            // Affects processing.
            _affectsTrigger = new AffectsTrigger();
            var affectsPerm = new AffectsPermanentTrigger(_affectsTrigger);
            var affectsClear = new AffectsClearTrigger(_affectsTrigger);

            this.Triggers.Add(affectsClear);
            this.Triggers.Add(_affectsTrigger);
            this.Triggers.Add(affectsPerm);

            // These are of the utmost importance so we'll track these.
            this.Triggers.Add(new Trigger(@"^You are surrounded by a white aura.$", "#partial-affect sanctuary", "", true, ""));
            this.Triggers.Add(new Trigger(@"^The white aura around your body fades.$", "#remove-affect sanctuary", "", true, ""));

            this.Triggers.Add(new Trigger(@"^You feel yourself moving more quickly.$", "#partial-affect haste", "", true, ""));
            this.Triggers.Add(new Trigger(@"^You feel yourself slow down.$", "#remove-affect haste", "", true, ""));
            this.Triggers.Add(new Trigger(@"^You feel yourself slowing down.", "#remove-affect haste", "", true, ""));

            this.Triggers.Add(new Trigger(@"^You feel less protected.$", "#remove-affect protection good;#remove-affect protection neutral;#remove-affect protection evil", "", true, ""));
            this.Triggers.Add(new Trigger(@"^Your protection disappears!$", "#remove-affect protection good;#remove-affect protection neutral;#remove-affect protection evil", "", true, ""));

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

        /// <summary>
        /// Creates any database tables that we add custom triggers for.
        /// </summary>
        public async void CreateDbTables()
        {
            await using (var conn = new SqliteConnection($"Data Source={this.ProfileSettings.SqliteDatabase}"))
            {
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
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

    }
}
