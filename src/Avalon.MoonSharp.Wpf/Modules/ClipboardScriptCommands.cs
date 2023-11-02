/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Clipboard = System.Windows.Clipboard;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Clipboard Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "clipboard")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClipboardScriptCommands
    {
        [MoonSharpModuleMethod(Description = "Copies text to the clipboard.",
            ParameterCount = 1)]
        public void SetText(string text)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.SetText(text));
                return;
            }

            Clipboard.SetText(text, TextDataFormat.Text);
        }

        [MoonSharpModuleMethod(Description = "Copies Unicode text to the clipboard.",
            ParameterCount = 1)]
        public void SetUnicodeText(string text)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.SetUnicodeText(text));
                return;
            }

            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }

        [MoonSharpModuleMethod(Description = "Copies text of type HTML to the clipboard.",
            ParameterCount = 1)]
        public void SetHtml(string text)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.SetHtml(text));
                return;
            }

            Clipboard.SetText(text, TextDataFormat.Html);
        }

        [MoonSharpModuleMethod(Description = "Retrieves text from the clipboard.",
            ParameterCount = 0)]
        public string GetText()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                string text = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    text = Clipboard.GetText();
                });

                return text;
            }

            return Clipboard.GetText();
        }

        [MoonSharpModuleMethod(Description = "If the clipboard currently contains text.",
            ParameterCount = 0)]
        public bool ContainsText()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                bool containsText = false;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    containsText = Clipboard.ContainsText();
                });

                return containsText;
            }

            return Clipboard.ContainsText();
        }

        [MoonSharpModuleMethod(Description = "Permanently adds data to the clipboard so it is available after the program that set it ends.",
            ParameterCount = 0)]
        public void Flush()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(Clipboard.Flush);
                return;
            }

            Clipboard.Flush();
        }
    }
}