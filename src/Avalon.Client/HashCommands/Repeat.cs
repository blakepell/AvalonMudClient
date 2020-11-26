using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Repeat's a command N number of times.
    /// </summary>
    public class Repeat : HashCommand
    {
        public Repeat(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#repeat";

        public override string Description { get; } = "Repeat's a command N times.";

        public override void Execute()
        {
            var argOne = this.Parameters.FirstArgument();
            string argTwo = argOne.Item2;

            if (!argOne.Item1.IsNumeric())
            {
                Interpreter.Conveyor.EchoError($"--> Syntax: #repeat <number of times> <command>");

            }

            int repeatTimes = int.Parse(argOne.Item1);
            var sb = Argus.Memory.StringBuilderPool.Take();

            for (int i = 0; i < repeatTimes; i++)
            {
                sb.Clear();
                sb.Append(argTwo).Replace("@count", (i + 1).ToString()).Replace("@index", i.ToString());
                Interpreter.Send(sb.ToString());
            }

            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}
