using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Linq;

namespace Avalon.Timers
{

    public class ScheduledTasks
    {

        public ScheduledTasks(Interpreter interp)
        {
            this.Interpreter = interp;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += ScheduledTasks_Tick;
        }

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

        public void AddTask(string command, bool isLua, DateTime runAfter)
        {
            var task = new ScheduledTask {Command = command, IsLua = isLua, RunAfter = runAfter};
            this.Tasks.Add(task);

            // There is something in the timer, make sure it's enabled.
            _timer.IsEnabled = true;
        }

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

        private Interpreter Interpreter { get; set; }

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public List<ScheduledTask> Tasks { get; set; } = new List<ScheduledTask>();

    }
}