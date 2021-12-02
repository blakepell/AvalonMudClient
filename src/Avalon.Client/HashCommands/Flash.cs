/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Flashes the window in the task bar.
    /// </summary>
    public class Flash : HashCommand
    {

        [DllImport("user32")]
        private static extern int FlashWindow(IntPtr hwnd, bool bInvert);

        public Flash(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#flash";

        public override string Description { get; } = "Makes the window on the Windows task bar flash.";

        public override void Execute()
        {
            var wih = new WindowInteropHelper(App.MainWindow);
            FlashWindow(wih.Handle, true);
        }

    }

}
