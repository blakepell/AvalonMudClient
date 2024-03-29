﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Colors
{
    public class DarkGreen : AnsiColor
    {
        public override string AnsiCode => "\x1B[0;32m";

        public override string MudColorCode => "{g";

        public override string Name => "Dark Green";
    }
}