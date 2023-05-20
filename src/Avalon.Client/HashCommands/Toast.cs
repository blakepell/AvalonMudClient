/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Sends a Windows toast notification through a WinForms NotifyIcon control.
    /// </summary>
    public class Toast : HashCommand
    {
        public Toast(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#toast";

        public override string Description { get; } = "Sends a Windows toast notification.";

        public override void Execute()
        {
            App.Toast.ShowNotification("Avalon", this.Parameters);
        }

    }
}
