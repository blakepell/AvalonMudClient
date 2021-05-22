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
    public class Black : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;30m";

        public override string MudColorCode => "{k";

        public override string Name => "Black";

    }
}