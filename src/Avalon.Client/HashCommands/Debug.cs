using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Runs some debugging code.";

        public override void Execute()
        {
            object o = App.MainWindow.SqlTasks.SelectValue("select count(*) from test");
            App.Conveyor.EchoInfo($"{o} items in the test table.");
        }
    }
}