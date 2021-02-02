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
            sb.AppendLine("Main Terminal: ");
            sb.Append("  {G*{x IsAutoScrollEnabled={y").Append(App.MainWindow.GameTerminal.IsAutoScrollEnabled).AppendLine("{x");

            sb.AppendLine("Terminal 1: ");
            sb.Append("  {G*{x IsAutoScrollEnabled={y").Append(App.MainWindow.Terminal1.IsAutoScrollEnabled).AppendLine("{x");

            sb.AppendLine("Terminal 2: ");
            sb.Append("  {G*{x IsAutoScrollEnabled={y").Append(App.MainWindow.Terminal2.IsAutoScrollEnabled).AppendLine("{x");

            sb.AppendLine("Terminal 2: ");
            sb.Append("  {G*{x IsAutoScrollEnabled={y").Append(App.MainWindow.Terminal3.IsAutoScrollEnabled).AppendLine("{x");


            sb.AppendFormat("\r\nStringBuilder Pool: {{y{0}{{x Idle, {{y64{{x Max Idle Capacity\r\n", Argus.Memory.StringBuilderPool.Count());
            App.Conveyor.EchoText(sb.ToString());

            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}