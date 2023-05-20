/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Interfaces
{
    public interface ICompassWindow : IWindow
    {

        /// <summary>
        /// Sets the angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle"></param>
        void SetAngle(double angle);

        /// <summary>
        /// Sets the angle to a friendly direction (N, NE, E, SE, S, SW, W or NW).
        /// </summary>
        /// <param name="direction"></param>
        void SetDirection(string direction);

    }
}
