using System;
using System.Reflection;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos back the parameter the user sends.
    /// </summary>
    public class Version : HashCommand
    {
        public Version(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#version";

        public override string Description { get; } = "Shows version information for this application.";

        public override void Execute()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string bit = Environment.Is64BitProcess ? "64-bit" : "32-bit";

#if DEBUG
            string mode = "Debug";
#else
            string mode = "Release";
#endif

            Interpreter.Conveyor.EchoText("\r\n");
            Interpreter.Conveyor.EchoText($"Version:  {version} {bit}\r\n");
            Interpreter.Conveyor.EchoText($"Build:    {mode}");
        }

    }
}
