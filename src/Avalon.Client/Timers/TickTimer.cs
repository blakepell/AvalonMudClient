﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System.Diagnostics;
using System.Windows.Threading;

namespace Avalon.Timers
{

    /// <summary>
    /// A general purpose tick timer.  It requires some time mechanism from the game to properly work.
    /// In many Diku based muds an hour (and sometimes half hour) is available that can be put into your
    /// prompt.  These times generally change on a tick a thus can be used to sync the TickTimer.
    /// </summary>
    public class TickTimer
    {
        public TickTimer(IConveyor conveyer)
        {
            this.Conveyor = conveyer;

            _stopWatch = new Stopwatch();

            _tickTimer.Interval = TimeSpan.FromSeconds(.25);
            _tickTimer.Tick += TickTimer_Tick;
            _tickTimer.Start();
            _stopWatch.Start();
        }

        /// <summary>
        /// Countdown our tick timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TickTimer_Tick(object sender, object e)
        {
            if (_lastTimeValue != Conveyor.GetGameTime())
            {
                //  Debug tick timing.
                if (Conveyor.ClientSettings.Debug)
                {
                    Conveyor.EchoLog($"--> TICK @ {DateTime.Now}: {((double)_stopWatch.ElapsedMilliseconds / 1000)}", LogType.Debug);
                }

                _lastTimeValue = Conveyor.GetGameTime();

                // Reset if we've shown them the tick warning.
                _echoWarningShown = false;

                // Go ahead and restart now, we'll do some extra tick processing after the stopwatch has been updated.
                _stopWatch.Restart();

                // Run the tick code for each plugin
                foreach (var plugin in App.Plugins)
                {
                    if (!plugin.Initialized)
                    {
                        continue;
                    }

                    plugin.Tick();
                }

                // Tick commands if they're enabled.
                if (App.Settings.ProfileSettings.EnableCommandsOnTick)
                {
                    if (!string.IsNullOrWhiteSpace(App.Settings.ProfileSettings.ExecuteCommandsOnTick))
                    {
                        App.MainWindow.Interp.Send(App.Settings.ProfileSettings.ExecuteCommandsOnTick, false, false);
                    }
                }

                return;
            }

            // Set the duration until the next tick is processed.  The setting by default is 40 but is configurable per profile.
            _secondsUntilTick = App.Settings.ProfileSettings.TickDurationInSeconds - (int)(_stopWatch.ElapsedMilliseconds / 1000);

            // Only update the UI if it's changed and the value is greater than or equal to 0.
            if (Conveyor.GetTickTime() != _secondsUntilTick && _secondsUntilTick >= 0)
            {
                Conveyor.SetTickTime(_secondsUntilTick);
            }

            // Only show a tick warning once if it's in their settings and it's exactly 5 seconds until a tick.
            if (_secondsUntilTick == 5 && _echoWarningShown == false && Conveyor.ProfileSettings.EchoTickWarning)
            {
                var line = new Line
                {
                    FormattedText = "Tick in 5 seconds.\r\n", 
                    IgnoreLastColor = true, 
                    ForegroundColor = AnsiColors.Cyan,
                    ReverseColors = true
                };

                Conveyor.EchoText(line, TerminalTarget.Main);
                _echoWarningShown = true;
            }

        }

        /// <summary>
        /// Whether or not they've seen a tick warning for this tick.
        /// </summary>
        private bool _echoWarningShown = false;

        /// <summary>
        /// This is the in game time.  When the time changes we know a tick has occurred.
        /// </summary>
        private string _lastTimeValue = "";

        /// <summary>
        /// The Conveyor for allowing for interaction with the UI.
        /// </summary>
        private IConveyor Conveyor  { get; }

        /// <summary>
        /// Timer to time ticks with.
        /// </summary>
        private readonly DispatcherTimer _tickTimer = new();

        /// <summary>
        /// A Stopwatch to see how long the tick actually took.
        /// </summary>
        private readonly Stopwatch _stopWatch;

        /// <summary>
        /// The number of estimated seconds left until a tick occurs in the game.
        /// </summary>
        private int _secondsUntilTick = 0;

    }
}