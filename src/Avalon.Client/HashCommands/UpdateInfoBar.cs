using Avalon.Common.Colors;
using System;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    public class UpdateInfoBar : HashCommand
    {
        public UpdateInfoBar(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#update-info-bar";

        public override string Description { get; } = "Updates the info bar with current values from the variables table.";

        public override void Execute()
        {
            try
            {
                // MaxHealth must be set before Health so the color coding is correct.
                App.MainWindow.InfoBar.MaxHealth = int.Parse(this.Interpreter.Conveyor.GetVariable("MaxHealth"));
                App.MainWindow.InfoBar.Health = int.Parse(this.Interpreter.Conveyor.GetVariable("Health"));
                App.MainWindow.InfoBar.MaxMana = int.Parse(this.Interpreter.Conveyor.GetVariable("MaxMana"));
                App.MainWindow.InfoBar.Mana = int.Parse(this.Interpreter.Conveyor.GetVariable("Mana"));
                App.MainWindow.InfoBar.MaxMove = int.Parse(this.Interpreter.Conveyor.GetVariable("MaxMove"));
                App.MainWindow.InfoBar.Move = int.Parse(this.Interpreter.Conveyor.GetVariable("Move"));
                App.MainWindow.InfoBar.Wimpy = int.Parse(this.Interpreter.Conveyor.GetVariable("Wimpy"));
                App.MainWindow.InfoBar.Stance = this.Interpreter.Conveyor.GetVariable("Stance");
                App.MainWindow.InfoBar.Room = this.Interpreter.Conveyor.GetVariable("Room");
                App.MainWindow.InfoBar.Exits = this.Interpreter.Conveyor.GetVariable("ExitsShort");
                App.MainWindow.InfoBar.Time = this.Interpreter.Conveyor.GetVariable("GameTime");
                App.MainWindow.InfoBar.Target = this.Interpreter.Conveyor.GetVariable("Target").Trim();

            }
            catch (Exception ex)
            {
                Interpreter.EchoText($"ERROR: #update-info-bar => {ex.Message}", AnsiColors.Red);
            }
        }

    }
}
