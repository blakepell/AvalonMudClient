﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System.Text;
using System.Windows;
using Avalon.Colors;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class TerminalWindow : ITerminalWindow
    {
        /// <summary>
        /// The value of the Lua text editor.
        /// </summary>
        public string Text
        {
            get => Terminal.Text;
            set => Terminal.Text = value;
        }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// The window type.
        /// </summary>
        public WindowType WindowType { get; set; } = WindowType.TerminalWindow;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TerminalWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminalWindowWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Fires when the window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminalWindowWindow_Closed(object sender, System.EventArgs e)
        {
            // Remove this specific window from the shared Conveyor's window list.
            App.Conveyor.WindowList.Remove(this);
        }

        /// <summary>
        /// Appends text to the terminal.
        /// </summary>
        public void AppendText(string text)
        {
            Terminal.Append(text, true);
        }

        /// <summary>
        /// Appends a line to the terminal.
        /// </summary>
        /// <param name="line"></param>
        public void AppendText(Line line)
        {
            Terminal.Append(line);
        }

        /// <summary>
        /// Appends a StringBuilder to the terminal.
        /// </summary>
        /// <param name="sb"></param>
        public void AppendText(StringBuilder sb)
        {
            Terminal.Append(sb, true);
        }

        /// <summary>
        /// Appends text to the terminal window and converts mud color codes into ANSI color.
        /// </summary>
        /// <param name="text"></param>
        public void AppendAnsi(string text)
        {
            var sb = Argus.Memory.StringBuilderPool.Take();
            sb.AppendLine(text);
            Colorizer.MudToAnsiColorCodes(sb);
            Terminal.AppendAnsi(sb);
            Argus.Memory.StringBuilderPool.Return(sb);
        }

        /// <summary>
        /// Appends text to the terminal window and converts mud color codes into ANSI color.
        /// </summary>
        /// <param name="sb"></param>
        public void AppendAnsi(StringBuilder sb)
        {
            Colorizer.MudToAnsiColorCodes(sb);
            Terminal.AppendAnsi(sb);
        }

        /// <summary>
        /// Activates the Window and brings it to the forefront and focused.
        /// </summary>
        void IWindow.Activate()
        {
            base.Activate();
        }

    }
}
