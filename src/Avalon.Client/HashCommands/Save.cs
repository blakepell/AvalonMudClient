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
                Interpreter.Conveyor.EchoLog("Settings Saved", Common.Models.LogType.Success);
            }
            catch (Exception ex)
            {
                Interpreter.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);
            }
        }
    }
}