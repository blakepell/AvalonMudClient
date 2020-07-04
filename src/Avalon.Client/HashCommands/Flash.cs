using System;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Flashes the window in the taskbar.
    /// </summary>
    public class Flash : HashCommand
    {

        [DllImport("user32")]
        private static extern int FlashWindow(IntPtr hwnd, bool bInvert);

        public Flash(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#flash";

        public override string Description { get; } = "Makes the window on the Windows taskbar flash.";

        public override void Execute()
        {
            var wih = new WindowInteropHelper(App.MainWindow);
            FlashWindow(wih.Handle, true);
        }

    }

}
