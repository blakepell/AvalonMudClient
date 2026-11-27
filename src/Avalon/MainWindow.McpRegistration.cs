using System.Windows;
using Avalon.Mcp;
using Mosaic.UI.Wpf.Controls;

namespace Avalon;

public partial class MainWindow
{
    private readonly HashSet<McpClient> _busyMcpClients = new();

    private async void AiMenu_OnDropDownOpened(object sender, RoutedEventArgs e) =>
        await Task.WhenAll(UpdateMcpMenuAsync(McpClient.Codex), UpdateMcpMenuAsync(McpClient.ClaudeCode));

    private async void RegisterCodex_OnClick(object sender, RoutedEventArgs e) =>
        await UpdateMcpMenuAsync(McpClient.Codex, register: true);

    private async void UnregisterCodex_OnClick(object sender, RoutedEventArgs e) =>
        await UpdateMcpMenuAsync(McpClient.Codex, register: false);

    private async void RegisterClaude_OnClick(object sender, RoutedEventArgs e) =>
        await UpdateMcpMenuAsync(McpClient.ClaudeCode, register: true);

    private async void UnregisterClaude_OnClick(object sender, RoutedEventArgs e) =>
        await UpdateMcpMenuAsync(McpClient.ClaudeCode, register: false);

    private async Task UpdateMcpMenuAsync(McpClient client, bool? register = null)
    {
        if (!_busyMcpClients.Add(client))
        {
            return;
        }

        var (registerItem, unregisterItem) = client == McpClient.Codex
            ? (RegisterCodexMenuItem, UnregisterCodexMenuItem)
            : (RegisterClaudeMenuItem, UnregisterClaudeMenuItem);
        var clientName = client == McpClient.Codex ? "Codex" : "Claude Code";
        var status = "Checking registration…";
        bool? registered = null;
        registerItem.IsEnabled = unregisterItem.IsEnabled = false;
        registerItem.ToolTip = unregisterItem.ToolTip = status;

        try
        {
            var port = ViewModel.AppSettings.McpServerPort;
            registered = await McpClientRegistration.ConfigureAsync(client, register, port);
            if (register is not null)
            {
                var message = registered == true
                    ? $"Avalon is registered with {clientName}."
                    : $"Avalon was unregistered from {clientName}.";
                ToastManager.Default?.Show("MCP Server", message);
            }
        }
        catch (Exception ex)
        {
            status = ex.Message;
            if (register is not null && IsLoaded)
            {
                MessageBox.Show(this, status, $"MCP Registration for {clientName}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            registerItem.ToolTip = registered is null ? status : registered == true ? "Avalon is already registered." : null;
            unregisterItem.ToolTip = registered is null ? status : registered == false ? "Avalon is currently unregistered." : null;
            registerItem.IsEnabled = registered == false;
            unregisterItem.IsEnabled = registered == true;
            _busyMcpClients.Remove(client);
        }
    }
}
