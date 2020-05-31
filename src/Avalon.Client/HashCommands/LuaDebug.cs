using Avalon.Common.Interfaces;
using CommandLine;
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
                        sb.AppendLine();
                        sb.Append("{CLua Environment Info:{x\r\n");
                        sb.Append("---------------------------------------------------------------------\r\n");

                        sb.AppendFormat("  {{G* {{WActive Lua Scripts Running:{{x {{C{0}{{x\r\n", lua.ActiveLuaScripts);
                        sb.AppendFormat(" {{G * {{WTotal Lua Scripts Run:{{x {{C{0}{{x\r\n", lua.LuaScriptsRun);
                        sb.AppendFormat(" {{G * {{WGlobal Variable Count:{{x {{C{0}{{x\r\n", globalVariableCount);
                        sb.AppendLine();
                    }

                    if (o.GlobalsList || runAll)
                    {
                        sb.AppendLine();
                        sb.Append("{CLua Global Variables:{x\r\n");
                        sb.Append("---------------------------------------------------------------------\r\n");

                        if (globalVariableCount == 0)
                        {
                            sb.Append("  {G* {WNo global variables are currently stored.{x");
                            sb.AppendLine();
                        }
                        else
                        {
                            foreach (string key in lua.LuaGlobalVariables.Keys)
                            {
                                sb.AppendFormat("  {{G* {{W{0}: {{C{1}{{x\r\n", key, lua.LuaGlobalVariables[key]);
                            }
                        }
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
            [Option('i', "info", Required = false, HelpText = "Information about the current Lua enviornment.")]
            public bool Info { get; set; } = false;

            [Option('g', "global", Required = false, HelpText = "A list of Lua global variables currently stored.")]
            public bool GlobalsList { get; set; } = false;

        }

    }
}