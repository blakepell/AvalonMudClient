using Avalon.Common.Colors;
using System;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Saves the current profile's settings file.
    /// </summary>
    public class Save : HashCommand
    {
        public Save(IInterpreter interp) : base (interp)
        {
        }

        public override string Name { get; } = "#save";

        public override string Description { get; } = "Saves the current profile/settings.";

        public override void Execute()
        {
            try
            {
                App.Settings.SaveSettings();
                Interpreter.EchoText($"--> Settings Saved\r\n", AnsiColors.Cyan);
            }
            catch (Exception ex)
            {
                Interpreter.EchoText($"--> Error\r\n", AnsiColors.Red);
                Interpreter.EchoText(ex.Message);
            }
        }

    }
}