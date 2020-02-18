using System;

namespace Avalon.Timers
{
    public class ScheduledTask
    {
        public string Command { get; set; }
        public bool IsLua { get; set; }
        public DateTime RunAfter { get; set; } = DateTime.MaxValue;
    }
}
