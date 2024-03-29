﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Colors;
using Avalon.Common.Models;
using Avalon.Common.Settings;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Interface for any UI interactions that should be supported.
    /// </summary>
    public interface IConveyor
    {
        ProfileSettings ProfileSettings { get; }

        AvalonSettings ClientSettings { get; }

        string GetVariable(string key);

        void SetVariable(string key, string value);

        void SetVariable(string key, string value, string color);

        void HideVariable(string key);

        void ShowVariable(string key);

        string ReplaceVariablesWithValue(string text);

        void RemoveVariable(string key);

        string Title { get; set; }

        void EchoText(string text);

        void EchoText(string text, AnsiColor foregroundColor, TerminalTarget terminal);

        void EchoText(string text, TerminalTarget target);

        void EchoText(Line line, TerminalTarget target);

        void EchoText(string text, string windowName);

        void EchoText(Line line, string windowName);

        void EchoLog(string text, LogType type);

        void EchoDebug(string text);

        void EchoInfo(string text);

        void EchoSuccess(string text);

        void EchoWarning(string text);

        void EchoError(string text);

        string GetText(TerminalTarget target, bool removeColors);

        string GetSelectedText(TerminalTarget target, bool removeColors);

        void SetTickTime(int value);

        int GetTickTime();

        string GetGameTime();

        void ClearTerminal(TerminalTarget target);

        int LineCount(TerminalTarget target);

        bool EnableGroup(string groupName);

        bool DisableGroup(string groupName);

        ITrigger FindTrigger(string id);

        Task<string> InputBox(string caption, string title);

        Task<string> InputBox(string caption, string title, string prepopulateText);

        void InputBoxToVariable(string caption, string title, string variable);

        void Focus(FocusTarget target);

        StringBuilder Scrape { get; set; }

        bool ScrapeEnabled { get; set; }

        void ExecuteCommand(string cmd);

        Task ExecuteCommandAsync(string cmd);

        void ExecuteLua(string lua);

        Task ExecuteLuaAsync(string lua);

        void SortTriggersByPriority();

        void ProgressBarRepeaterClear();

        void ProgressBarRepeaterAdd(string key, int value, int maximum, string text);

        void ProgressBarRepeaterAdd(string key, int value, int maximum, string text, string command);

        void ProgressBarRemove(string key);

        void SetCustomTabVisible(CustomTab tab, bool visible);

        void SetCustomTabLabel(CustomTab tab, string label);

        string ProgressBarRepeaterStatusText { get; set; }

        bool ProgressBarRepeaterStatusVisible { get; set; }

        WindowPosition GetWindowPosition { get; }

        List<IWindow> WindowList { get; set; }
    }
}
