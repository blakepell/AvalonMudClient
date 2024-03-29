﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Represents a hash command.
    /// </summary>
    public interface IHashCommand
    {

        string Name { get; }

        string Description { get; }

        string Parameters { get; set; }

        void Execute();

        Task ExecuteAsync();

        IInterpreter Interpreter { get; set; }

        bool IsAsync { get; set; }

    }
}
