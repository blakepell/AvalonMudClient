using System.Text;
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

        string GetVariable(string key);

        void SetVariable(string key, string value);

        string ReplaceVariablesWithValue(string text);

        void RemoveVariable(string key);

        string Title { get; set; }

        void EchoText(string text);

        void EchoText(string text, AnsiColor foregroundColor, TerminalTarget terminal);

        void EchoText(string text, TerminalTarget target);

        void EchoText(Line line, TerminalTarget target);

        void EchoLog(string text, LogType type);

        void SetTickTime(int value);

        int GetTickTime();

        string GetGameTime();

        void ClearTerminal(TerminalTarget target);

        int LineCount(TerminalTarget target);

        StringBuilder Scrape { get; set; }

        bool ScrapeEnabled { get; set; }

    }
}
