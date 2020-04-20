using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.HashCommands;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    public class Edit : HashCommand
    {
        public Edit(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public Edit()
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#edit";

        public override string Description { get; } = "Edits a mob prog";

        public override async Task ExecuteAsync()
        {
            var argOne = this.Parameters.FirstArgument();
            string argTwo = argOne.Item2;

            if (!argOne.Item2.IsNumeric())
            {
                this.Interpreter.Conveyor.EchoLog("Syntax: #edit <mp> <vnum>", LogType.Error);
                return;
            }

            this.Interpreter.Conveyor.Scrape.Clear();
            this.Interpreter.Conveyor.ScrapeEnabled = true;
            await this.Interpreter.Send($"edit mp {argOne.Item2}");
            await Task.Delay(500);
            await this.Interpreter.Send($"\r\n");
            await Task.Delay(1500);
            this.Interpreter.Conveyor.ScrapeEnabled = false;

            var sb = Argus.Memory.StringBuilderPool.Take();

            using (var reader = new StringReader(this.Interpreter.Conveyor.Scrape.ToString()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Code:") || line.StartsWith("Vnum:") || line.StartsWith("<"))
                    {
                        continue;
                    }

                    sb.AppendLine(line);
                }

            }

            if (sb.ToString().Contains("MPEdit: That vnum does not exist."))
            {
                this.Interpreter.Conveyor.EchoLog("You need to create the mob prog with 'edit mp create <vnum>'", LogType.Information);
                Argus.Memory.StringBuilderPool.Return(sb);
                return;
            }

            var win = new MobProgEditorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Text = sb.ToString().Trim(),
                StatusText = $"Editting mob prog {argOne.Item2}"                
            };

            Argus.Memory.StringBuilderPool.Return(sb);

            var result = win.ShowDialog();

            if (result != null && result.Value)
            {
                // Edit the mob prog, enter the code editor, clear it's contents.
                await this.Interpreter.Send($"edit mp {argOne.Item2}");
                await Task.Delay(250);
                await this.Interpreter.Send($"code");
                await Task.Delay(250);
                await this.Interpreter.Send($".c");

                // Split the prog up into lines
                var lines = win.Text.Split(Environment.NewLine);

                // Send each line with a slight delay as to not get disconnected from the game.
                foreach (string line in lines)
                {
                    await this.Interpreter.Send(line, false, false);
                    await Task.Delay(250);
                }

                await this.Interpreter.Send("@");
            }

            // Set the focus back to the input box.
            this.Interpreter.Conveyor.Focus(FocusTarget.Input);
        }

        public override void Execute()
        {

        }
        
    }
}
