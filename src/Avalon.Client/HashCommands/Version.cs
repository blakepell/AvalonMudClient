/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Windows.Media;

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
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            string bit = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            string rendering = "N/A";

#if DEBUG
            string mode = "Debug";
#else
            string mode = "Release";
#endif

            switch (RenderOptions.ProcessRenderMode)
            {
                case System.Windows.Interop.RenderMode.Default:
                    rendering = "Enabled";
                    break;
                case System.Windows.Interop.RenderMode.SoftwareOnly:
                    rendering = "Disabled";
                    break;
            }

            Interpreter.Conveyor.EchoText("\r\n");
            Interpreter.Conveyor.EchoText($"Version:            {version} {bit}\r\n");
            Interpreter.Conveyor.EchoText($"Build:              {mode}\r\n");
            Interpreter.Conveyor.EchoText($"Hardware Rendering: {rendering}\r\n");
        }

    }
}
