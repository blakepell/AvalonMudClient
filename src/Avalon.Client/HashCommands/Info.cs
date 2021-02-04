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

            sb.AppendLine();

            sb.AppendFormat("StringBuilder Memory Pool: {{y{0}{{x Idle, {{y64{{x Max Idle Capacity\r\n", Argus.Memory.StringBuilderPool.Count());
            sb.AppendFormat("Line Memory Pool: {{y{0}{{x Idle of Max {{y{1}{{x Max Idle Capacity\r\n", App.LineMemoryPool.Count(), App.LineMemoryPool.Max);
            sb.AppendFormat("Line Memory Pool: {{y{0}{{x Objects Created {{y{1}{{x Objects Reused\r\n", App.LineMemoryPool.CounterNewObjects, App.LineMemoryPool.CounterReusedObjects);

            App.Conveyor.EchoText(sb.ToString());
            Argus.Memory.StringBuilderPool.Return(sb);
        }
    }
}