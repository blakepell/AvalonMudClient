using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Argus.Extensions;
using Avalon.Common.Colors;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Echos the line count in the main terminal window.
    /// </summary>
    public class LineCount : HashCommand
    {
        public LineCount(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#line-count";

        public override string Description { get; } = "Echos the line count in the main terminal window.";

        public override void Execute()
        {
            int gameTermLines = Interpreter.Conveyor.LineCount(TerminalTarget.Main);
            int gameTermBackBufferLines = Interpreter.Conveyor.LineCount(TerminalTarget.BackBuffer);
            int commLines = Interpreter.Conveyor.LineCount(TerminalTarget.Communication);
            int oocCommLines = Interpreter.Conveyor.LineCount(TerminalTarget.OutOfCharacterCommunication);
            int totalLines = gameTermLines + gameTermBackBufferLines + commLines + oocCommLines;

            // TODO - EchoLine
            Interpreter.Conveyor.EchoText($"\r\n");
            Interpreter.Conveyor.EchoText($"    Game Terminal: {gameTermLines.ToString().FormatIfNumber(0).PadLeft(8, ' ')}\r\n", AnsiColors.Cyan, TerminalTarget.Main);
            Interpreter.Conveyor.EchoText($"      Back Buffer: {gameTermBackBufferLines.ToString().FormatIfNumber(0).PadLeft(8, ' ')}\r\n", AnsiColors.Cyan, TerminalTarget.Main);
            Interpreter.Conveyor.EchoText($"    Communication: {commLines.ToString().FormatIfNumber(0).PadLeft(8, ' ')}\r\n", AnsiColors.Cyan, TerminalTarget.Main);
            Interpreter.Conveyor.EchoText($"OOC Communication: {oocCommLines.ToString().FormatIfNumber(0).PadLeft(8, ' ')}\r\n", AnsiColors.Cyan, TerminalTarget.Main);
            Interpreter.Conveyor.EchoText($"                   --------\r\n", AnsiColors.Cyan, TerminalTarget.Main);
            Interpreter.Conveyor.EchoText($"      Total Lines: {totalLines.ToString().FormatIfNumber(0).PadLeft(8, ' ')}\r\n", AnsiColors.Cyan, TerminalTarget.Main);
        }

    }
}
