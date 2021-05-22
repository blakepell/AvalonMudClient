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
    public class Red : AnsiColor
    {
        public override string AnsiCode => "\x1B[1;31m";

        public override string MudColorCode => "{R";

        public override string Name => "Red";

    }
}
