/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Colors
{
    public class DarkPurple : AnsiColor
    {
        public override string AnsiCode => "\x1B[0;35m";

        public override string MudColorCode => "{m";

        public override string Name => "Dark Purple";
    }
}