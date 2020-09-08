using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Argus.Extensions;

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
            int commLines = Interpreter.Conveyor.LineCount(TerminalTarget.Terminal1);
            int oocCommLines = Interpreter.Conveyor.LineCount(TerminalTarget.Terminal2);
            int term3 = Interpreter.Conveyor.LineCount(TerminalTarget.Terminal3);
            int totalLines = gameTermLines + gameTermBackBufferLines + commLines + oocCommLines + term3;

            var sb = Argus.Memory.StringBuilderPool.Take();

            sb.Append("\r\n");
            sb.Append("{C    Game Terminal: ").AppendFormat("{0,8}", gameTermLines.ToString().FormatIfNumber(0)).Append("\r\n");
            sb.Append("{C      Back Buffer: ").AppendFormat("{0,8}", gameTermBackBufferLines.ToString().FormatIfNumber(0)).Append("\r\n");
            sb.Append("{C    Communication: ").AppendFormat("{0,8}", commLines.ToString().FormatIfNumber(0)).Append("\r\n");
            sb.Append("{COOC Communication: ").AppendFormat("{0,8}", oocCommLines.ToString().FormatIfNumber(0)).Append("\r\n");
            sb.Append("{C       Terminal 3: ").AppendFormat("{0,8}", term3.ToString().FormatIfNumber(0)).Append("\r\n");
            sb.Append("{C                   --------\r\n");
            sb.Append("{C      Total Lines: ").AppendFormat("{0,8}", totalLines.ToString().FormatIfNumber(0)).Append("{x\r\n");

            this.Interpreter.Conveyor.EchoText(sb.ToString());

            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}
