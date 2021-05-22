/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Scripting;
using Avalon.Lua;
using MoonSharp.Interpreter;
using System;
using System.Threading.Tasks;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{
    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        private DynValue Code { get; set; }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Runs some debugging code.";

        public override async Task ExecuteAsync()
        {
            ((Interpreter)this.Interpreter).ScriptHost.MoonSharp.Reset();
        }

        public override void Execute()
        {
//            var cmds = new Avalon.Lua.ScriptCommands(this.Interpreter, new Random());
//            var engine = new NLuaEngine();
//            engine.RegisterObject<ScriptCommands>(cmds, "lua");
            
//            engine.Execute<object>($@"
//lua:LogInfo(""{{GN{{gLua{{x"")
//x
//for i = 1, 5 do
//    lua:LogInfo(tostring(i))
//    lua:Pause(1000)
//end
//");
        }

        //App.Conveyor.EchoText(sw.ElapsedMilliseconds + "\n", TerminalTarget.Terminal1);

        //sw.Restart();
        //for (int i = 0; i < 1000; i++)
        //{
        //    App.MainWindow.Interp.LuaCaller.Execute($"lua:LogInfo('{i.ToString()}')\n");
        //}
        //sw.Stop();

        //App.Conveyor.EchoText(sw.ElapsedMilliseconds + "\n", TerminalTarget.Terminal1);

        //var cmds = new Avalon.Lua.ScriptCommands(this.Interpreter, new Random());
        //var engine = new NLuaEngine(this.Interpreter, cmds);

        //for (int i = 0; i < 10000; i++)
        //{
        //    engine.Execute($"lua:LogInfo('{i.ToString()}')\nreturn 0");
        //}

        //object ret = engine.Execute("local buf = lua:SetVariable(\"Character\", \"LuaGuy\")\nlocal buf = lua:GetVariable(\"Character\")\nlua:LogInfo(buf)");

        //App.Conveyor.EchoInfo(ret.ToString());

        //App.InstanceGlobals.TriggersLockedForUpdate = !App.InstanceGlobals.TriggersLockedForUpdate;

        //if (App.InstanceGlobals.ReplacementTriggers.Count !=
        //    App.Settings.ProfileSettings.ReplacementTriggerList.Count)
        //{
        //    App.Conveyor.EchoError("Replacement triggers don't match.");
        //}
        //else
        //{
        //    App.Conveyor.EchoSuccess("Replace triggers match.");
        //}

        //if (App.InstanceGlobals.GagTriggers.Count !=
        //    App.Settings.ProfileSettings.TriggerList.Count(x => x.Gag))
        //{
        //    App.Conveyor.EchoError("Gag triggers don't match.");
        //}
        //else
        //{
        //    App.Conveyor.EchoSuccess("Gag triggers match.");
        //}

        //if (App.InstanceGlobals.Triggers.Count !=
        //    App.Settings.ProfileSettings.TriggerList.Count)
        //{
        //    App.Conveyor.EchoError("Triggers don't match.");
        //}
        //else
        //{
        //    App.Conveyor.EchoSuccess("Triggers match.");
        //}

        //foreach (var t in App.Settings.ProfileSettings.TriggerList.Where(x => x.Gag && x.Enabled))
        //{
        //    App.InstanceGlobals.GagTriggers.Add(t);
        //}
        //int y = 0;
        //if (this.Parameters == "run")
        //{
        //    //App.MainWindow.Interp.LuaCaller.SharedScript.ClearSources();
        //    DynValue fnc = App.MainWindow.Interp.LuaCaller.SharedScript.Globals.Get("test");
        //    DynValue res = App.MainWindow.Interp.LuaCaller.SharedScript.Call(fnc, DateTime.Now.ToString());

        //    // Works
        //    //DynValue fnc = App.MainWindow.Interp.LuaCaller.SharedScript.Globals.Get("test");
        //    //DynValue res = App.MainWindow.Interp.LuaCaller.SharedScript.Call(fnc, DateTime.Now.ToString());

        //    // Works 2
        //    //DynValue fnc = App.MainWindow.Interp.LuaCaller.SharedScript.Globals.Get("test_two");
        //    //DynValue res = App.MainWindow.Interp.LuaCaller.SharedScript.Call(fnc);

        //    int y = 0;
        //}
        //else
        //{

        //    App.Conveyor.EchoLog("Function Loaded: test(item)", LogType.Information);

        //    _ = App.MainWindow.Interp.LuaCaller.SharedScript.DoString("function test(item)\n    lua.LogInfo(item)\nreturn item\nend", codeFriendlyName: "test_src");

        //    // Works
        //    //this.Function = App.MainWindow.Interp.LuaCaller.SharedScript.DoString("function test(item)\n    lua.LogInfo(item)\nreturn item\nend", codeFriendlyName: "test_src");

        //    // Works 2
        //    //this.Code = App.MainWindow.Interp.LuaCaller.SharedScript.LoadString("function test()\n    lua.LogInfo('test')\nreturn item\nend\nfunction test_two()\nlua.LogInfo('test two')\nend");
        //    //App.MainWindow.Interp.LuaCaller.SharedScript.Call(this.Code);

        //}
        //int x = 0;
    }
}