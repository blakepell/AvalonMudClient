using Argus.Memory;
using Avalon.Models;
using Mosaic.UI.Wpf.Logging;
using System.Windows;

namespace Avalon
{
    public partial class App
    {
        static App()
        {
            AppFolder = @"Apps\AvalonMudClient";
            ProgId = "AvalonMudClient";
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(e);
            var settings = AppServices.GetRequiredService<AppSettings>();
            var viewModel = AppServices.GetRequiredService<AppViewModel>();
            viewModel.AppSettings = settings;

            var version = typeof(App).Assembly.GetName().Version;
            Logger.LogInfo($"Starting {viewModel.Title} version {version}.");
            viewModel.McpServer.Attach(settings);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            AppServices.GetRequiredService<AppViewModel>().McpServer.Shutdown();
            Scripting.AudioScriptCommands.ReleasePlayers();
            base.OnExit(e);
        }
    }
}
