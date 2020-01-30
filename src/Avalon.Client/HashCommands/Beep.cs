using System.Media;
using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Makes the default system sound.
    /// </summary>
    public class Beep : HashCommand
    {
        public Beep(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#beep";

        public override string Description { get; } = "Plays the system sound for a beep.";

        public override void Execute()
        {
            // Default beep
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                SystemSounds.Beep.Play();
                return;
            }

            if (!this.Parameters.IsNumeric())
            {
                Interpreter.EchoText($"--> Syntax: #beep <1, 2, 3, 4, 5>", AnsiColors.Red);
                return;
            }

            int beepType = int.Parse(this.Parameters);

            switch (beepType)
            {
                case 1:
                    SystemSounds.Beep.Play();
                    return;
                case 2:
                    SystemSounds.Asterisk.Play();
                    return;
                case 3:
                    SystemSounds.Exclamation.Play();
                    return;
                case 4:
                    SystemSounds.Hand.Play();
                    return;
                case 5:
                    SystemSounds.Question.Play();
                    return;
            }

            // Default if anything other than 1-5 was entered.
            SystemSounds.Beep.Play();

        }

    }
}
