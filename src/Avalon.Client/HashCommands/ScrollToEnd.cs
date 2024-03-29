﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Scrolls the main game terminal to the end.
    /// </summary>
    public class ScrollToEnd : HashCommand
    {
        public ScrollToEnd(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#scroll-to-end";

        public override string Description { get; } = "Scrolls the main game terminal to the end.";

        public override void Execute()
        {
            double visualTop = App.MainWindow.GameTerminal.TextArea.TextView.GetVisualTopByDocumentLine(App.MainWindow.GameTerminal.Document.Lines.Count);
            App.MainWindow.GameTerminal.ScrollToVerticalOffset(visualTop);
            App.MainWindow.Terminal1.ScrollToLastLine();
            App.MainWindow.Terminal2.ScrollToLastLine();
            App.MainWindow.Terminal3.ScrollToLastLine();
        }

    }
}
