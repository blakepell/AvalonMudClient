using System.ComponentModel;
using System.Net;
using System.Windows;
using Avalon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using Mosaic.UI.Wpf.Logging;

namespace Avalon.Mcp;

/// <summary>
/// Hosts the Model Context Protocol server over streamable HTTP on the loopback interface. The server
/// follows <see cref="AppSettings.McpServerEnabled"/> and <see cref="AppSettings.McpServerPort"/>.
/// </summary>
public sealed partial class McpServerHost : ObservableObject
{
    private const string ServerInstructions =
        "Tools for observing and controlling the running Avalon Mud Client, a client for text-based multiplayer games (MUDs). " +
        "Each saved game is a profile with its own aliases (shortcuts for what the player types), triggers (regular expressions " +
        "that react to lines from the game), variables (named string values used as @Key) and scripts (shared JavaScript functions). " +
        "Tools use the profile whose terminal is showing unless you name one; call list_profiles when unsure. " +
        "Before writing any JavaScript, call get_scripting_guide (topic \"primer\"): scripts are Mosaic JavaScript running on .NET, " +
        "not browser JavaScript, and Avalon's member names are case-sensitive .NET names such as terminal.Send. " +
        "Prefer plain Command aliases and triggers when the action only sends game commands. List existing items before adding " +
        "so you update rather than duplicate them, and confirm with the player before removing anything they didn't clearly ask to remove.";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private AppSettings? _settings;
    private WebApplication? _app;

    /// <summary>
    /// Gets whether the server is listening.
    /// </summary>
    [ObservableProperty]
    public partial bool IsRunning { get; private set; }

    /// <summary>
    /// Gets the endpoint MCP clients connect to, or null when the server is stopped.
    /// </summary>
    [ObservableProperty]
    public partial string? Endpoint { get; private set; }

    /// <summary>
    /// Gets the reason the server last failed to start, or null.
    /// </summary>
    [ObservableProperty]
    public partial string? LastError { get; private set; }

    /// <summary>
    /// Gets the status bar tool tip describing the server state.
    /// </summary>
    public string StatusToolTip => IsRunning
        ? $"MCP server listening at {Endpoint}"
        : LastError is null ? "MCP server is off" : $"MCP server failed to start: {LastError}";

    partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(StatusToolTip));

    partial void OnLastErrorChanged(string? value) => OnPropertyChanged(nameof(StatusToolTip));

    /// <summary>
    /// Follows the settings, starting the server now if it is enabled.
    /// </summary>
    public void Attach(AppSettings settings)
    {
        if (_settings is not null)
        {
            _settings.PropertyChanged -= Settings_PropertyChanged;
        }

        _settings = settings;
        settings.PropertyChanged += Settings_PropertyChanged;
        _ = ApplyAsync();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppSettings.McpServerEnabled) or nameof(AppSettings.McpServerPort))
        {
            _ = ApplyAsync();
        }
    }

    /// <summary>
    /// Starts, stops or restarts the server to match the current settings.
    /// </summary>
    private async Task ApplyAsync()
    {
        var enabled = _settings?.McpServerEnabled == true;
        var port = _settings?.McpServerPort ?? 0;

        await _gate.WaitAsync();
        try
        {
            await StopCoreAsync();
            if (enabled)
            {
                await StartCoreAsync(port);
            }
            else
            {
                LastError = null;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task StartCoreAsync(int port)
    {
        if (port is < IPEndPoint.MinPort + 1 or > IPEndPoint.MaxPort)
        {
            LastError = $"Port {port} is not valid.";
            Logger.LogError(LastError);
            return;
        }

        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
        builder.Services
            .AddMcpServer(options =>
            {
                var version = typeof(McpServerHost).Assembly.GetName().Version?.ToString() ?? "1.0.0";
                options.ServerInfo = new Implementation { Name = "avalon-mud-client", Title = "Avalon Mud Client", Version = version };
                options.ServerInstructions = ServerInstructions;
            })
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<AvalonTools>()
            .WithTools<ProfileTools>()
            .WithTools<AliasTools>()
            .WithTools<TriggerTools>()
            .WithTools<VariableTools>()
            .WithTools<ScriptTools>();

        var app = builder.Build();
        app.MapMcp("/mcp");

        try
        {
            await app.StartAsync();
        }
        catch (Exception ex)
        {
            await app.DisposeAsync();
            LastError = ex.Message;
            Logger.LogError($"The MCP server could not start on port {port}: {ex.Message}");
            return;
        }

        _app = app;
        LastError = null;
        Endpoint = $"http://localhost:{port}/mcp";
        IsRunning = true;
        Logger.LogInfo($"MCP server listening at {Endpoint}.");
    }

    private async Task StopCoreAsync()
    {
        if (_app is not { } app)
        {
            return;
        }

        _app = null;
        IsRunning = false;
        Endpoint = null;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await app.StopAsync(timeout.Token);
        }
        finally
        {
            await app.DisposeAsync();
        }

        Logger.LogInfo("MCP server stopped.");
    }

    /// <summary>
    /// Stops the server when the client exits.
    /// </summary>
    public void Shutdown()
    {
        if (_settings is not null)
        {
            _settings.PropertyChanged -= Settings_PropertyChanged;
            _settings = null;
        }

        if (_app is { } app)
        {
            _app = null;
            // Kestrel's shutdown does not need the dispatcher, so blocking here cannot deadlock.
            Task.Run(async () =>
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                await app.StopAsync(timeout.Token);
                await app.DisposeAsync();
            }).Wait(TimeSpan.FromSeconds(2));
        }
    }

    /// <summary>
    /// Runs a function on the UI thread.
    /// </summary>
    internal static Task<T> OnUiThreadAsync<T>(Func<T> func) =>
        Application.Current.Dispatcher.InvokeAsync(func).Task;
}
