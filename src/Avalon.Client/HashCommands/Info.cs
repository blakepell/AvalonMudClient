/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using Avalon.Common.Interfaces;
using System.Linq;
using Avalon.Common.Models;

namespace Avalon.HashCommands
{
    public class Info : HashCommand
    {
        public Info(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#info";

        public override string Description { get; } = "Displays client information.";

        public override void Execute()
        {
            // This case is if they specified a window that might exist, we'll find it, edit that.
            var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals("Debug", StringComparison.Ordinal)) as TerminalWindow;

            if (win == null)
            {
                // Window wasn't found, create it.
                win = new TerminalWindow {Name = "Debug"};

                // Add the terminal window to our list.
                this.Interpreter.Conveyor.WindowList.Add(win);
            }

            var sb = Argus.Memory.StringBuilderPool.Take();
            sb.AppendLine("Main Terminal: ");
            sb.Append(" {G*{x IsAutoScrollEnabled = {y").Append(App.MainWindow.GameTerminal.IsAutoScrollEnabled).AppendLine("{x");

            var startLineOne = App.MainWindow.GameTerminal.TextArea.TextView.VisualLines.First().FirstDocumentLine;
            var startLineTwo = App.MainWindow.GameTerminal.TextArea.TextView.VisualLines.First().FirstDocumentLine;
            string startLineText = App.MainWindow.GameTerminal.GetText(startLineOne.Offset, startLineOne.Length);

            var endLineOne = App.MainWindow.GameTerminal.TextArea.TextView.VisualLines.Last().FirstDocumentLine;
            var endLineTwo = App.MainWindow.GameTerminal.TextArea.TextView.VisualLines.Last().LastDocumentLine;
            string endLineText = App.MainWindow.GameTerminal.GetText(endLineOne.Offset, endLineTwo.Length);

            sb.AppendFormat("\r\nStringBuilder Pool: {{y{0}{{x Idle, {{y64{{x Max Idle Capacity\r\n", Argus.Memory.StringBuilderPool.Count().ToString());

            sb.Append($" {{G*{{x Has Wrapped Lines = {{y{App.MainWindow.GameTerminal.HasVisibleWrappedLines.ToString()}{{x\r\n");
            sb.Append($" {{G*{{x Start Visual Line = {{y{startLineOne.LineNumber.ToString()}{{x\r\n");
            sb.Append($" {{G*{{x Start Offsets = {{y{startLineOne.Offset.ToString()}{{x to {{y{startLineTwo.EndOffset.ToString()}{{x\r\n");
            sb.Append($" {{G*{{x Start Visual Line Text = {startLineText}\r\n");
            sb.Append($" {{G*{{x End Visual Line = {{y{endLineOne.LineNumber.ToString()}{{x\r\n");
            sb.Append($" {{G*{{x End Offsets = {{y{endLineOne.Offset.ToString()}{{x to {{y{endLineTwo.EndOffset.ToString()}{{x\r\n");
            sb.Append($" {{G*{{x End Visual Line Text = {endLineText}\r\n");

            sb.AppendLine("Terminal 1: ");
            sb.Append("  {G*{x IsAutoScrollEnabled = {y").Append(App.MainWindow.Terminal1.IsAutoScrollEnabled).AppendLine("{x");

            sb.AppendLine("Terminal 2: ");
            sb.Append("  {G*{x IsAutoScrollEnabled = {y").Append(App.MainWindow.Terminal2.IsAutoScrollEnabled).AppendLine("{x");

            sb.AppendLine("Terminal 2: ");
            sb.Append("  {G*{x IsAutoScrollEnabled = {y").Append(App.MainWindow.Terminal3.IsAutoScrollEnabled).AppendLine("{x");

            win.Title = "Debug Information";
            win.Terminal.Text = "";
            win.AppendAnsi(sb);
            win.Show();
            win.Focus();
            
            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}