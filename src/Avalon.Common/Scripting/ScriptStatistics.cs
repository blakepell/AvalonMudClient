/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// A class that tracks statistics for the scripting environment as a whole.
    /// </summary>
    public class ScriptStatistics : INotifyPropertyChanged
    {
        /// <summary>
        /// An object for thread locking access to resources.
        /// </summary>
        private object _lockObject = new();

        private int _scriptsActive = 0;

        /// <summary>
        /// The number of scripts that are currently active in the <see cref="ScriptHost"/>.
        /// </summary>
        public int ScriptsActive
        {
            get => _scriptsActive;
            set
            {
                lock (_lockObject)
                {
                    _scriptsActive = value;
                }

                OnPropertyChanged(nameof(ScriptsActive));
            }
        }

        private int _runCount = 0;

        /// <summary>
        /// The total number of scripts that have been run.
        /// </summary>
        public int RunCount
        {
            get => _runCount;
            set
            {
                lock (_lockObject)
                {
                    _runCount = value;
                }

                OnPropertyChanged(nameof(RunCount));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
