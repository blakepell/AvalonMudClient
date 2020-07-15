using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Reverses a set of directions.
    /// </summary>
    public class ReverseDirections : HashCommand
    {
        public ReverseDirections(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#reverse-path";

        public override string Description { get; } = "Reverse's a fast walk path";

        public override void Execute()
        {
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                this.Interpreter.Conveyor.EchoLog("A set of fast walk direction is required as the parameter.", LogType.Warning);
                return;
            }

            var forwardList = new List<string>()
            {
                "n", "e", "s", "w", "u", "d", "nw", "ne", "sw", "se"
            };

            var reverseList = new List<string>()
            {
                "s", "w", "n", "e", "d", "u", "se", "sw", "ne", "nw"
            };

            this.Parameters = Utilities.Utilities.Speedwalk(this.Parameters);

            var sb = Argus.Memory.StringBuilderPool.Take();
            var path = this.Parameters.Split(';').Reverse().ToList();

            for (int i = 0; i < path.Count; i++)
            {
                // Swap the reverse direction in
                int pos = forwardList.IndexOf(path[i]);

                if (pos == -1)
                {
                    continue;
                }

                path[i] = reverseList[pos];
            }

            // This will be each individual step (or a number in the same direction)
            foreach (string step in path)
            {
                if (step.ContainsNumber())
                {
                    string stepsStr = "";
                    string direction = "";

                    // Pluck the number off the front (e.g. 4n)
                    foreach (char c in step)
                    {
                        if (char.IsNumber(c))
                        {
                            stepsStr += c;
                        }
                        else
                        {
                            direction += c;
                        }
                    }

                    // The number of steps to repeat this specific step
                    int steps = int.Parse(stepsStr);

                    for (int i = 1; i <= steps; i++)
                    {
                        sb.Append(direction);
                        sb.Append(';');
                    }

                }
                else
                {
                    // No number, put it in verbatim.
                    sb.Append(step);
                    sb.Append(';');
                }

            }

            sb.TrimEnd(';');

            // Finally, look for parens and turn semi-colons in between there into spaces.  This might be hacky but should
            // allow for commands in the middle of our directions as long as they're surrounded by ().
            bool on = false;

            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '(')
                {
                    on = true;
                }
                else if (sb[i] == ')')
                {
                    on = false;
                }

                if (on == true && sb[i] == ';')
                {
                    sb[i] = ' ';
                }
            }

            // Now that the command has been properly placed, remove any parents.
            sb.Replace("(", "").Replace(")", "");

            this.Interpreter.Conveyor.EchoText(sb.ToString());

            Argus.Memory.StringBuilderPool.Return(sb);
        }

    }
}
