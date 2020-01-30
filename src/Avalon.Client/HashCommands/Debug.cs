using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Command used for debugging.
    /// </summary>
    public class Debug : HashCommand
    {
        public Debug(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#debug";

        public override string Description { get; } = "Command used for developer debugging.";

        public override void Execute()
        {
            //App.MainWindow.WriteLine()
            //App.AppSettingsProvider.AppSettings.TriggersEnabled = !App.AppSettingsProvider.AppSettings.TriggersEnabled;
            //App.MainWindow.WriteLine($"--> Triggers Enabled: {App.AppSettingsProvider.AppSettings.TriggersEnabled}");

            //App.AppSettingsProvider.AppSettings.Debug = !App.AppSettingsProvider.AppSettings.Debug;
            //App.MainWindow.WriteLine($"--> Debug: {App.AppSettingsProvider.AppSettings.Debug}");
            //App.MainWindow.WriteLine($"--> Triggers Enabled: {App.AppSettingsProvider.AppSettings.TriggersEnabled}");

            //Utilities.SortAffects();

            //foreach (var a in App.MainWindow.Affects)
            //{
            //    App.MainWindow.WriteLine(a.Display());
            //}
        }

    }
}
