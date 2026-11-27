using System.ComponentModel;
using Argus.Memory;
using Avalon.Models;
using ModelContextProtocol.Server;

namespace Avalon.Mcp;

/// <summary>
/// The tools the MCP server exposes. Everything touching the UI is marshaled to the dispatcher.
/// </summary>
[McpServerToolType]
public sealed class AvalonTools
{
    private static AppViewModel ViewModel => AppServices.GetRequiredService<AppViewModel>();

    /// <summary>
    /// Gets the text shown in the main window status bar.
    /// </summary>
    [McpServerTool(Name = "get_status_text", ReadOnly = true, Idempotent = true)]
    [Description("Gets the text currently shown on the left side of the Avalon Mud Client's status bar.")]
    public static Task<string> GetStatusText() => McpServerHost.OnUiThreadAsync(() => ViewModel.StatusText);

    /// <summary>
    /// Sets the text shown in the main window status bar.
    /// </summary>
    [McpServerTool(Name = "set_status_text", Idempotent = true)]
    [Description("Sets the text shown on the left side of the Avalon Mud Client's status bar.")]
    public static Task<string> SetStatusText([Description("The text to show in the status bar.")] string text) =>
        McpServerHost.OnUiThreadAsync(() =>
        {
            ViewModel.StatusText = text;
            return $"Status text set to \"{text}\".";
        });
}
