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
