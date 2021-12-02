/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

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
                App.MainWindow.InfoBar.Stance = this.Interpreter.Conveyor.GetVariable("Stance");
                App.MainWindow.InfoBar.Room = this.Interpreter.Conveyor.GetVariable("Room");
                App.MainWindow.InfoBar.Exits = this.Interpreter.Conveyor.GetVariable("ExitsShort");
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError($"ERROR: #update-info-bar => {ex.Message}");
            }
        }

    }
}
