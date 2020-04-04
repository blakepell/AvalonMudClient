using Avalon.Common.Interfaces;
using Avalon.HashCommands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{
    /// <summary>
    /// Show the version of the DSL plugin.
    /// </summary>
    public class DslVersion : HashCommand
    {
        public DslVersion(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
        }

        public DslVersion()
        {
            this.IsAsync = false;
        }

        public override string Name { get; } = "#dsl-version";

        public override string Description { get; } = "Version of the DSL plugin.";

        public override void Execute()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string bit = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

#if DEBUG
            string mode = "Debug";
#else
            string mode = "Release";
#endif

            Interpreter.Conveyor.EchoText("\r\n");
            Interpreter.Conveyor.EchoText($"Assembly: {assemblyName}\r\n");
            Interpreter.Conveyor.EchoText($"Version:  {version} {bit}\r\n");
            Interpreter.Conveyor.EchoText($"Build:    {mode}");
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
