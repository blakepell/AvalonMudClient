/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using System.Collections.Concurrent;
using System.Windows;

namespace Avalon.Timers
{
    public class BatchTasks
    {
        public BatchTasks(Interpreter interp)
        {
            this.Interpreter = interp;
        }

        public async void StartBatch(int secondsBetweenCommands)
        {
            if (this.IsExecuting)
            {
                this.Interpreter.Conveyor.EchoLog("The batch is already executing.", LogType.Warning);
                return;
            }

            if (Tasks.Count == 0)
            {
                this.IsExecuting = false;
                return;
            }

            Tasks.CompleteAdding();
            this.IsExecuting = true;

            while (!Tasks.IsCompleted)
            {
                var task = Tasks.Take();

                if (task.IsLua)
                {
                    await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
                    {
                        _ = await this.Interpreter.ScriptHost.MoonSharp.ExecuteAsync<object>(task.Command);
                    }));
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
                    {
                        await Interpreter.Send(task.Command, false, false);
                    }));
                }

                await Task.Delay(TimeSpan.FromSeconds(secondsBetweenCommands));
            }

            this.IsExecuting = false;
        }

        /// <summary>
        /// Adds a task into the batch task list.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isLua"></param>
        public void AddTask(string command, bool isLua)
        {
            if (this.IsExecuting)
            {
                this.Interpreter.Conveyor.EchoLog("You cannot add new items while a batch is executing.", LogType.Warning);
                return;
            }

            // It's null, initialize it.
            if (this.Tasks == null)
            {
                this.Tasks = new BlockingCollection<BatchTask>(new ConcurrentQueue<BatchTask>());
            }

            // It's already been executed, dispose of it and create a new one.
            if (this.Tasks.IsCompleted)
            {
                this.Tasks.Dispose();
                this.Tasks = new BlockingCollection<BatchTask>(new ConcurrentQueue<BatchTask>());
            }

            // Add the new task.
            var task = new BatchTask { Command = command, IsLua = isLua };
            this.Tasks.TryAdd(task, TimeSpan.FromSeconds(3));
        }

        /// <summary>
        /// Whether the batch is currently executing.
        /// </summary>
        public bool IsExecuting { get; set; } = false;

        /// <summary>
        /// A copy of the current Interpreter so the tasks can be run from here.
        /// </summary>
        private Interpreter Interpreter { get; }

        /// <summary>
        /// All pending scheduled.
        /// </summary>
        public BlockingCollection<BatchTask> Tasks { get; set; } = new(new ConcurrentQueue<BatchTask>());

    }

    /// <summary>
    /// A task that will be executed in batch in the order it was received.
    /// </summary>
    public class BatchTask
    {
        /// <summary>
        /// The command to execute.  This can either be a raw command sent to the game or a Lua
        /// script.  This depends on the value of the IsLua property.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Whether the command is a raw command or a Lua script.
        /// </summary>
        public bool IsLua { get; set; }

    }

}
