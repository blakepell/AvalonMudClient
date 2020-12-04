using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Linq;

namespace Avalon.Timers
{

    /// <summary>
    /// Tasks (raw commands or Lua scripts) that can be scheduled to run at some point in the future.
    /// </summary>
    public class ScheduledTasks
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interp"></param>
        public ScheduledTasks(Interpreter interp)
        {
            this.Interpreter = interp;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += ScheduledTasks_Tick;
        }

        /// <summary>
        /// The timer method that runs if tasks exist in the Queue.  This currently updates every one second but
        /// only runs if tasks are in the queue.  A task will execute after it's RunAfter date in the order by the
        /// timestamp.  Once a task executes it's automatically removed, once the last task executes the timer
        /// disables itself until new items are added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScheduledTasks_Tick(object sender, object e)
        {
            var tasks = this.Dequeue();

            if (tasks == null)
            {
                return;
            }

            foreach (var task in tasks)
            {
                if (task.IsLua)
                {
                    await this.Interpreter.LuaCaller.ExecuteAsync(task.Command);
                }
                else
                {
                    await Interpreter.Send(task.Command);
                }
            }
        }

        /// <summary>
        /// Adds a task into the scheduler.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isLua"></param>
        /// <param name="runAfter"></param>
        public void AddTask(string command, bool isLua, DateTime runAfter)
        {
            var task = new ScheduledTask {Command = command, IsLua = isLua, RunAfter = runAfter};
            this.Tasks.Add(task);

            // There is something in the timer, make sure it's enabled.
            _timer.IsEnabled = true;
        }

        /// <summary>
        /// Dequeue and return all tasks that are past their RunAfter time. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScheduledTask> Dequeue()
        {
            if (Tasks.Count == 0)
            {
                return null;
            }

            var list = new List<ScheduledTask>();

            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                if (Tasks[i].RunAfter < DateTime.Now)
                {
                    // Add the task that needs to be executed to the return list and then remove it
                    // from our queue.
                    list.Add(Tasks[i]);
                    Tasks.RemoveAt(i);
                }
            }

            // There are no more tasks, disable the timer so that it's not burning CPU while there's
            // nothing to process.
            if (Tasks.Count == 0)
            {
                _timer.IsEnabled = false;
            }

            return list.OrderBy(x => x.RunAfter);
        }

        /// <summary>
        /// Clears all tasks from the scheduled tasks queue.
        /// </summary>
        public void ClearTasks()
        {
            this.Tasks.Clear();
            _timer.IsEnabled = false;
        }

        /// <summary>
        /// A copy of the current Interpreter so the tasks can be run from here.
        /// </summary>
        private Interpreter Interpreter { get; }

        /// <summary>
        /// The dispatch timer used for checking the queue.
        /// </summary>
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        /// <summary>
        /// All pending scheduled.
        /// </summary>
        public List<ScheduledTask> Tasks { get; set; } = new List<ScheduledTask>();

    }

    /// <summary>
    /// A task that should be run after a given date/time.
    /// </summary>
    public class ScheduledTask
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

        /// <summary>
        /// The date that the task should run after.
        /// </summary>
        public DateTime RunAfter { get; set; } = DateTime.MaxValue;
    }

}