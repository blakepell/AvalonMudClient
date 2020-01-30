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
            string platform = "Windows Presentation Foundation (WPF)";

#if DEBUG
            string mode = "Debug";
#else
            string mode = "Release";
#endif
            Interpreter.EchoText($"\r\nVersion: {mode} {version} | {platform}");
        }

    }
}
