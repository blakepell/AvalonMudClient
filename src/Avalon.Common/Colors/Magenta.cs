﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Colors
{
    public class Magenta : AnsiColor
    {
        public override string AnsiCode => "\x1B[38;5;61m";

        public override string MudColorCode => "{u";

        public override string Name => "Magenta";
    }
}