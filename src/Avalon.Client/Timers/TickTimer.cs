using Avalon.Common.Interfaces;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using Avalon.Common.Colors;
using Avalon.Common.Models;

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
                // TICK!
                if (Conveyor.ProfileSettings.Debug)
                {
                    var line = new Line
                    {
                        FormattedText = $"--> TICK @ {DateTime.Now}: {((double) _stopWatch.ElapsedMilliseconds / 1000)}", 
                        Text = $"--> TICK @ {DateTime.Now}: {((double) _stopWatch.ElapsedMilliseconds / 1000)}", 
                        ForegroundColor = AnsiColors.Cyan, 
                        IgnoreLastColor = true
                    };

                    Conveyor.EchoText(line, TerminalTarget.Main);
                }

                _lastTimeValue = Conveyor.GetGameTime();

                // Reset if we've shown them the tick warning.
                _echoWarningShown = false;

                _stopWatch.Restart();
                return;
            }

            // TODO - Make this a setting, 40 seconds is the default for DSL (minus the jiggle time where the tick can lag a small amount), make this configurable.
            _secondsUntilTick = 40 - (int)(_stopWatch.ElapsedMilliseconds / 1000);

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
                    FormattedText = $"Tick in 5 seconds.\r\n", 
                    Text = $"Tick in 5 seconds.\r\n", 
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
        private IConveyor Conveyor  { get; set; }

        /// <summary>
        /// Timer to time ticks with.
        /// </summary>
        private readonly DispatcherTimer _tickTimer = new DispatcherTimer();

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