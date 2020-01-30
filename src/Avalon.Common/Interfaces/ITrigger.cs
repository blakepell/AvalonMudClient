using System;
using Avalon.Common.Models;

namespace Avalon.Common.Interfaces
{
    public interface ITrigger
    {
        string Pattern { get; set; }

        string Command { get; set; }

        string Character { get; set; }

        string Identifier { get; set; }

        string Group { get; set; }

        bool IsMatch(string line, bool skipVariableSet = false);

        bool IsSilent { get; set; }

        bool VariableReplacement { get; set; }

        bool Gag { get; set; }

        bool Plugin { get; set; }

        bool Enabled { get; set; }

        TerminalTarget MoveTo { get; set; }

        bool HighlightLine { get; set; }

        int Count { get; set; }

        void Execute();

        IConveyor Conveyor { get; set; }

        DateTime LastMatched { get; set; }

    }
}
