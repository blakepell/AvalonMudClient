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
    /// Clears all tasks in the scheduled tasks queue.
    /// </summary>
    public class TaskClear : HashCommand
    {
        public TaskClear(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#task-clear";

        public override string Description { get; } = "Clears all tasks in the scheduled tasks queue.";

        public override void Execute()
        {
            App.MainWindow.ScheduledTasks.ClearTasks();
        }

    }
}