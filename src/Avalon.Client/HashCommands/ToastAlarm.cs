/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Media;
using System.Windows.Forms;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Sends a Windows toast notification through a WinForms NotifyIcon control.
    /// </summary>
    public class ToastAlarm : HashCommand
    {
        public ToastAlarm(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#toast-alarm";

        public override string Description { get; } = "Sends a Windows toast notification.";

        public override void Execute()
        {
            SystemSounds.Exclamation.Play();
            App.Toast.ShowNotification("Avalon", this.Parameters, ToolTipIcon.Warning);
        }

    }
}
