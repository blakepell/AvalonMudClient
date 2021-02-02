using Avalon.Common.Interfaces;
using CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalon.HashCommands
{
    public class LuaDebug : HashCommand
    {
        public LuaDebug(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#lua-debug";

        public override string Description { get; } = "Info about the currently Lua environment.";

        public override async Task ExecuteAsync()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    var sb = Argus.Memory.StringBuilderPool.Take();
                    var lua = ((Interpreter)this.Interpreter).LuaCaller;
                    bool runAll = false;
                    int globalVariableCount = lua.LuaGlobalVariables.Count;

                    if (!o.Info && !o.GlobalsList)
                    {
                        runAll = true;
                    }

                    if (o.Info || runAll)
                    {
                        // For fun, calculate the total bytes of stored Lua code for where Lua exists, in aliases, triggers and the global file.
                        int totalBytes = 0;

                        foreach (var a in App.Settings.ProfileSettings.AliasList.Where(x => x.IsLua))
                        {
                            totalBytes += Encoding.UTF8.GetByteCount(a.Command);
                        }

                        foreach (var a in App.Settings.ProfileSettings.TriggerList.Where(x => x.IsLua))
                        {
                            totalBytes += Encoding.UTF8.GetByteCount(a.Command);
                        }

                        totalBytes += Encoding.UTF8.GetByteCount(App.Settings.ProfileSettings.LuaGlobalScript);

                        sb.AppendLine();
                        sb.Append("{CLua Environment Info:{x\r\n");
                        sb.Append("---------------------------------------------------------------------\r\n");

                        sb.AppendFormat("  {{G* {{WActive Lua Scripts Running:{{x {{C{0}{{x\r\n", lua.ActiveLuaScripts);
                        sb.AppendFormat(" {{G * {{WTotal Lua Scripts Run:{{x      {{C{0}{{x\r\n", lua.LuaScriptsRun);
                        sb.AppendFormat(" {{G * {{WGlobal Variable Count:{{x      {{C{0}{{x\r\n", globalVariableCount);
                        sb.AppendFormat(" {{G * {{WLua Error Count:{{x            {{C{0}{{x\r\n", lua.LuaErrorCount);


                        sb.AppendFormat(" {{G * {{WLua Global Code Storage:{{x    {{C{0} bytes{{x\r\n", $"{Encoding.UTF8.GetByteCount(App.Settings.ProfileSettings.LuaGlobalScript):n0}");
                        sb.AppendFormat(" {{G * {{WLua Overall Code Storage:{{x   {{C{0} bytes{{x\r\n", $"{totalBytes:n0}");
                    }

                    if (o.GlobalsList || runAll)
                    {
                        sb.AppendLine();
                        sb.Append("{CLua Global Variables:{x\r\n");
                        sb.Append("---------------------------------------------------------------------\r\n");

                        if (globalVariableCount == 0)
                        {
                            sb.Append("  {G* {WNo global variables are currently stored.{x");
                        }
                        else
                        {
                            foreach (string key in lua.LuaGlobalVariables.Keys)
                            {
                                sb.AppendFormat("  {{G* {{W{0}: {{C{1}{{x\r\n", key, lua.LuaGlobalVariables[key]);
                            }
                        }

                        sb.AppendLine();
                    }

                    this.Interpreter.Conveyor.EchoText(sb.ToString());

                    Argus.Memory.StringBuilderPool.Return(sb);
                });

            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        public class Arguments
        {
            [Option('i', "info", Required = false, HelpText = "Information about the current Lua environment.")]
            public bool Info { get; set; } = false;

            [Option('g', "global", Required = false, HelpText = "A list of Lua global variables currently stored.")]
            public bool GlobalsList { get; set; } = false;

        }

    }
}