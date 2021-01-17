using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    public class Info : HashCommand
    {
        public Info(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#info";

        public override string Description { get; } = "Displays client information.";

        public override void Execute()
        {
            var sb = Argus.Memory.StringBuilderPool.Take();
            sb.AppendLine($"Main Terminal: ");
            sb.AppendLine($"  {{G*{{x IsAutoScrollEnabled={App.MainWindow.GameTerminal.IsAutoScrollEnabled}");

            sb.AppendLine($"Terminal 1: ");
            sb.AppendLine($"  {{G*{{x IsAutoScrollEnabled={App.MainWindow.Terminal1.IsAutoScrollEnabled}");

            sb.AppendLine($"Terminal 2: ");
            sb.AppendLine($"  {{G*{{x IsAutoScrollEnabled={App.MainWindow.Terminal2.IsAutoScrollEnabled}");

            sb.AppendLine($"Terminal 2: ");
            sb.AppendLine($"  {{G*{{x IsAutoScrollEnabled={App.MainWindow.Terminal3.IsAutoScrollEnabled}");

            App.Conveyor.EchoText(sb.ToString());

            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}