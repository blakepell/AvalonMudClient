using System.Windows;
using System.Windows.Controls;
using Avalon.Models;

namespace Avalon.Controls;

/// <summary>
/// The main window's status bar, showing the active game terminal's toggles, connection and the
/// terminal font size.
/// </summary>
public partial class MainWindowStatusBar : UserControl
{
    /// <summary>
    /// Initializes a new instance of the MainWindowStatusBar class.
    /// </summary>
    public MainWindowStatusBar()
    {
        InitializeComponent();
    }

    private MudTerminal? ActiveTerminal => (DataContext as AppViewModel)?.ActiveTerminal;

    private void ButtonToggleTriggers_Click(object sender, RoutedEventArgs e) => ActiveTerminal?.ToggleTriggers();

    private void ButtonToggleAliases_Click(object sender, RoutedEventArgs e) => ActiveTerminal?.ToggleAliases();

    private void ButtonToggleMcpServer_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AppViewModel { AppSettings: { } settings })
        {
            settings.McpServerEnabled = !settings.McpServerEnabled;
        }
    }

    private async void ButtonToggleGmcp_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveTerminal is { } terminal)
        {
            await terminal.ToggleGmcpAsync();
        }
    }
}
