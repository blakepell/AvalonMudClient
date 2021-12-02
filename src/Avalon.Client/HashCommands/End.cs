/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Windows;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Exits the application.
    /// </summary>
    public class End : HashCommand
    {
        public End(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#end";

        public override string Description { get; } = "Exits the application.";

        public override void Execute()
        {
            Application.Current.Shutdown(0);
        }

    }
}